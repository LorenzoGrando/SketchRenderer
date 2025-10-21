using System.Collections.Generic;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using SketchRenderer.ShaderLibrary;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "Composition")]
    public class CompositionVolumeComponent : VolumeComponent, ISketchVolumeComponent
    {
        public ColorParameter OutlineStrokeColor = new ColorParameter(Color.black);
        public ColorParameter ShadingStrokeColor = new ColorParameter(Color.black);
        public EnumParameter<BlendingOperations> StrokeBlendingOperation = new EnumParameter<BlendingOperations>(BlendingOperations.BLEND_MULTIPLY);
        public ClampedFloatParameter BlendStrength = new ClampedFloatParameter(1f, 0f, 1f);
        public ClampedFloatParameter MaterialAccumulation = new ClampedFloatParameter(0f, 0f, 1f);

        public bool HasFeatureOverride => Features != null && Features.Count > 0;
        public List<SketchRendererFeatureType> Features { get; private set; }

        public void CopyFromContext(SketchRendererContext context)
        {
            OutlineStrokeColor.value = context.CompositionFeatureData.OutlineStrokeColor;
            ShadingStrokeColor.value = context.CompositionFeatureData.ShadingStrokeColor;
            StrokeBlendingOperation.value = context.CompositionFeatureData.StrokeBlendMode;
            BlendStrength.value = context.CompositionFeatureData.BlendStrength;
            MaterialAccumulation.value = context.CompositionFeatureData.MaterialAccumulationStrength;
        }

        public void SetRenderingTargets(List<SketchRendererFeatureType> targets)
        {
            Features = targets;
        }
    }
}