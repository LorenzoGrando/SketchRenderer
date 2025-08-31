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
        public Texture2D AlbedoTexture;
        public Texture2D NormalTexture;
        public Vector2Int Scale;
        [Range(0f, 1f)]
        public float BaseColorBlendFactor;

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
            return AlbedoTexture != null && NormalTexture != null;
        }

        public MaterialSurfacePassData GetPassDataByVolume()
        {
            if(VolumeManager.instance == null || VolumeManager.instance.stack == null)
                return this;

            MaterialSurfacePassData overrideData = new MaterialSurfacePassData();
        
            overrideData.ProjectionMethod = ProjectionMethod;
            overrideData.AlbedoTexture = AlbedoTexture;
            overrideData.NormalTexture = NormalTexture;
            overrideData.Scale = Scale;
            overrideData.BaseColorBlendFactor = BaseColorBlendFactor;
        
            return overrideData;
        }

        public bool RequiresTextureCoordinateFeature()
        {
            return TextureProjectionGlobalData.CheckProjectionRequiresUVFeature(GetPassDataByVolume().ProjectionMethod);
        }
    }
}