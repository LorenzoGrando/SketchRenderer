using System;
using SketchRenderer.Runtime.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [System.Serializable]
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "Sketch Outline")]
    public class SketchOutlineVolumeComponent : OutlineVolumeComponent
    {
        [Header("Sketch Outlines")]
        public EnumParameter<ComputeData.KernelSize2D> StrokeArea = new (ComputeData.KernelSize2D.SIZE_8X8);
        public ClampedIntParameter StrokeScale = new (1, 1, 4);
        public ClampedFloatParameter StrokeScaleOffset = new (1, 0, 1);
        public BoolParameter DoDownscale = new (true);
        public ClampedIntParameter DownscaleFactor = new (2, 2, 4);
        public ClampedFloatParameter MinThresholdForStroke = new (0.1f, 0, 1);
        public ClampedFloatParameter DirectionSmoothing = new (0.5f, 1, 1);
        public ClampedFloatParameter FrameSmoothingFactor = new (0, 0, 1);
    }
}