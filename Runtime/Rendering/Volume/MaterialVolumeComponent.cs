using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [System.Serializable]
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "Material")]
    public class MaterialVolumeComponent : VolumeComponent
    {
        public EnumParameter<TextureProjectionGlobalData.TextureProjectionMethod> ProjectionMethod =
            new EnumParameter<TextureProjectionGlobalData.TextureProjectionMethod>(TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_CONSTANT_SCALE);
        public ClampedFloatParameter ConstantScaleFalloffFactor = new ClampedFloatParameter(0f, 1f, 5f);
        public Texture2DParameter AlbedoTexture = new Texture2DParameter(null);
        public Texture2DParameter DirectionalTexture = new Texture2DParameter(null);
        public NoInterpVector2Parameter Scales = new NoInterpVector2Parameter(Vector2.one);
        public ClampedFloatParameter BaseColorBlend = new ClampedFloatParameter(0f, 0f, 1f);
    }
}