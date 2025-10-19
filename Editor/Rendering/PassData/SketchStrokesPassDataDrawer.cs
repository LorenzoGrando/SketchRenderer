using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Rendering;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    [CustomPropertyDrawer(typeof(SketchStrokesPassData))]
    internal class SketchStrokesPassDataDrawer : PropertyDrawer
    {
        VisualElement passDataField;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            passDataField = new VisualElement();
            var passDataLabel = new Label("Sketchy Strokes");
            passDataField.Add(passDataLabel);
            
            SerializedProperty strokeProp = property.FindPropertyRelative("OutlineStrokeData");
            var strokeAssetField = SketchRendererUI.SketchObjectProperty(strokeProp, typeof(StrokeAsset), nameOverride:"Stroke Asset");
            SketchRendererUIUtils.AddWithMargins(passDataField, strokeAssetField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty combinationRangeProp = property.FindPropertyRelative("StrokeCombinationRange");
            var combinationRangeField = SketchRendererUI.SketchIntSliderPropertyWithInput(combinationRangeProp, changeCallback:evt => ZeroToggleableFeature_Changed(evt, 0));
            SketchRendererUIUtils.AddWithMargins(passDataField, combinationRangeField.Container, SketchRendererUIData.MajorIndentCorners);

            if (combinationRangeProp.intValue > 0)
            {
                SerializedProperty combinationThresholdProp = property.FindPropertyRelative("StrokeCombinationThreshold");
                var combinationThresholdField = SketchRendererUI.SketchFloatSliderPropertyWithInput(combinationThresholdProp);
                SketchRendererUIUtils.AddWithMargins(passDataField, combinationThresholdField.Container, SketchRendererUIData.MajorIndentCorners);
            }
            
            
            SerializedProperty kernelSizeProp = property.FindPropertyRelative("SampleArea");
            ComputeData.KernelSize2D kernelSize = (ComputeData.KernelSize2D)kernelSizeProp.enumValueIndex;
            var kernelSizeField = SketchRendererUI.SketchEnumProperty(kernelSizeProp, kernelSize, nameOverride: "Stroke Detection Area"); 
            SketchRendererUIUtils.AddWithMargins(passDataField, kernelSizeField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty strokeScaleProp = property.FindPropertyRelative("StrokeSampleScale");
            var strokeScaleField = SketchRendererUI.SketchIntSliderPropertyWithInput(strokeScaleProp, changeCallback:evt => ZeroToggleableFeature_Changed(evt, 1), nameOverride: "Stroke Scaling Area");
            SketchRendererUIUtils.AddWithMargins(passDataField, strokeScaleField.Container, SketchRendererUIData.MajorIndentCorners);

            if (strokeScaleProp.intValue > 1)
            {
                SerializedProperty strokeScaleOffsetProp = property.FindPropertyRelative("StrokeSampleOffsetRate");
                var strokeScaleOffsetField = SketchRendererUI.SketchFloatSliderPropertyWithInput(strokeScaleOffsetProp, nameOverride: "Stroke Origin Offset");
                SketchRendererUIUtils.AddWithMargins(passDataField, strokeScaleOffsetField.Container, SketchRendererUIData.MajorIndentCorners);
            }
            
            SerializedProperty thresholdProp = property.FindPropertyRelative("StrokeThreshold");
            var thresholdField = SketchRendererUI.SketchFloatSliderPropertyWithInput(thresholdProp, nameOverride: "Detection Threshold");
            SketchRendererUIUtils.AddWithMargins(passDataField, thresholdField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty directionSmoothingProp = property.FindPropertyRelative("DirectionSmoothingFactor");
            var directionSmoothingField = SketchRendererUI.SketchFloatSliderPropertyWithInput(directionSmoothingProp, nameOverride: "Direction Smoothing");
            SketchRendererUIUtils.AddWithMargins(passDataField, directionSmoothingField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty smoothingProp = property.FindPropertyRelative("FrameSmoothingFactor");
            var smoothingField = SketchRendererUI.SketchFloatSliderPropertyWithInput(smoothingProp, nameOverride: "Per-Frame Smoothing");
            SketchRendererUIUtils.AddWithMargins(passDataField, smoothingField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty doDownscaleProp = property.FindPropertyRelative("DoDownscale");
            var downscaleField = SketchRendererUI.SketchBoolProperty(doDownscaleProp, nameOverride: "Do Downscaled Detection");
            downscaleField.Field.RegisterValueChangedCallback(_ => ForceRepaint());
            SketchRendererUIUtils.AddWithMargins(passDataField, downscaleField.Container, SketchRendererUIData.MajorIndentCorners);
            
            /*
            if (doDownscaleProp.boolValue)
            {
                SerializedProperty downscaleFactorProp = property.FindPropertyRelative("DownscaleFactor");
                var downscaleFactorField = SketchRendererUI.SketchIntSliderPropertyWithInput(downscaleFactorProp, nameOverride: "Downscale Amount");
                SketchRendererUIUtils.AddWithMargins(passDataField, downscaleFactorField.Container, SketchRendererUIData.MajorIndentCorners);
            }
            */
            
            return passDataField;
        }
        
        internal void ForceRepaint()
        {
            passDataField.SendEvent(ExecuteCommandEvent.GetPooled(SketchRendererUIData.RepaintEditorCommand));
        }

        internal void ZeroToggleableFeature_Changed(ChangeEvent<int> evt, int thresholdValue)
        {
            if (evt.previousValue > thresholdValue && evt.newValue == thresholdValue)
                ForceRepaint();
            else if(evt.previousValue == thresholdValue && evt.newValue > thresholdValue)
                ForceRepaint();
        }
    }
}