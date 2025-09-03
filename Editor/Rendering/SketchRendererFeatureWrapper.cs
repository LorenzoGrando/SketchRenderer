using System;
using System.Collections.Generic;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Editor.Rendering
{
    public class SketchRendererFeatureWrapper
    {
        //Many behaviours here are directly inspired from the following thread on handling the URP Renderer:
        //https://discussions.unity.com/t/urp-adding-a-renderfeature-from-script/842637/4
        
        private static Dictionary<SketchRendererFeatureType, Type> rendererFeatureTypes = new Dictionary<SketchRendererFeatureType, Type>
        {
            {SketchRendererFeatureType.UVS, typeof(RenderUVsRendererFeature)},
            {SketchRendererFeatureType.OUTLINE_SMOOTH, typeof(SmoothOutlineRendererFeature)},
            {SketchRendererFeatureType.OUTLINE_SKETCH, typeof(SketchOutlineRendererFeature)},
            {SketchRendererFeatureType.LUMINANCE, typeof(LuminanceRendererFeature)},
            {SketchRendererFeatureType.MATERIAL, typeof(MaterialSurfaceRendererFeature)},
            {SketchRendererFeatureType.COMPOSITOR, typeof(SketchCompositionRendererFeature)}
        };

        private static SketchRendererFeatureType[] rendererFeatureHierarchyTarget = new SketchRendererFeatureType[]
        {
            SketchRendererFeatureType.UVS, SketchRendererFeatureType.OUTLINE_SMOOTH, SketchRendererFeatureType.OUTLINE_SKETCH, SketchRendererFeatureType.LUMINANCE, SketchRendererFeatureType.MATERIAL, SketchRendererFeatureType.COMPOSITOR
        };

        private static UniversalRendererData GetCurrentRendererData()
        {
            try
            {
                if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
                {
                    UniversalRenderPipelineAsset renderer = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                    UniversalRendererData rendererData = null;
                    for (int i = 0; i < renderer.rendererDataList.Length; i++)
                    {
                        if (renderer.rendererDataList[i] != null && renderer.rendererDataList[i] is UniversalRendererData)
                        {
                            rendererData = renderer.rendererDataList[i] as UniversalRendererData;
                            break;
                        }
                    }
                    
                    if(rendererData == null)
                        throw new NullReferenceException("[SketchRendererDataWrapper] There is no UniversalRendererData in current RendererAsset");

                    return rendererData;
                }

                throw new Exception("[SketchRendererDataWrapper] Active RenderPipeline is not currently supported.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
            return null;
        }
        
        public static bool CheckHasActiveFeature(SketchRendererFeatureType featureType)
        {
            ScriptableRendererFeature feature = GetRendererFeature(rendererFeatureTypes[featureType]);
            return feature != null;
        }

        private static ScriptableRendererFeature GetRendererFeature(Type featureType)
        {
            UniversalRendererData rendererData = GetCurrentRendererData();
            if (rendererData != null)
            {
                for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
                {
                    if(rendererData.rendererFeatures[i] != null && rendererData.rendererFeatures[i].GetType() == featureType)
                        return rendererData.rendererFeatures[i];
                }
            }
            return null;
        }

        private static ISketchRendererFeature GetSketchRendererFeature(Type featureType)
        {
            ScriptableRendererFeature feature = GetRendererFeature(featureType);
            ISketchRendererFeature sketchRendererFeature = feature as ISketchRendererFeature;
            if(sketchRendererFeature != null)
                return sketchRendererFeature;
            else
                throw new Exception("[SketchRendererDataWrapper] Renderer feature does not implement ISketchRendererFeature interface.");
        }
        
        public static void AddRendererFeature(SketchRendererFeatureType featureType)
        {
            if(CheckHasActiveFeature(featureType))
                return;
            
            UniversalRendererData rendererData = GetCurrentRendererData();
            if (rendererData != null)
            {
                ScriptableRendererFeature rendererFeature = GetNewRendererFeatureAsset(featureType);
                int preferredHierarchySlot = GetIndexOfNextHierarchyFeature(featureType);
                if(rendererFeature != null)
                    AddFeatureToData(rendererData, rendererFeature, preferredHierarchySlot);
            }
        }
        
        private static void AddFeatureToData(ScriptableRendererData data, ScriptableRendererFeature feature, int targetHierarchyIndex)
        {
            var serializedObject = new SerializedObject(data);

            var renderFeaturesProp = serializedObject.FindProperty("m_RendererFeatures");
            var renderFeaturesMapProp = serializedObject.FindProperty("m_RendererFeatureMap");

            serializedObject.Update();
            
            if (EditorUtility.IsPersistent(data))
                AssetDatabase.AddObjectToAsset(feature, data);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out var guid, out long localId);
            
            if (targetHierarchyIndex > renderFeaturesProp.arraySize || renderFeaturesProp.arraySize == 0)
                renderFeaturesProp.arraySize++;
            else
                renderFeaturesProp.InsertArrayElementAtIndex(targetHierarchyIndex);
            var componentProp = renderFeaturesProp.GetArrayElementAtIndex(targetHierarchyIndex);
            componentProp.objectReferenceValue = feature;
            
            if (targetHierarchyIndex > renderFeaturesMapProp.arraySize || renderFeaturesMapProp.arraySize == 0)
                renderFeaturesMapProp.arraySize++;
            else
                renderFeaturesMapProp.InsertArrayElementAtIndex(targetHierarchyIndex);
            var guidProp = renderFeaturesMapProp.GetArrayElementAtIndex(targetHierarchyIndex);
            guidProp.longValue = localId;
            
            if (EditorUtility.IsPersistent(data))
            {
                AssetDatabase.SaveAssetIfDirty(data);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(data), ImportAssetOptions.ForceUpdate);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static int GetIndexOfNextHierarchyFeature(SketchRendererFeatureType featureType)
        {
            for (int i = 0; i < rendererFeatureHierarchyTarget.Length; i++)
            {
                if (rendererFeatureHierarchyTarget[i] != featureType)
                    continue;
                
                UniversalRendererData data = GetCurrentRendererData();
                
                
                for (int hierarchyIndex = i + 1; hierarchyIndex < rendererFeatureHierarchyTarget.Length; hierarchyIndex++)
                {
                    SketchRendererFeatureType targetFeatureType = rendererFeatureHierarchyTarget[hierarchyIndex];
                    ScriptableRendererFeature feature = GetRendererFeature(rendererFeatureTypes[targetFeatureType]);

                    if (feature != null)
                    {
                        for (int rendererListIndex = 0; rendererListIndex < data.rendererFeatures.Count; rendererListIndex++)
                        {
                            if(data.rendererFeatures[rendererListIndex] == feature)
                                return rendererListIndex;
                        }
                    }
                }
                
                //If process fails to find any succeeding feature, return the index one bigger than current data length.
                return data.rendererFeatures.Count;
            }

            return 0;
        }
        
        public static void ConfigureRendererFeature(SketchRendererFeatureType featureType, SketchRendererContext rendererContext, SketchResourceAsset resourceAsset)
        {
            if(!CheckHasActiveFeature(featureType))
                AddRendererFeature(featureType);
            
            UpdateRendererFeatureByContext(featureType, rendererContext, resourceAsset);
            UniversalRendererData rendererData = GetCurrentRendererData();
            if (EditorUtility.IsPersistent(rendererData))
            {
                AssetDatabase.SaveAssetIfDirty(rendererData);
            }
        }

        private static void UpdateRendererFeatureByContext(SketchRendererFeatureType featureType, SketchRendererContext context, SketchResourceAsset resources)
        {
            if(context == null)
                throw new ArgumentNullException("[SketchRendererDataWrapper] Missing SketchRendererContext in attempt to configure current renderer.");
            
            ISketchRendererFeature sketchFeature = GetSketchRendererFeature(rendererFeatureTypes[featureType]);
            if (sketchFeature != null)
            {
                sketchFeature.ConfigureByContext(context, resources);
            }
        }
        
        public static void RemoveRendererFeature(SketchRendererFeatureType featureType)
        {
            if(!CheckHasActiveFeature(featureType))
                return;
            
            UniversalRendererData rendererData = GetCurrentRendererData();
            if (rendererData != null)
            {
                ScriptableRendererFeature rendererFeature = GetRendererFeature(rendererFeatureTypes[featureType]);
                if(rendererFeature != null)
                    RemoveFeatureFromData(rendererData, rendererFeature);
            }
        }

        private static void RemoveFeatureFromData(ScriptableRendererData data, ScriptableRendererFeature feature)
        {
            var serializedObject = new SerializedObject(data);

            var renderFeaturesProp = serializedObject.FindProperty("m_RendererFeatures");
            var renderFeaturesMapProp = serializedObject.FindProperty("m_RendererFeatureMap");
            
            serializedObject.Update();
            int foundIndex = -1;
            for (int i = 0; i < renderFeaturesProp.arraySize; i++)
            {
                if (renderFeaturesProp.GetArrayElementAtIndex(i).objectReferenceValue == feature)
                {
                    foundIndex = i;
                    break;
                }
            }
            if (foundIndex > -1)
            {
                renderFeaturesProp.DeleteArrayElementAtIndex(foundIndex);
                renderFeaturesMapProp.DeleteArrayElementAtIndex(foundIndex);
                
                serializedObject.ApplyModifiedProperties();

                if (EditorUtility.IsPersistent(data))
                {
                    AssetDatabase.RemoveObjectFromAsset(feature);
                    //Ensure data is dirty from removal
                    data.SetDirty();
                    AssetDatabase.SaveAssetIfDirty(data);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(data), ImportAssetOptions.ForceUpdate);
                    UnityEditor.Editor.DestroyImmediate(feature);
                }
            }
        }

        private static ScriptableRendererFeature GetNewRendererFeatureAsset(SketchRendererFeatureType featureType)
        {
            ScriptableRendererFeature feature = null;
            switch (featureType)
            {
                case SketchRendererFeatureType.UVS:
                    feature = ScriptableObject.CreateInstance<RenderUVsRendererFeature>();
                    feature.name = nameof(RenderUVsRendererFeature);
                    break;
                case SketchRendererFeatureType.OUTLINE_SMOOTH:
                    feature = ScriptableObject.CreateInstance<SmoothOutlineRendererFeature>();
                    feature.name = nameof(SmoothOutlineRendererFeature);
                    break;
                case SketchRendererFeatureType.OUTLINE_SKETCH:
                    feature = ScriptableObject.CreateInstance<SketchOutlineRendererFeature>();
                    feature.name = nameof(SketchOutlineRendererFeature);
                    break;
                case SketchRendererFeatureType.LUMINANCE:
                    feature = ScriptableObject.CreateInstance<LuminanceRendererFeature>();
                    feature.name = nameof(LuminanceRendererFeature);
                    break;
                case SketchRendererFeatureType.MATERIAL:
                    feature = ScriptableObject.CreateInstance<MaterialSurfaceRendererFeature>();
                    feature.name = nameof(MaterialSurfaceRendererFeature);
                    break;
                case SketchRendererFeatureType.COMPOSITOR:
                    feature = ScriptableObject.CreateInstance<SketchCompositionRendererFeature>();
                    feature.name = nameof(SketchCompositionRendererFeature);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(featureType), featureType, null);
            }
            
            return feature;
        }
    }
}