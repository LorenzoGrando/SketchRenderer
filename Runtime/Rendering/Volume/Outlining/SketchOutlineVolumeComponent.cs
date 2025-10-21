using System;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [System.Serializable]
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "Sketch Outline")]
    public class SketchOutlineVolumeComponent : OutlineVolumeComponent, ISketchVolumeComponent
    {
        [Header("Sketch Outlines")]
        public EnumParameter<ComputeData.KernelSize2D> StrokeArea = new (ComputeData.KernelSize2D.SIZE_8X8);
        public ClampedIntParameter StrokeCombinationRange = new(0, 0, 8);
        public ClampedFloatParameter StrokeCombinationThreshold = new(0.75f, 0f, 1f);
        public ClampedIntParameter StrokeScale = new (1, 1, 4);
        public ClampedFloatParameter StrokeScaleOffset = new (1, 0, 1);
        public BoolParameter DoDownscale = new (true);
        public ClampedFloatParameter MinThresholdForStroke = new (0.1f, 0, 1);
        public ClampedFloatParameter DirectionSmoothing = new (0.5f, 0, 1);
        public ClampedFloatParameter FrameSmoothingFactor = new (0, 0, 1);
        public ClampedFloatParameter StrokeDepthFalloff = new (0, 0f, 1f);

        public override EdgeDetectionGlobalData.EdgeDetectionOutputType OutputType => EdgeDetectionGlobalData.EdgeDetectionOutputType.OUTPUT_DIRECTION_DATA_VECTOR;
        
        public bool HasStrokeAssetOverride => StrokeAsset != null;
        public StrokeAsset StrokeAssetRef;
        public StrokeAsset StrokeAsset { get; private set; }
        
        public void CopyFromContext(SketchRendererContext context)
        {
            base.CopyFromContext(context);
            StrokeArea.value = context.SketchyOutlineFeatureData.SampleArea;
            StrokeCombinationRange.value = context.SketchyOutlineFeatureData.StrokeCombinationRange;
            StrokeCombinationThreshold.value = context.SketchyOutlineFeatureData.StrokeCombinationThreshold;
            StrokeScale.value = context.SketchyOutlineFeatureData.StrokeSampleScale;
            StrokeScaleOffset.value = context.SketchyOutlineFeatureData.StrokeSampleOffsetRate;
            DoDownscale.value = context.SketchyOutlineFeatureData.DoDownscale;
            MinThresholdForStroke.value = context.SketchyOutlineFeatureData.StrokeThreshold;
            DirectionSmoothing.value = context.SketchyOutlineFeatureData.DirectionSmoothingFactor;
            FrameSmoothingFactor.value = context.SketchyOutlineFeatureData.FrameSmoothingFactor;
            StrokeDepthFalloff.value = context.SketchyOutlineFeatureData.StrokeThicknessDepthFalloff;
            StrokeAsset = context.SketchyOutlineFeatureData.OutlineStrokeData;
        }
    }
}