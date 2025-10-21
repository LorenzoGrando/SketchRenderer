using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [System.Serializable]
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "Material")]
    public class MaterialVolumeComponent : VolumeComponent, ISketchVolumeComponent
    {
        public EnumParameter<TextureProjectionGlobalData.TextureProjectionMethod> ProjectionMethod =
            new EnumParameter<TextureProjectionGlobalData.TextureProjectionMethod>(TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_CONSTANT_SCALE);
        public ClampedFloatParameter ConstantScaleFalloffFactor = new ClampedFloatParameter(0f, 1f, 5f);
        public Texture2DParameter AlbedoTexture = new Texture2DParameter(null);
        public Texture2DParameter DirectionalTexture = new Texture2DParameter(null);
        public NoInterpVector2Parameter Scales = new NoInterpVector2Parameter(Vector2.one);
        public ClampedFloatParameter BaseColorBlend = new ClampedFloatParameter(0f, 0f, 1f);
        
        public void CopyFromContext(SketchRendererContext context)
        {
            ProjectionMethod.value = context.MaterialFeatureData.ProjectionMethod;
            ConstantScaleFalloffFactor.value = context.MaterialFeatureData.ConstantScaleFalloffFactor;
            AlbedoTexture.value = context.MaterialFeatureData.AlbedoTexture;
            DirectionalTexture.value = context.MaterialFeatureData.NormalTexture;
            Scales.value = context.MaterialFeatureData.Scale;
            BaseColorBlend.value = context.MaterialFeatureData.BaseColorBlendFactor;
        }
    }
}