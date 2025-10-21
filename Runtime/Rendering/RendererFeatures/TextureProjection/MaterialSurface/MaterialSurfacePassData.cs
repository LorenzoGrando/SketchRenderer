using SketchRenderer.Runtime.Rendering.Volume;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    [System.Serializable]
    public class MaterialSurfacePassData : ISketchRenderPassData<MaterialSurfacePassData>
    {
        public TextureProjectionGlobalData.TextureProjectionMethod ProjectionMethod;
        [Range(1f, 5f)]
        public float ConstantScaleFalloffFactor = 2f;
        public Texture AlbedoTexture;
        public Texture NormalTexture;
        public Vector2Int Scale;
        [Range(0f, 1f)]
        public float BaseColorBlendFactor;

        public MaterialSurfacePassData()
        {
            ProjectionMethod = TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE;
            ConstantScaleFalloffFactor = 2f;
            Scale = Vector2Int.one;
            BaseColorBlendFactor = 0f;
        }

        public void CopyFrom(MaterialSurfacePassData passData)
        {
            ProjectionMethod = passData.ProjectionMethod;
            ConstantScaleFalloffFactor = passData.ConstantScaleFalloffFactor;
            AlbedoTexture = passData.AlbedoTexture;
            NormalTexture = passData.NormalTexture;
            Scale = new Vector2Int(passData.Scale.x, passData.Scale.y);
            BaseColorBlendFactor = passData.BaseColorBlendFactor;
        }
    
        public bool IsAllPassDataValid()
        {
            MaterialSurfacePassData passData = GetPassDataByVolume();
            return passData.AlbedoTexture != null && passData.NormalTexture != null;
        }

        public MaterialSurfacePassData GetPassDataByVolume()
        {
            if(VolumeManager.instance == null || VolumeManager.instance.stack == null)
                return this;
            MaterialVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<MaterialVolumeComponent>();
            if (volumeComponent == null || !volumeComponent.active)
                return this;

            MaterialSurfacePassData overrideData = new MaterialSurfacePassData();
        
            overrideData.ProjectionMethod = volumeComponent.ProjectionMethod.overrideState ? volumeComponent.ProjectionMethod.value : ProjectionMethod;
            overrideData.ConstantScaleFalloffFactor = volumeComponent.ConstantScaleFalloffFactor.overrideState ? volumeComponent.ConstantScaleFalloffFactor.value : ConstantScaleFalloffFactor;
            overrideData.AlbedoTexture = volumeComponent.AlbedoTexture.overrideState ? volumeComponent.AlbedoTexture.value : AlbedoTexture;
            overrideData.NormalTexture = volumeComponent.DirectionalTexture.overrideState ? volumeComponent.DirectionalTexture.value : NormalTexture;
            overrideData.Scale = volumeComponent.Scales.overrideState ? new Vector2Int(Mathf.RoundToInt(volumeComponent.Scales.value.x), Mathf.RoundToInt(volumeComponent.Scales.value.y)) : Scale;
            overrideData.BaseColorBlendFactor = volumeComponent.BaseColorBlend.overrideState ? volumeComponent.BaseColorBlend.value : BaseColorBlendFactor;
        
            return overrideData;
        }
        
        public bool ActiveInVolume()
        {
            if(VolumeManager.instance == null || VolumeManager.instance.stack == null)
                return false;
            MaterialVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<MaterialVolumeComponent>();
            if (volumeComponent == null)
                return false;

            return volumeComponent.AnyPropertiesIsOverridden();
        }

        public bool RequiresTextureCoordinateFeature()
        {
            return TextureProjectionGlobalData.CheckProjectionRequiresUVFeature(GetPassDataByVolume().ProjectionMethod);
        }
    }
}