using System;
using System.Collections.Generic;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace SketchRenderer.Editor.Rendering
{
    [InitializeOnLoad]
    internal static class SketchRendererFeatureWrapper
    {
        //Many behaviours here are directly inspired from the following thread on handling the URP Renderer:
        //https://discussions.unity.com/t/urp-adding-a-renderfeature-from-script/842637/4
        
        internal static event Action<SketchRendererFeatureType> OnFeatureValidated;

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

        internal static SketchRendererContext internalRendererContext;
        private static RenderUVsRendererFeature cachedUvsRendererFeature;
        private static SmoothOutlineRendererFeature cachedSmoothOutlineRendererFeature;
        private static SketchOutlineRendererFeature cachedSketchOutlineRendererFeature;
        private static MaterialSurfaceRendererFeature cachedMaterialSurfaceRendererFeature;
        private static LuminanceRendererFeature cachedLuminanceRendererFeature;
        private static SketchCompositionRendererFeature cachedSketchCompositionRendererFeature;
        
        
        #region Renderer Data Feature Validation
        static SketchRendererFeatureWrapper()
        {
            AssemblyReloadEvents.beforeAssemblyReload += DisposeInternalState;
            AssemblyReloadEvents.afterAssemblyReload += PrepareInternalState;
            
            Selection.selectionChanged += ShouldTrackRendererFeatures;
            ShouldTrackRendererFeatures();
        }

        private static void PrepareInternalState()
        {
            if (lastAcquiredRendererData == null)
                GetCurrentRendererData();

            if (lastAcquiredRendererData != null)
            {
                if (internalRendererContext == null)
                {
                    internalRendererContext = ScriptableObject.CreateInstance<SketchRendererContext>();
                    internalRendererContext.hideFlags = HideFlags.HideAndDontSave;
                    internalRendererContext.CompositionFeatureData = new SketchCompositionPassData();
                }
                UpdateInternalContext();
            }
        }

        private static void DisposeInternalState()
        {
            if(internalRendererContext != null)
                Object.DestroyImmediate(internalRendererContext);
            DisposeCachedFeatures();
        }

        private static void DisposeCachedFeatures()
        {
            cachedUvsRendererFeature = null;
            cachedMaterialSurfaceRendererFeature = null;
            cachedLuminanceRendererFeature = null;
            cachedSketchCompositionRendererFeature = null;
            cachedSmoothOutlineRendererFeature = null;
            cachedSketchCompositionRendererFeature = null;
        }
        
        private static bool trackingRendererFeatures = false;
        private static void ShouldTrackRendererFeatures()
        {
            if (lastAcquiredRendererData == null)
                GetCurrentRendererData();
            if (Selection.activeObject != lastAcquiredRendererData)
            {
                if(trackingRendererFeatures)
                    EditorApplication.update -= TrackRendererFeatures;
                trackingRendererFeatures = false;
                return;
            }
            
            if (!trackingRendererFeatures)
            {
                trackingRendererFeatures = true;
                EditorApplication.update += TrackRendererFeatures;
            }
        }

        private static void TrackRendererFeatures()
        {
            Span<(SketchRendererFeatureType feature, bool present, int index)> presentFeatures = stackalloc (SketchRendererFeatureType, bool, int)[rendererFeatureHierarchyTarget.Length];
            for (int i = 0; i < rendererFeatureHierarchyTarget.Length; i++)
                presentFeatures[i] = new (rendererFeatureHierarchyTarget[i], false, -1);
            GetFeatureTypesInRenderer(ref presentFeatures, lastAcquiredRendererData);
            bool isDifferent = false;
            for (int i = 0; i < presentFeatures.Length; i++)
            {
                if (internalRendererContext.IsFeaturePresent(presentFeatures[i].feature) != presentFeatures[i].present)
                {
                    isDifferent = true;
                    break;
                }
            }

            if (isDifferent)
            {
                int compositorIndex = -1;
                for (int i = 0; i < presentFeatures.Length; i++)
                {
                    if(!presentFeatures[i].present)
                        continue;
                    
                    if (presentFeatures[i].feature == SketchRendererFeatureType.COMPOSITOR)
                    {
                        compositorIndex = presentFeatures[i].index;
                        break;
                    }
                }

                Span<(SketchRendererFeatureType feature, bool add)> overshootFeatures = stackalloc (SketchRendererFeatureType, bool)[rendererFeatureHierarchyTarget.Length];
                int overshotCount = 0;
                if (compositorIndex > -1)
                {
                    for (int i = 0; i < presentFeatures.Length; i++)
                    {
                        if(!presentFeatures[i].present)
                            continue;

                        if (presentFeatures[i].feature != SketchRendererFeatureType.COMPOSITOR && presentFeatures[i].index > compositorIndex)
                        {
                            overshootFeatures[overshotCount] = new (presentFeatures[i].feature, true);
                            RemoveRendererFeature(presentFeatures[i].feature);
                        }
                    }
                }
                
                UpdateInternalContext();
                ValidateOvershoots(ref overshootFeatures);
                ValidateDependencies(internalRendererContext);
                ActiveEditorTracker.sharedTracker.ForceRebuild();
                overshootFeatures.Clear();
            }
            ValidateHierarchy(internalRendererContext, lastAcquiredRendererData);
            presentFeatures.Clear();
        }

        private static void UpdateInternalContext()
        {
            if(internalRendererContext == null)
                return;

            for (int i = 0; i < rendererFeatureHierarchyTarget.Length; i++)
            {
                SketchRendererFeatureType featureType = rendererFeatureHierarchyTarget[i];
                ScriptableRendererFeature feature = GetRendererFeature(rendererFeatureTypes[featureType]);
                ApplyToInternalContextByFeature(featureType, feature);
            }
            internalRendererContext.ConfigureSettings();
        }

        private static void ApplyToInternalContextByFeature(SketchRendererFeatureType featureType, ScriptableRendererFeature feature)
        {
            if(internalRendererContext == null)
                return;
            
            bool hasFeature = feature != null;
            switch (featureType)
            {
                case SketchRendererFeatureType.UVS:
                    if (hasFeature)
                    {
                        RenderUVsRendererFeature typedFeatured = (RenderUVsRendererFeature)feature;
                        internalRendererContext.UVSFeatureData = typedFeatured.UvsPassData;
                        cachedUvsRendererFeature = typedFeatured;
                    }
                    else
                        cachedUvsRendererFeature = null;

                    break;
                case SketchRendererFeatureType.OUTLINE_SMOOTH:
                    if (hasFeature)
                    {
                        internalRendererContext.UseSmoothOutlineFeature = true;
                        SmoothOutlineRendererFeature smoothOutlineFeature = (SmoothOutlineRendererFeature)feature;
                        internalRendererContext.EdgeDetectionFeatureData = smoothOutlineFeature.EdgeDetectionPassData;
                        internalRendererContext.AccentedOutlineFeatureData =
                            smoothOutlineFeature.AccentedOutlinePassData;
                        internalRendererContext.ThicknessDilationFeatureData = smoothOutlineFeature.ThicknessPassData;
                        cachedSmoothOutlineRendererFeature = smoothOutlineFeature;
                    }
                    else
                    {
                        internalRendererContext.UseSmoothOutlineFeature = false;
                        cachedSmoothOutlineRendererFeature = null;
                    }

                    break;
                case SketchRendererFeatureType.OUTLINE_SKETCH:
                    if (hasFeature)
                    {
                        internalRendererContext.UseSketchyOutlineFeature = true;
                        SketchOutlineRendererFeature sketchOutlineRendererFeature = (SketchOutlineRendererFeature)feature;
                        internalRendererContext.EdgeDetectionFeatureData = sketchOutlineRendererFeature.EdgeDetectionPassData;
                        internalRendererContext.SketchyOutlineFeatureData = sketchOutlineRendererFeature.SketchStrokesPassData;
                        cachedSketchOutlineRendererFeature = sketchOutlineRendererFeature;
                    }
                    else
                    {
                        internalRendererContext.UseSketchyOutlineFeature = false;
                        cachedSketchOutlineRendererFeature = null;
                    }

                    break;
                case SketchRendererFeatureType.LUMINANCE:
                    if (hasFeature)
                    {
                        internalRendererContext.UseLuminanceFeature = true;
                        LuminanceRendererFeature typedFeature = (LuminanceRendererFeature)feature;
                        internalRendererContext.LuminanceFeatureData = typedFeature.LuminanceData;
                    }
                    else
                    {
                        internalRendererContext.UseLuminanceFeature = false;
                        cachedLuminanceRendererFeature = null;
                    }

                    break;
                case SketchRendererFeatureType.MATERIAL:
                    if (hasFeature)
                    {
                        internalRendererContext.UseMaterialFeature = true;
                        MaterialSurfaceRendererFeature typedFeature = (MaterialSurfaceRendererFeature)feature;
                        internalRendererContext.MaterialFeatureData = typedFeature.MaterialData;
                        cachedMaterialSurfaceRendererFeature = typedFeature;
                    }
                    else
                    {
                        internalRendererContext.UseMaterialFeature = false;
                        cachedMaterialSurfaceRendererFeature = null;
                    }

                    break;
                case SketchRendererFeatureType.COMPOSITOR:
                    if (hasFeature)
                    {
                        SketchCompositionRendererFeature typedFeature = (SketchCompositionRendererFeature)feature;
                        internalRendererContext.CompositionFeatureData = typedFeature.CompositionPassData;
                        cachedSketchCompositionRendererFeature = typedFeature;
                    }
                    else
                    {
                        internalRendererContext.CompositionFeatureData = null;
                        cachedSketchCompositionRendererFeature = null;
                    }

                    break;
            }
        }

        private static void ValidateDependencies(SketchRendererContext context)
        {
            //Enforce some dependencies to always be present in hierarchy order
            if (context.UseUVsFeature)
            {
                if (!CheckHasActiveFeature(SketchRendererFeatureType.UVS))
                {
                    AddRendererFeature(SketchRendererFeatureType.UVS);
                    ScriptableRendererFeature feature = GetRendererFeature(rendererFeatureTypes[SketchRendererFeatureType.UVS]);
                    ApplyToInternalContextByFeature(SketchRendererFeatureType.UVS, feature);
                    OnFeatureValidated?.Invoke(SketchRendererFeatureType.UVS);
                    Debug.LogError("[SketchRenderer] Attempting to manually remove UVs feature when at least one active other sketch feature uses object space texture projection, which requires an output for this feature.");
                }
            }
            
            //Never allow two outline features
            if (context.UseSmoothOutlineFeature && context.UseSketchyOutlineFeature)
            {
                if (CheckHasActiveFeature(SketchRendererFeatureType.OUTLINE_SMOOTH) && CheckHasActiveFeature(SketchRendererFeatureType.OUTLINE_SKETCH))
                {
                    RemoveRendererFeature(SketchRendererFeatureType.OUTLINE_SMOOTH);
                    RemoveRendererFeature(SketchRendererFeatureType.OUTLINE_SKETCH);
                    ApplyToInternalContextByFeature(SketchRendererFeatureType.OUTLINE_SMOOTH, null);
                    ApplyToInternalContextByFeature(SketchRendererFeatureType.OUTLINE_SKETCH, null);
                    Debug.LogWarning("[SketchRenderer] Attempted to manually add two different sketch outline features. Currently only one active outline style can be present in the renderer.");
                }
            }

            if (context.UseCompositorFeature)
            {
                if (!CheckHasActiveFeature(SketchRendererFeatureType.COMPOSITOR))
                {
                    AddRendererFeature(SketchRendererFeatureType.COMPOSITOR);
                    ScriptableRendererFeature feature = GetRendererFeature(rendererFeatureTypes[SketchRendererFeatureType.COMPOSITOR]);
                    ApplyToInternalContextByFeature(SketchRendererFeatureType.COMPOSITOR, feature);
                    OnFeatureValidated?.Invoke(SketchRendererFeatureType.COMPOSITOR);
                    Debug.LogError("[SketchRenderer] Attempting to manually remove or move compositor feature. It is required after all sketch features if any sketch feature is present in the renderer data.");
                }
            }
            
            context.ConfigureSettings();
        }

        private static void ValidateOvershoots(ref Span<(SketchRendererFeatureType, bool)> overshotFeatures)
        {
            for (int i = 0; i < overshotFeatures.Length; i++)
            {
                if(!overshotFeatures[i].Item2)
                    return;

                if (!CheckHasActiveFeature(overshotFeatures[i].Item1))
                {
                    AddRendererFeature(overshotFeatures[i].Item1);
                    ScriptableRendererFeature feature = GetRendererFeature(rendererFeatureTypes[overshotFeatures[i].Item1]);
                    ApplyToInternalContextByFeature(overshotFeatures[i].Item1, feature);
                    OnFeatureValidated?.Invoke(overshotFeatures[i].Item1);
                }
            }
        }

        private static void ValidateHierarchy(SketchRendererContext context, UniversalRendererData rendererData)
        {
            if (rendererData == null)
                rendererData = GetCurrentRendererData();
            if (rendererData == null)
                return;
            //While there is an ideal hierarchy when adding passes, the only real reliance is on the compositor being the last in regard to all other sketch passes.
            if (context.UseCompositorFeature)
            {
                int targetIndex = GetIndexOfPreviousHierarchyFeature(SketchRendererFeatureType.COMPOSITOR);
                Type targetType = rendererFeatureTypes[SketchRendererFeatureType.COMPOSITOR];
                for(int i = 0; i < rendererData.rendererFeatures.Count; i++)
                {
                    if (rendererData.rendererFeatures[i].GetType() == targetType && i < targetIndex)
                    {
                        //Remove it here, and have the check detect this the next frame.
                        RemoveRendererFeature(SketchRendererFeatureType.COMPOSITOR);
                    } 
                }
            }
        }
        
        internal static UniversalRendererData lastAcquiredRendererData;

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
                  
                    lastAcquiredRendererData = rendererData;

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
        
        #endregion
        
        #region Renderer Feature Management

        internal static void CacheRendererFeature(ScriptableRendererFeature feature, SketchRendererFeatureType featureType)
        {
            switch (featureType)
            {
                case SketchRendererFeatureType.UVS:
                    cachedUvsRendererFeature = (RenderUVsRendererFeature)feature;
                    break;
                case SketchRendererFeatureType.OUTLINE_SMOOTH:
                    cachedSmoothOutlineRendererFeature = (SmoothOutlineRendererFeature)feature;
                    break;
                case SketchRendererFeatureType.OUTLINE_SKETCH:
                    cachedSketchCompositionRendererFeature = (SketchCompositionRendererFeature)feature;
                    break;
                case SketchRendererFeatureType.LUMINANCE:
                    cachedLuminanceRendererFeature = (LuminanceRendererFeature)feature;
                    break;
                case SketchRendererFeatureType.MATERIAL:
                    cachedMaterialSurfaceRendererFeature = (MaterialSurfaceRendererFeature)feature;
                    break;
                case SketchRendererFeatureType.COMPOSITOR:
                    cachedSketchCompositionRendererFeature = (SketchCompositionRendererFeature)feature;
                    break;
            }
        }
        
        internal static bool CheckHasActiveFeature(SketchRendererFeatureType featureType)
        {
            ScriptableRendererFeature feature = GetCachedRendererFeature(featureType);
            return feature != null;
        }
      
        private static void GetFeatureTypesInRenderer(ref Span<(SketchRendererFeatureType featureType, bool present, int index)> presentFeatures, UniversalRendererData rendererData = null)
        {
            if(rendererData == null)
                rendererData = GetCurrentRendererData();
            if (rendererData != null)
            {
                int found = 0;
                foreach (KeyValuePair<SketchRendererFeatureType, Type> kvp in rendererFeatureTypes)
                {
                    (SketchRendererFeatureType featureType, bool present, int index) feature = new (kvp.Key, false, -1);
                    for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
                    {
                        if(rendererData.rendererFeatures[i] == null)
                            continue;
                        
                        if (rendererData.rendererFeatures[i].GetType() == kvp.Value)
                        {
                            feature.present = true;
                            feature.index = i;
                            break;
                        }
                    }
                    presentFeatures[found] = feature;
                    found++;
                }
            }
        }

        private static ScriptableRendererFeature GetCachedRendererFeature(SketchRendererFeatureType featureType)
        {
            ScriptableRendererFeature feature = null;
            switch (featureType)
            {
                case SketchRendererFeatureType.UVS:
                    feature = cachedUvsRendererFeature;
                    break;

                case SketchRendererFeatureType.OUTLINE_SMOOTH:
                    feature = cachedSmoothOutlineRendererFeature;
                    break;

                case SketchRendererFeatureType.OUTLINE_SKETCH:
                    feature = cachedSketchCompositionRendererFeature;
                    break;

                case SketchRendererFeatureType.LUMINANCE:
                    feature = cachedLuminanceRendererFeature;
                    break;

                case SketchRendererFeatureType.MATERIAL:
                    feature = cachedMaterialSurfaceRendererFeature;
                    break;

                case SketchRendererFeatureType.COMPOSITOR:
                    feature = cachedSketchCompositionRendererFeature;
                    break;
            }

            if (feature == null)
            {
                feature = GetRendererFeature(rendererFeatureTypes[featureType]);
                if(feature != null)
                    CacheRendererFeature(feature, featureType);
            }

            return feature;
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

        private static ISketchRendererFeature GetCachedSketchRendererFeature(SketchRendererFeatureType featureType)
        {
            ScriptableRendererFeature feature = GetCachedRendererFeature(featureType);
            
            ISketchRendererFeature sketchRendererFeature = feature as ISketchRendererFeature;
            if(sketchRendererFeature != null)
                return sketchRendererFeature;
            else
                throw new Exception("[SketchRendererDataWrapper] Renderer feature does not implement ISketchRendererFeature interface.");
        }
        
        internal static void AddRendererFeature(SketchRendererFeatureType featureType)
        {
            if(CheckHasActiveFeature(featureType))
                return;
            
            UniversalRendererData rendererData = GetCurrentRendererData();
            if (rendererData != null)
            {
                ScriptableRendererFeature rendererFeature = GetNewRendererFeatureAsset(featureType);
                int preferredHierarchySlot = featureType == SketchRendererFeatureType.COMPOSITOR ? GetIndexOfPreviousHierarchyFeature(featureType) : GetIndexOfNextHierarchyFeature(featureType);
                if (rendererFeature != null)
                {
                    AddFeatureToData(rendererData, rendererFeature, preferredHierarchySlot);
                    CacheRendererFeature(rendererFeature, featureType);
                }
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
        
        private static int GetIndexOfPreviousHierarchyFeature(SketchRendererFeatureType featureType)
        {
            for (int i = 0; i < rendererFeatureHierarchyTarget.Length; i++)
            {
                if (rendererFeatureHierarchyTarget[i] != featureType)
                    continue;
                
                UniversalRendererData data = GetCurrentRendererData();
                
                
                for (int hierarchyIndex = i - 1; hierarchyIndex > 0; hierarchyIndex--)
                {
                    SketchRendererFeatureType targetFeatureType = rendererFeatureHierarchyTarget[hierarchyIndex];
                    ScriptableRendererFeature feature = GetRendererFeature(rendererFeatureTypes[targetFeatureType]);

                    if (feature != null)
                    {
                        for (int rendererListIndex = 0; rendererListIndex < data.rendererFeatures.Count; rendererListIndex++)
                        {
                            if(data.rendererFeatures[rendererListIndex] == feature)
                                return rendererListIndex + 1;
                        }
                    }
                }
            }

            return 0;
        }
      
        internal static void ConfigureRendererFeature(SketchRendererFeatureType featureType, SketchRendererContext rendererContext, SketchResourceAsset resourceAsset)
        {
            if (!CheckHasActiveFeature(featureType))
                AddRendererFeature(featureType);

            UpdateRendererFeatureByContext(featureType, rendererContext, resourceAsset);
            UniversalRendererData rendererData = GetCurrentRendererData();
            if (EditorUtility.IsPersistent(rendererData))
            {
                EditorUtility.SetDirty(rendererData);
                AssetDatabase.SaveAssetIfDirty(rendererData);
                AssetDatabase.Refresh();
            }
        }

        private static void UpdateRendererFeatureByContext(SketchRendererFeatureType featureType, SketchRendererContext context, SketchResourceAsset resources)
        {
            if(context == null)
                throw new ArgumentNullException("[SketchRendererDataWrapper] Missing SketchRendererContext in attempt to configure current renderer.");
            
            ISketchRendererFeature sketchFeature = GetCachedSketchRendererFeature(featureType);
            if (sketchFeature != null)
            {
                sketchFeature.ConfigureByContext(context, resources);
            }
        }
        
        internal static void RemoveRendererFeature(SketchRendererFeatureType featureType)
        {
            if(!CheckHasActiveFeature(featureType))
                return;
            
            UniversalRendererData rendererData = GetCurrentRendererData();
            if (rendererData != null)
            {
                ScriptableRendererFeature rendererFeature = GetCachedRendererFeature(featureType);
                if(rendererFeature != null)
                    RemoveFeatureFromData(rendererData, rendererFeature);
                CacheRendererFeature(null, featureType);
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
        
        #endregion
    }
}