using System.Collections.Generic;
using UnityEngine;
using SketchRenderer.ShaderLibrary;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    [System.Serializable]
    public class SketchCompositionPassData : ISketchRenderPassData<SketchCompositionPassData>
    {
        public enum DebugMode
        {
            NONE, MATERIAL_ALBEDO, MATERIAL_DIRECTION, OUTLINES, LUMINANCE
        }
        [Header("Debug")]
        public DebugMode debugMode = DebugMode.NONE;

        [Header("Composition")] 
        public Color OutlineStrokeColor = Color.black;
        public Color ShadingStrokeColor = Color.black;
        [Range(0f, 1f)] 
        public float MaterialAccumulationStrength;
        public BlendingOperations StrokeBlendMode = BlendingOperations.BLEND_MULTIPLY;
        [Range(0f, 1f)]
        public float BlendStrength = 1f;
    
        [HideInInspector]
        [SerializeField]
        private List<SketchRendererFeatureType> featuresToCompose;
        public List<SketchRendererFeatureType> FeaturesToCompose
        {
            get
            {
                if (featuresToCompose == null)
                    featuresToCompose = new List<SketchRendererFeatureType>();
            
                return featuresToCompose;
            }
            set
            {
                featuresToCompose = value;
            }
        }

        public void CopyFrom(SketchCompositionPassData passData)
        {
            debugMode = passData.debugMode;
            OutlineStrokeColor = passData.OutlineStrokeColor;
            ShadingStrokeColor = passData.ShadingStrokeColor;
            MaterialAccumulationStrength = passData.MaterialAccumulationStrength;
            StrokeBlendMode = passData.StrokeBlendMode;
            BlendStrength = passData.BlendStrength;
            FeaturesToCompose = new List<SketchRendererFeatureType>(passData.FeaturesToCompose);
        }
    
        public bool IsAllPassDataValid()
        {
            return true;
        }

        public SketchCompositionPassData GetPassDataByVolume()
        {
            return this;
        }

        public bool RequiresColorTexture()
        {
            return FeaturesToCompose != null && !FeaturesToCompose.Contains(SketchRendererFeatureType.MATERIAL);
        }
    }
}