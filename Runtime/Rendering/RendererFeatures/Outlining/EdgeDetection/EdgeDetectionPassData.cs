using System;
using UnityEngine;
using UnityEngine.Rendering;
using SketchRenderer.Runtime.Rendering.Volume;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    [Serializable]
    public class EdgeDetectionPassData : ISketchRenderPassData<EdgeDetectionPassData>
    {
        public EdgeDetectionGlobalData.EdgeDetectionMethod Method;
        public EdgeDetectionGlobalData.EdgeDetectionSource Source;
        [Range(0, 1)]
        public float OutlineThreshold;
        [Range(0, 3)]
        public int OutlineOffset;
        [Range(0, 1)] 
        public float OutlineDistanceFalloff;
        [Range(0,1)]
        public float OutlineAngleSensitivity;
        [Range(0,1)]
        public float OutlineAngleConstraint;
        [Range(0,1)]
        public float OutlineNormalSensitivity;
        
        [HideInInspector]
        public EdgeDetectionGlobalData.EdgeDetectionOutputType OutputType;
        
        //Multiple edge passes
        [HideInInspector]
        public bool IsSplitEdgePass => Source == EdgeDetectionGlobalData.EdgeDetectionSource.ALL;
        [HideInInspector] [SerializeField]
        [Range(0, 1)]
        public float PrimarySplitOutlineThreshold = 0;
        [HideInInspector] [SerializeField]
        [Range(0, 1)]
        public float SecondarySplitOutlineThreshold = 0;

        public EdgeDetectionPassData()
        {
            Method = EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_3X3;
            Source = EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH_NORMALS;
            OutlineThreshold = 0.075f;
            OutlineOffset = 0;
            OutlineDistanceFalloff = 0.5f;
            OutlineAngleSensitivity = 1f;
            OutlineAngleConstraint = 0.45f;
            OutlineNormalSensitivity = 0.5f;
        }
        public void CopyFrom(EdgeDetectionPassData passData)
        {
            Method = passData.Method;
            Source = passData.Source;
            OutlineThreshold = passData.OutlineThreshold;
            OutlineOffset = passData.OutlineOffset;
            OutlineDistanceFalloff = passData.OutlineDistanceFalloff;
            OutlineAngleSensitivity = passData.OutlineAngleSensitivity;
            OutlineAngleConstraint = passData.OutlineAngleConstraint;
            OutlineNormalSensitivity = passData.OutlineNormalSensitivity;
            OutputType = passData.OutputType;
            PrimarySplitOutlineThreshold = passData.PrimarySplitOutlineThreshold;
            SecondarySplitOutlineThreshold = passData.SecondarySplitOutlineThreshold;
        }
        public bool IsAllPassDataValid()
        {
            return true;
        }

        private OutlineVolumeComponent UpdateTargetVolume()
        {
            //This honestly feels really bad to do,
            //but i found it ideal over needing too add two different components to a stack, one for edge detection and one for outline specifics.
            //So each class inherits from a common base, and we choose here based on the stack
            //TODO: This just fires a warning, ideally select the correct volume based on renderer data?

            SmoothOutlineVolumeComponent smoothComponent = VolumeManager.instance.stack.GetComponent<SmoothOutlineVolumeComponent>();
            SketchOutlineVolumeComponent sketchComponent = VolumeManager.instance.stack.GetComponent<SketchOutlineVolumeComponent>();
            bool hasSmooth = smoothComponent != null && smoothComponent.AnyPropertiesIsOverridden();
            bool hasSketch = sketchComponent != null && sketchComponent.AnyPropertiesIsOverridden();
            if (hasSmooth && hasSketch)
            {
                Debug.LogWarning("Multiple edge detection outline volumes detected in scene, defaulting to Settings values. Please remove or disable one of the overrides.");
                return null;
            }
            else if (hasSmooth)
                return smoothComponent;
            else if (hasSketch)
                return sketchComponent;
            else return null;
        }

        public EdgeDetectionPassData GetPassDataByVolume()
        {
            if(VolumeManager.instance == null || VolumeManager.instance.stack == null)
                return this;
            OutlineVolumeComponent volumeComponent = UpdateTargetVolume();
            if (volumeComponent == null || !volumeComponent.active)
                return this;

            EdgeDetectionPassData overrideData = new EdgeDetectionPassData();
            
            overrideData.Method = volumeComponent.Method.overrideState
                ? volumeComponent.Method.value : Method;
            overrideData.Source = volumeComponent.Source.overrideState ? volumeComponent.Source.value : Source;
            if (overrideData.Source == EdgeDetectionGlobalData.EdgeDetectionSource.COLOR)
                overrideData.OutlineThreshold = volumeComponent.ColorThreshold.overrideState ? volumeComponent.ColorThreshold.value : OutlineThreshold;
            else
                overrideData.OutlineThreshold = volumeComponent.DepthNormalsThreshold.overrideState ? volumeComponent.DepthNormalsThreshold.value : OutlineThreshold;
            overrideData.OutlineDistanceFalloff = volumeComponent.DistanceFalloff.overrideState ? volumeComponent.DistanceFalloff.value : OutlineDistanceFalloff;
            overrideData.OutlineOffset = volumeComponent.Offset.overrideState ? volumeComponent.Offset.value : OutlineOffset;
            overrideData.OutlineAngleSensitivity = volumeComponent.AngleSensitivity.overrideState ? volumeComponent.AngleSensitivity.value : OutlineAngleSensitivity;
            overrideData.OutlineAngleConstraint = volumeComponent.AngleConstraint.overrideState ? volumeComponent.AngleConstraint.value : OutlineAngleConstraint;
            overrideData.OutlineNormalSensitivity = volumeComponent.NormalSensitivity.overrideState ? volumeComponent.NormalSensitivity.value : OutlineNormalSensitivity;
            overrideData.OutputType = OutputType;

            overrideData.PrimarySplitOutlineThreshold = volumeComponent.DepthNormalsThreshold.overrideState ? volumeComponent.DepthNormalsThreshold.value : PrimarySplitOutlineThreshold;
            overrideData.SecondarySplitOutlineThreshold = volumeComponent.ColorThreshold.overrideState ? volumeComponent.ColorThreshold.value : SecondarySplitOutlineThreshold;
            
            return overrideData;
        }
    }
}