using System;
using UnityEngine;
using UnityEngine.Rendering;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "Luminance")]
    public class LuminanceVolumeComponent : VolumeComponent, ISketchVolumeComponent
    {
        public EnumParameter<TextureProjectionGlobalData.TextureProjectionMethod> ProjectionMethod =
            new EnumParameter<TextureProjectionGlobalData.TextureProjectionMethod>(TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_CONSTANT_SCALE);
        public ClampedFloatParameter ConstantScaleFalloffFactor = new ClampedFloatParameter(0f, 1f, 5f);
        public BoolParameter SmoothTransitions = new BoolParameter(false);
        public NoInterpVector2Parameter ToneScales = new NoInterpVector2Parameter(Vector2.one);
        public ClampedFloatParameter LuminanceOffset = new ClampedFloatParameter(0f, -1f, 1f);

        public bool HasTAMOverride => TonalArtMap != null;
        public TonalArtMapAsset TonalArtMap { get; private set; }
        
        public void CopyFromContext(SketchRendererContext context)
        {
            ProjectionMethod.value = context.LuminanceFeatureData.ProjectionMethod;
            ConstantScaleFalloffFactor.value = context.LuminanceFeatureData.ConstantScaleFalloffFactor;
            SmoothTransitions.value = context.LuminanceFeatureData.SmoothTransitions;
            ToneScales.value = context.LuminanceFeatureData.ToneScales;
            LuminanceOffset.value = context.LuminanceFeatureData.LuminanceScalar;
            TonalArtMap = context.LuminanceFeatureData.ActiveTonalMap;
        }
    }
}