using System;
using System.Collections.Generic;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEngine;


namespace SketchRenderer.Runtime.Data
{
    [CreateAssetMenu(fileName = "SketchRendererContext", menuName = SketchRendererData.PackageAssetItemPath + "SketchRendererContext")]
    public class SketchRendererContext : ScriptableObject {
        public event Action OnValidated;
        public event Action OnRedrawSettings;
        public bool IsDirty { get; set; }
        
        [HideInInspector] public bool UseUVsFeature => (UseMaterialFeature && MaterialFeatureData.RequiresTextureCoordinateFeature())
                                                       || (UseLuminanceFeature && LuminanceFeatureData.RequiresTextureCoordinateFeature());
        
        public RenderUVsPassData UVSFeatureData;
        [HideInInspector][SerializeField]
        private bool isUVsFeatureDirty;

        public bool UseMaterialFeature;
        public MaterialSurfacePassData MaterialFeatureData = new ();
        [HideInInspector][SerializeField]
        private bool isMaterialFeatureDirty;

        public bool UseLuminanceFeature;
        public LuminancePassData LuminanceFeatureData = new();
        [HideInInspector][SerializeField]
        private bool isLuminanceFeatureDirty;
        
        public EdgeDetectionPassData EdgeDetectionFeatureData;

        public bool UseSmoothOutlineFeature;
        public AccentedOutlinePassData AccentedOutlineFeatureData = new ();
        public ThicknessDilationPassData ThicknessDilationFeatureData = new ();
        [HideInInspector][SerializeField]
        private bool isSmoothFeatureDirty;

        public bool UseSketchyOutlineFeature;
        public SketchStrokesPassData SketchyOutlineFeatureData = new ();
        [HideInInspector][SerializeField]
        private bool isSketchyFeatureDirty;

        public bool UseCompositorFeature => UseUVsFeature || UseMaterialFeature || UseLuminanceFeature || UseSmoothOutlineFeature || UseSketchyOutlineFeature;
        public SketchCompositionPassData CompositionFeatureData = new ();
        [HideInInspector][SerializeField]
        private bool isCompositionFeatureDirty;

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

        public bool IsFeatureDirty(SketchRendererFeatureType featureType)
        {
            return featureType switch
            {
                SketchRendererFeatureType.UVS => isUVsFeatureDirty,
                SketchRendererFeatureType.OUTLINE_SMOOTH => isSmoothFeatureDirty,
                SketchRendererFeatureType.OUTLINE_SKETCH => isSketchyFeatureDirty,
                SketchRendererFeatureType.LUMINANCE => isLuminanceFeatureDirty,
                SketchRendererFeatureType.MATERIAL => isMaterialFeatureDirty,
                SketchRendererFeatureType.COMPOSITOR => isCompositionFeatureDirty,
                _ => throw new NotImplementedException(),
            };
        }
        
        public void SetFeatureDirty(SketchRendererFeatureType featureType, bool isDirty)
        {
            switch (featureType)
            {
                case SketchRendererFeatureType.UVS:
                    isUVsFeatureDirty = isDirty;
                    break;
                case SketchRendererFeatureType.OUTLINE_SMOOTH:
                    isSmoothFeatureDirty = isDirty;
                    break;
                case SketchRendererFeatureType.OUTLINE_SKETCH:
                    isSketchyFeatureDirty = isDirty;
                    break;
                case SketchRendererFeatureType.LUMINANCE:
                    isLuminanceFeatureDirty = isDirty;
                    break;
                case SketchRendererFeatureType.MATERIAL:
                    isMaterialFeatureDirty = isDirty;
                    break;
                case SketchRendererFeatureType.COMPOSITOR:
                    isCompositionFeatureDirty = isDirty;
                        break;
            }
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

        public void Redraw()
        {
            OnRedrawSettings?.Invoke();
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