using System;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.Rendering
{
    public static class SketchRendererManager
    {
        private static SketchRendererContext currentRendererContext;
        public static SketchRendererContext CurrentRendererContext
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

        public static SketchResourceAsset ResourceAsset
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
            
        public static bool IsSketchRendererPresent()
        {
            return SketchRendererFeatureWrapper.CheckHasActiveFeature(SketchRendererFeatureType.COMPOSITOR);
        }

        public static void UpdateRendererToCurrentContext()
        {
            if (CurrentRendererContext == null)
                throw new NullReferenceException("[SketchRenderer] Current renderer context is not set.");

            UpdateRendererByContext(CurrentRendererContext);
        }

        private static void UpdateRendererByContext(SketchRendererContext rendererContext)
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
    }
}