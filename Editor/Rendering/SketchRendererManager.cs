using System;
using SketchRenderer.Editor.Utils;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.Rendering
{
    [InitializeOnLoad]
    internal static class SketchRendererManager
    {
        static SketchRendererManager()
        {
            SketchRendererFeatureWrapper.OnFeatureValidated += feature => UpdateFeatureByContext(feature, CurrentRendererContext);
            if (ManagerSettings != null)
            {
                ManagerSettings.OnContextSettingsChanged += UpdateBySettingsChange;
                ManagerSettings.ValidateGlobalSettings();
            }
        }
        
        private static SketchRendererManagerSettings settings;
        internal static SketchRendererManagerSettings ManagerSettings
        {
            get
            {
                if (settings == null)
                {
                    if (!AssetDatabase.AssetPathExists(SketchRendererData.DefaultSketchManagerSettingsPackagePath))
                    {
                        if(SketchAssetCreationWrapper.TryValidateOrCreateAssetPath(SketchRendererData.DefaultPackageAssetDirectoryPath)) {
                            var set = ScriptableObject.CreateInstance<SketchRendererManagerSettings>();
                            AssetDatabase.CreateAsset(set, SketchRendererData.DefaultSketchManagerSettingsPackagePath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }

                    settings = AssetDatabase.LoadAssetAtPath<SketchRendererManagerSettings>(SketchRendererData.DefaultSketchManagerSettingsPackagePath);
                }
                return settings;
            }
        }
        
        private static SketchRendererContext defaultRendererContext;
        internal static SketchRendererContext DefaultRendererContext
        {
            get
            {
                if (defaultRendererContext == null)
                {
                    defaultRendererContext = AssetDatabase.LoadAssetAtPath<SketchRendererContext>(SketchRendererData.DefaultSketchRendererContextPackagePath);
                    ResourceReloader.ReloadAllNullIn(defaultRendererContext, SketchRendererData.PackagePath);
                }
                return defaultRendererContext;
            }
        }

        internal static SketchRendererContext CurrentRendererContext
        {
            get => ManagerSettings.CurrentRendererContext;
            set
            {
                ManagerSettings.CurrentRendererContext = value;
                EditorUtility.SetDirty(ManagerSettings);
                AssetDatabase.SaveAssetIfDirty(ManagerSettings);
            }
        }

        private static SketchResourceAsset resourceAsset;
        internal static SketchResourceAsset ResourceAsset
        {
            get
            {
                if (resourceAsset == null)
                {
                    resourceAsset = AssetDatabase.LoadAssetAtPath<SketchResourceAsset>(SketchRendererData.DefaultSketchResourceAssetPackagePath);
                    ResourceReloader.ReloadAllNullIn(resourceAsset, SketchRendererData.PackagePath);
                }

                return resourceAsset;
            }
        }
        private static readonly SketchRendererFeatureType[] featureTypesInPackage = Enum.GetValues(typeof(SketchRendererFeatureType)) as SketchRendererFeatureType[];
        private static readonly int totalFeatureTypes = featureTypesInPackage.Length;

        internal static void UpdateRendererToDefaultContext()
        {
            CurrentRendererContext = DefaultRendererContext;
            UpdateRendererToCurrentContext();
        }

        internal static void UpdateRendererToCurrentContext()
        {
            if (CurrentRendererContext == null)
                throw new NullReferenceException("[SketchRenderer] Current renderer context is not set.");

            UpdateRendererByContext(CurrentRendererContext);
        }

        internal static void UpdateRendererByContext(SketchRendererContext rendererContext)
        {
            if (rendererContext == null)
                throw new NullReferenceException("[SketchRenderer] Renderer context used to configure is not set.");
            
            Span<(SketchRendererFeatureType Feature, bool Active)> features = stackalloc (SketchRendererFeatureType, bool)[totalFeatureTypes];
            for (int i = 0; i < totalFeatureTypes; i++)
            {
                features[i] = (featureTypesInPackage[i], rendererContext.IsFeaturePresent(featureTypesInPackage[i]));
            }
            
            for (int i = 0; i < totalFeatureTypes; i++)
            {
                if (features[i].Active)
                    SketchRendererFeatureWrapper.ConfigureRendererFeature(features[i].Feature, rendererContext, ResourceAsset);
                else
                    SketchRendererFeatureWrapper.RemoveRendererFeature(features[i].Feature);
            }
        }

        private static void UpdateBySettingsChange()
        {
            if (ManagerSettings.AlwaysUpdateRendererData)
            {
                UpdateRendererToCurrentContext();
                CurrentRendererContext.IsDirty = false;
            }
        }
      
        internal static void ClearRenderer()
        {
            for(int i = totalFeatureTypes - 1; i >= 0; i--)
                SketchRendererFeatureWrapper.RemoveRendererFeature(featureTypesInPackage[i]);
        }
        internal static void UpdateFeatureByCurrentContext(SketchRendererFeatureType featureType)
        {
            UpdateFeatureByContext(featureType, CurrentRendererContext);
        }
        
        internal static void UpdateFeatureByContext(SketchRendererFeatureType featureType, SketchRendererContext rendererContext)
        {
            if (rendererContext == null)
                throw new NullReferenceException("[SketchRenderer] Renderer context used to configure is not set.");

            if (SketchRendererFeatureWrapper.CheckHasActiveFeature(featureType))
            {
                SketchRendererFeatureWrapper.ConfigureRendererFeature(featureType, rendererContext, ResourceAsset);
            }
            else
                Debug.LogWarning($"[SketchRenderer] Current renderer asset does not have {featureType} as an active feature.");
        }
    }
}