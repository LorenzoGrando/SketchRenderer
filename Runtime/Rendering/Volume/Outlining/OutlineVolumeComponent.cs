using System;
using UnityEngine;
using UnityEngine.Rendering;
using SketchRenderer.Runtime.Rendering.RendererFeatures;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [System.Serializable]
    public abstract class OutlineVolumeComponent : VolumeComponent
    {
        [Space(10)]
        [Header("Edge Detection")]
        public EnumParameter<EdgeDetectionGlobalData.EdgeDetectionMethod> Method =
            new EnumParameter<EdgeDetectionGlobalData.EdgeDetectionMethod>(EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_3X3);
        public EnumParameter<EdgeDetectionGlobalData.EdgeDetectionSource> Source =
            new EnumParameter<EdgeDetectionGlobalData.EdgeDetectionSource>(EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH);
        public ClampedFloatParameter DepthNormalsThreshold = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter ColorThreshold = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter DistanceFalloff = new ClampedFloatParameter(1f, 0, 1);
        public ClampedIntParameter Offset = new ClampedIntParameter(0, 0, 3);
        public ClampedFloatParameter AngleSensitivity = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter AngleConstraint = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter NormalSensitivity = new ClampedFloatParameter(0, 0, 1);
    }
}