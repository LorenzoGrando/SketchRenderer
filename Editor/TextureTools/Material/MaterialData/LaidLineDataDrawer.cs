using SketchRenderer.Editor.UIToolkit;
using TextureTools.Material;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.MaterialData
{
    [CustomPropertyDrawer(typeof(LaidLineData))]
    public class LaidLineDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var assetField = new VisualElement();

            var lineFrequencyField = SketchRendererUI.SketchIntegerProperty(property.FindPropertyRelative("LineFrequency"));
            SketchRendererUIUtils.AddWithMargins(assetField, lineFrequencyField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var thicknessField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("LineThickness"));
            SketchRendererUIUtils.AddWithMargins(assetField, thicknessField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var strengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("LineStrength"));
            SketchRendererUIUtils.AddWithMargins(assetField, strengthField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var granularityDisplacementField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("LineGranularityDisplacement"), nameOverride:"Granularity Displacement");
            SketchRendererUIUtils.AddWithMargins(assetField, granularityDisplacementField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var granularityMaskField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("LineGranularityMasking"), nameOverride:"Granularity Masking");
            SketchRendererUIUtils.AddWithMargins(assetField, granularityMaskField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var tintField = SketchRendererUI.SketchColorProperty(property.FindPropertyRelative("LineTint"), nameOverride:"Tint");
            SketchRendererUIUtils.AddWithMargins(assetField, tintField.Container, SketchRendererUIData.MajorIndentCorners);
            
            return assetField;
        }
    }
}