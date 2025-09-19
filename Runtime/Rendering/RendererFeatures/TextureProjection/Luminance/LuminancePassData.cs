using UnityEngine;
using UnityEngine.Rendering;
using SketchRenderer.Runtime.Rendering.Volume;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    [System.Serializable]
    public class LuminancePassData : ISketchRenderPassData<LuminancePassData>
    {
        public TonalArtMapAsset ActiveTonalMap;
        public TextureProjectionGlobalData.TextureProjectionMethod ProjectionMethod;
        [Range(1f, 5f)]
        public float ConstantScaleFalloffFactor = 2f;
        public bool SmoothTransitions;
        public Vector2 ToneScales = Vector2.one;
        [Range(-1f, 1f)]
        public float LuminanceScalar = 0;
        public float LuminanceOffset
        {
            get
            {
                if (LuminanceScalar < 0)
                    return Mathf.Lerp(1, 0, Mathf.Abs(LuminanceScalar));
                else
                    return Mathf.Lerp(1, 9, LuminanceScalar);
            }
            set => LuminanceScalar = Mathf.Clamp(value, -1f, 1f);
        }

        public void CopyFrom(LuminancePassData passData)
        {
            ActiveTonalMap = passData.ActiveTonalMap;
            ProjectionMethod = passData.ProjectionMethod;
            ConstantScaleFalloffFactor = passData.ConstantScaleFalloffFactor;
            SmoothTransitions = passData.SmoothTransitions;
            ToneScales = new Vector2(passData.ToneScales.x, passData.ToneScales.y);
            LuminanceScalar = passData.LuminanceScalar;
        }

        public bool IsAllPassDataValid()
        {
            return ActiveTonalMap != null && ActiveTonalMap.IsPacked;
        }

        public LuminancePassData GetPassDataByVolume()
        {
            if(VolumeManager.instance == null || VolumeManager.instance.stack == null)
                return this;
            LuminanceVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<LuminanceVolumeComponent>();
            if (volumeComponent == null)
                return this;
            LuminancePassData overrideData = new LuminancePassData();
            
            overrideData.ProjectionMethod = volumeComponent.ProjectionMethod.overrideState ? volumeComponent.ProjectionMethod.value : ProjectionMethod;
            overrideData.SmoothTransitions = volumeComponent.SmoothTransitions.overrideState
                ? volumeComponent.SmoothTransitions.value : SmoothTransitions;
            overrideData.ConstantScaleFalloffFactor = volumeComponent.ConstantScaleFalloffFactor.overrideState ? volumeComponent.ConstantScaleFalloffFactor.value : ConstantScaleFalloffFactor;
            overrideData.ToneScales = volumeComponent.ToneScales.overrideState ? volumeComponent.ToneScales.value : ToneScales;
            overrideData.LuminanceOffset = volumeComponent.LuminanceOffset.overrideState ? volumeComponent.LuminanceOffset.value : LuminanceScalar;
            
            overrideData.ActiveTonalMap = ActiveTonalMap;
            
            return overrideData;
        }
        
        public bool RequiresTextureCoordinateFeature()
        {
            return TextureProjectionGlobalData.CheckProjectionRequiresUVFeature(GetPassDataByVolume().ProjectionMethod);
        }
    }
}