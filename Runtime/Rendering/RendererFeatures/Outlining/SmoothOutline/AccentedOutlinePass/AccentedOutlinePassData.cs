using UnityEngine;
using UnityEngine.Rendering;
using SketchRenderer.Runtime.Rendering.Volume;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    [System.Serializable]
    public class AccentedOutlinePassData : ISketchRenderPassData<AccentedOutlinePassData>
    {
        public bool UseAccentedOutlines;
        [Header("Distortion Settings")] 
        public bool BakeDistortionDuringRuntime;
        [Range(0.25f, 1f)]
        public float BakedTextureScaleFactor = 1f;
        public float Rate = 20f;
        [Range(0f, 1f)]
        public float Strength = 0.3f;

        [Header("Additional Lines")] 
        [Range(0, 3)]
        public int AdditionalLines;
        [Range(0, 1f)]
        public float AdditionalLineTintPersistence;
        [Range(0, 1f)]
        public float AdditionalLineDistortionJitter;

        [Header("Outline Masking")] 
        public Texture2D PencilOutlineMask;
        public Vector2 MaskScale;
        
        public AccentedOutlinePassData()
        {
            Rate = 20.0f;
            Strength = 1.0f;
        }
        
        public bool RequireMultipleTextures => AdditionalLines > 1;
        [HideInInspector] public bool ForceRebake;

        public void CopyFrom(AccentedOutlinePassData passData)
        {
            UseAccentedOutlines = passData.UseAccentedOutlines;
            BakeDistortionDuringRuntime = passData.BakeDistortionDuringRuntime;
            Rate = passData.Rate;
            Strength = passData.Strength;
            AdditionalLines = passData.AdditionalLines;
            AdditionalLineTintPersistence = passData.AdditionalLineTintPersistence;
            AdditionalLineDistortionJitter = passData.AdditionalLineDistortionJitter;
            PencilOutlineMask = passData.PencilOutlineMask;
            MaskScale = new Vector2(passData.MaskScale.x, passData.MaskScale.y);
        }
        
        public bool IsAllPassDataValid()
        {
            return UseAccentedOutlines && (Strength > 0 || (PencilOutlineMask != null && MaskScale != Vector2.zero));
        }

        public AccentedOutlinePassData GetPassDataByVolume()
        {
            if(VolumeManager.instance == null || VolumeManager.instance.stack == null)
                return this;
            SmoothOutlineVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<SmoothOutlineVolumeComponent>();
            if (volumeComponent == null)
                return this;
            AccentedOutlinePassData overrideData = new AccentedOutlinePassData();
            
            overrideData.UseAccentedOutlines = volumeComponent.UseAccentedOutlines.overrideState ? volumeComponent.UseAccentedOutlines.value : UseAccentedOutlines;
            overrideData.BakeDistortionDuringRuntime = volumeComponent.BakeDistortion.overrideState ? volumeComponent.BakeDistortion.value : BakeDistortionDuringRuntime;
            overrideData.BakedTextureScaleFactor = volumeComponent.BakedDistortionTextureScale.overrideState ? volumeComponent.BakedDistortionTextureScale.value : BakedTextureScaleFactor;
            overrideData.Rate = volumeComponent.DistortionRate.overrideState ? volumeComponent.DistortionRate.value : Rate;
            overrideData.Strength = volumeComponent.DistortionStrength.overrideState ? volumeComponent.DistortionStrength.value : Strength;
            overrideData.AdditionalLines = volumeComponent.AdditionalDistortionLines.overrideState ? volumeComponent.AdditionalDistortionLines.value : AdditionalLines;
            overrideData.AdditionalLineTintPersistence = volumeComponent.AdditionalLineTintPersistence.overrideState ? volumeComponent.AdditionalLineTintPersistence.value : AdditionalLineTintPersistence;
            overrideData.AdditionalLineDistortionJitter = volumeComponent.AdditionalLinesDistortionJitter.overrideState ? volumeComponent.AdditionalLinesDistortionJitter.value : AdditionalLineDistortionJitter;
            overrideData.PencilOutlineMask = PencilOutlineMask;
            overrideData.MaskScale = MaskScale;
            
            return overrideData;
        }
    }
}