using SketchRenderer.Runtime.Data;
using SketchRenderer.ShaderLibrary;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "Composition")]
    public class CompositionVolumeComponent : VolumeComponent
    {
        public ColorParameter OutlineStrokeColor = new ColorParameter(Color.black);
        public ColorParameter ShadingStrokeColor = new ColorParameter(Color.black);
        public EnumParameter<BlendingOperations> StrokeBlendingOperation = new EnumParameter<BlendingOperations>(BlendingOperations.BLEND_MULTIPLY);
        public ClampedFloatParameter BlendStrength = new ClampedFloatParameter(1f, 0f, 1f);
        public ClampedFloatParameter MaterialAccumulation = new ClampedFloatParameter(0f, 0f, 1f);
    }
}