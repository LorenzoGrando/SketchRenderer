using System;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.Rendering
{
    internal static class SketchRendererManager
    {
        private static SketchRendererContext currentRendererContext;
        internal static SketchRendererContext CurrentRendererContext
        {
            get
            {
                if (currentRendererContext == null)
                {
                    currentRendererContext = AssetDatabase.LoadAssetAtPath<SketchRendererContext>(SketchRendererData.DefaultSketchRendererContextPackagePath);
                    ResourceReloader.ReloadAllNullIn(currentRendererContext, SketchRendererData.PackagePath);
                }

                return currentRendererContext;
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