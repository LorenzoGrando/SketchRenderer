using System;
using System.Collections.Generic;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEngine;


namespace SketchRenderer.Runtime.Data
{
    [CreateAssetMenu(fileName = "SketchRendererContext", menuName = SketchRendererData.PackageAssetItemPath + "SketchRendererContext")]
    public class SketchRendererContext : ScriptableObject
    {
        [HideInInspector] public bool UseUVsFeature => (UseMaterialFeature && MaterialFeatureData.RequiresTextureCoordinateFeature())
                                                       || (UseLuminanceFeature && LuminanceFeatureData.RequiresTextureCoordinateFeature());
        
        public RenderUVsPassData UVSFeatureData;

        public bool UseMaterialFeature;
        public MaterialSurfacePassData MaterialFeatureData;

        public bool UseLuminanceFeature;
        public LuminancePassData LuminanceFeatureData;

        [HideInInspector] public bool UseEdgeDetectionFeature => UseSmoothOutlineFeature || UseSketchyOutlineFeature;
        public EdgeDetectionPassData EdgeDetectionFeatureData;

        public bool UseSmoothOutlineFeature;
        public AccentedOutlinePassData AccentedOutlineFeatureData;
        public ThicknessDilationPassData ThicknessDilationFeatureData;

        public bool UseSketchyOutlineFeature;
        public SketchStrokesPassData SketchyOutlineFeatureData;

        public SketchCompositionPassData CompositionFeatureData;

        public bool IsFeaturePresent(SketchRendererFeatureType featureType)
        {
            return featureType switch
            {
                SketchRendererFeatureType.UVS => UseUVsFeature,
                SketchRendererFeatureType.OUTLINE_SMOOTH => UseSmoothOutlineFeature,
                SketchRendererFeatureType.OUTLINE_SKETCH => UseSketchyOutlineFeature,
                SketchRendererFeatureType.LUMINANCE => UseLuminanceFeature,
                SketchRendererFeatureType.MATERIAL => UseMaterialFeature,
                SketchRendererFeatureType.COMPOSITOR => true,
                _ => throw new NotImplementedException(),
            };
        }

        public void ConfigureSettings()
        {
            List<SketchRendererFeatureType> featuresInContext = new List<SketchRendererFeatureType>();
            SketchRendererFeatureType[] features = Enum.GetValues(typeof(SketchRendererFeatureType)) as SketchRendererFeatureType[];
            for (int i = 0; i < features.Length; i++)
            {
                if (IsFeaturePresent(features[i]))
                    featuresInContext.Add(features[i]);
            }

            CompositionFeatureData.FeaturesToCompose = featuresInContext;
            AccentedOutlineFeatureData.ForceRebake = true;
        }

        public void OnValidate()
        {
            ConfigureSettings();
        }
    }
}