using System;
using UnityEngine;
using UnityEngine.Rendering;
using SketchRenderer.Runtime.Data;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [System.Serializable]
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "Smooth Outline")]
    public class SmoothOutlineVolumeComponent : OutlineVolumeComponent
    {
        [Header("Accented Outlines")]
        public BoolParameter UseAccentedOutlines = new(true);
        [Space(2.5f)]
        [Header("Accented Outlines - Thickness")]
        public ClampedIntParameter ThicknessRange = new ClampedIntParameter(0, 0, 5);
        public ClampedFloatParameter ThicknessStrength = new ClampedFloatParameter(0, 0, 1);
        [Space(2.5f)]
        [Header("Accented Outlines - Distortion")]
        public BoolParameter BakeDistortion = new BoolParameter(false);
        public FloatParameter DistortionRate = new FloatParameter(20f);
        public ClampedFloatParameter DistortionStrength = new ClampedFloatParameter(0, 0, 1);
        public ClampedIntParameter AdditionalDistortionLines = new ClampedIntParameter(0, 0, 3);
        public ClampedFloatParameter AdditionalLineTintPersistence = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter AdditionalLinesDistortionJitter = new ClampedFloatParameter(0, 0, 1);
    }
}