using System;
using UnityEngine;
using UnityEngine.Rendering;
using SketchRenderer.Runtime.Rendering.Volume;
using SketchRenderer.Runtime.TextureTools.Strokes;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    [Serializable]
    public class SketchStrokesPassData : ISketchRenderPassData<SketchStrokesPassData>
    {
        public StrokeAsset OutlineStrokeData;
        public ComputeData.KernelSize2D SampleArea;
        [Range(1f, 4f)] 
        public int StrokeSampleScale;
        public bool DoDownscale;
        [Range(2, 4)]
        public int DownscaleFactor;
        [Range(0f, 1f)] 
        public float StrokeThreshold;
        [Range(0f, 1f)] 
        public float FrameSmoothingFactor;
        
        [HideInInspector] 
        public bool UsePerpendicularDirection;

        public SketchStrokesPassData()
        {
            SampleArea = ComputeData.KernelSize2D.SIZE_8X8;
            StrokeSampleScale = 2;
            DoDownscale = false;
            DownscaleFactor = 2;
            StrokeThreshold = 0.05f;
            FrameSmoothingFactor = 0;
        }

        public void CopyFrom(SketchStrokesPassData passData)
        {
            OutlineStrokeData = passData.OutlineStrokeData;
            SampleArea = passData.SampleArea;
            StrokeSampleScale = passData.StrokeSampleScale;
            DoDownscale = passData.DoDownscale;
            DownscaleFactor = passData.DownscaleFactor;
            StrokeThreshold = passData.StrokeThreshold;
            FrameSmoothingFactor = passData.FrameSmoothingFactor;
            UsePerpendicularDirection = passData.UsePerpendicularDirection;
        }

        public bool IsAllPassDataValid()
        {
            return OutlineStrokeData != null;
        }

        public void ConfigurePerpendicularDirection(EdgeDetectionGlobalData.EdgeDetectionMethod method)
        {
            switch (method)
            {
                case EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_3X3:
                    UsePerpendicularDirection = false;
                    break;
                case EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_1X3:
                    UsePerpendicularDirection = true;
                    break;
            }
        }

        public SketchStrokesPassData GetPassDataByVolume()
        {
            if(VolumeManager.instance == null || VolumeManager.instance.stack == null)
                return this;
            SketchOutlineVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<SketchOutlineVolumeComponent>();
            if (volumeComponent == null)
                return this;
            
            SketchStrokesPassData overrideData = new SketchStrokesPassData();
            overrideData.OutlineStrokeData = OutlineStrokeData;
            overrideData.SampleArea = volumeComponent.StrokeArea.overrideState ? volumeComponent.StrokeArea.value : SampleArea;
            overrideData.StrokeSampleScale = volumeComponent.StrokeScale.overrideState ? volumeComponent.StrokeScale.value : StrokeSampleScale;
            overrideData.DoDownscale = volumeComponent.DoDownscale.overrideState ? volumeComponent.DoDownscale.value : DoDownscale;
            if(overrideData.DoDownscale)
                overrideData.DownscaleFactor = volumeComponent.DownscaleFactor.overrideState ? volumeComponent.DownscaleFactor.value : DownscaleFactor;
            else
                overrideData.DownscaleFactor = 1;
            overrideData.StrokeThreshold = volumeComponent.MinThresholdForStroke.overrideState ? volumeComponent.MinThresholdForStroke.value : StrokeThreshold;;
            overrideData.FrameSmoothingFactor = volumeComponent.FrameSmoothingFactor.overrideState ? volumeComponent.FrameSmoothingFactor.value : FrameSmoothingFactor;
            
            return overrideData;
        }
    }
}