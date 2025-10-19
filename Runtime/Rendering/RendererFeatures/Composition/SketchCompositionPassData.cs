using System.Collections.Generic;
using SketchRenderer.Runtime.Rendering.Volume;
using UnityEngine;
using SketchRenderer.ShaderLibrary;
using UnityEngine.Rendering;

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
            if(VolumeManager.instance == null || VolumeManager.instance.stack == null)
                return this;
            CompositionVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<CompositionVolumeComponent>();
            if (volumeComponent == null)
                return this;
            SketchCompositionPassData overrideData = new SketchCompositionPassData();

            overrideData.OutlineStrokeColor = volumeComponent.OutlineStrokeColor.overrideState ? volumeComponent.OutlineStrokeColor.value : OutlineStrokeColor;
            overrideData.ShadingStrokeColor = volumeComponent.ShadingStrokeColor.overrideState ? volumeComponent.ShadingStrokeColor.value : ShadingStrokeColor;
            overrideData.StrokeBlendMode = volumeComponent.StrokeBlendingOperation.overrideState ? volumeComponent.StrokeBlendingOperation.value : StrokeBlendMode;
            overrideData.BlendStrength = volumeComponent.BlendStrength.overrideState ? volumeComponent.BlendStrength.value : BlendStrength;
            overrideData.MaterialAccumulationStrength = volumeComponent.MaterialAccumulation.overrideState ? volumeComponent.MaterialAccumulation.value : MaterialAccumulationStrength;
            overrideData.featuresToCompose = FeaturesToCompose;
            
            return overrideData;
        }

        public bool RequiresColorTexture()
        {
            return FeaturesToCompose != null && !FeaturesToCompose.Contains(SketchRendererFeatureType.MATERIAL);
        }
    }
}