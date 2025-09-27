using System;
using UnityEngine;
using UnityEngine.Rendering;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [System.Serializable]
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "Luminance")]
    public class LuminanceVolumeComponent : VolumeComponent
    {
        public EnumParameter<TextureProjectionGlobalData.TextureProjectionMethod> ProjectionMethod =
            new EnumParameter<TextureProjectionGlobalData.TextureProjectionMethod>(TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_CONSTANT_SCALE);
        public ClampedFloatParameter ConstantScaleFalloffFactor = new ClampedFloatParameter(0f, 1f, 5f);
        public BoolParameter SmoothTransitions = new BoolParameter(false);
        public NoInterpVector2Parameter ToneScales = new NoInterpVector2Parameter(Vector2.one);
        public ClampedFloatParameter LuminanceOffset = new ClampedFloatParameter(0f, -1f, 1f);
    }
}