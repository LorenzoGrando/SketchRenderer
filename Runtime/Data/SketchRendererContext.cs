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
        public event Action OnValidated;
        public bool IsDirty { get; set; }
        
        [HideInInspector] public bool UseUVsFeature => (UseMaterialFeature && MaterialFeatureData.RequiresTextureCoordinateFeature())
                                                       || (UseLuminanceFeature && LuminanceFeatureData.RequiresTextureCoordinateFeature());
        
        public RenderUVsPassData UVSFeatureData;

        public bool UseMaterialFeature;
        public MaterialSurfacePassData MaterialFeatureData = new ();

        public bool UseLuminanceFeature;
        public LuminancePassData LuminanceFeatureData = new();
        
        public EdgeDetectionPassData EdgeDetectionFeatureData;

        public bool UseSmoothOutlineFeature;
        public AccentedOutlinePassData AccentedOutlineFeatureData = new ();
        public ThicknessDilationPassData ThicknessDilationFeatureData = new ();

        public bool UseSketchyOutlineFeature;
        public SketchStrokesPassData SketchyOutlineFeatureData = new ();

        public bool UseCompositorFeature => UseUVsFeature || UseMaterialFeature || UseLuminanceFeature || UseSmoothOutlineFeature || UseSketchyOutlineFeature;
        public SketchCompositionPassData CompositionFeatureData = new ();

        public bool IsFeaturePresent(SketchRendererFeatureType featureType)
        {
            return featureType switch
            {
                SketchRendererFeatureType.UVS => UseUVsFeature,
                SketchRendererFeatureType.OUTLINE_SMOOTH => UseSmoothOutlineFeature,
                SketchRendererFeatureType.OUTLINE_SKETCH => UseSketchyOutlineFeature,
                SketchRendererFeatureType.LUMINANCE => UseLuminanceFeature,
                SketchRendererFeatureType.MATERIAL => UseMaterialFeature,
                SketchRendererFeatureType.COMPOSITOR => UseCompositorFeature,
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

            if(CompositionFeatureData != null)
                CompositionFeatureData.FeaturesToCompose = featuresInContext;
            if(AccentedOutlineFeatureData != null)
                AccentedOutlineFeatureData.ForceRebake = true;
        }

        public void OnValidate()
        {
            IsDirty = true;
            ConfigureSettings();
            OnValidated?.Invoke();
        }

        public void OnEnable()
        {
            if(IsDirty)
                ConfigureSettings();
        }
    }
}