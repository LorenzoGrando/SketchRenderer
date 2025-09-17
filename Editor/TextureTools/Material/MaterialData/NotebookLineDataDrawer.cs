using SketchRenderer.Editor.UIToolkit;
using TextureTools.Material;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.MaterialData
{
    [CustomPropertyDrawer(typeof(NotebookLineData))]
    public class NotebookLineDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var assetField = new VisualElement();

            var horizontalContainer = new VisualElement();
            var horizontalLabel = new Label("Horizontal Lines");
            horizontalContainer.Add(horizontalLabel);
            SketchRendererUIUtils.AddWithMargins(assetField, horizontalContainer, SketchRendererUIData.MajorIndentCorners);

            var horFrequencyManipulator = new ClampedFloatManipulator(0, 100);
            var horizontalFrequencyField = SketchRendererUI.SketchFloatProperty(property.FindPropertyRelative("HorizontalLineFrequency"), true, manipulator:horFrequencyManipulator, nameOverride:"Number of Lines");
            SketchRendererUIUtils.AddWithMargins(horizontalContainer, horizontalFrequencyField.Container, SketchRendererUIData.RegularIndentCorners);
            
            var horOffset = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("HorizontalLineOffset"), nameOverride:"Line Offset");
            SketchRendererUIUtils.AddWithMargins(horizontalContainer, horOffset.Container, SketchRendererUIData.RegularIndentCorners);
            
            var horThickness = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("HorizontalLineThickness"), nameOverride:"Line Thickness");
            SketchRendererUIUtils.AddWithMargins(horizontalContainer, horThickness.Container, SketchRendererUIData.RegularIndentCorners);
            
            var horTint = SketchRendererUI.SketchColorProperty(property.FindPropertyRelative("HorizontalLineTint"), nameOverride:"Line Tint");
            SketchRendererUIUtils.AddWithMargins(horizontalContainer, horTint.Container, SketchRendererUIData.RegularIndentCorners);
            
            var verticalContainer = new VisualElement();
            var verticalLabel = new Label("Vertical Lines");
            verticalContainer.Add(verticalLabel);
            SketchRendererUIUtils.AddWithMargins(assetField, verticalContainer, SketchRendererUIData.MajorIndentCorners);

            var verFrequencyManipulator = new ClampedFloatManipulator(0, 100);
            var verticalFrequencyField = SketchRendererUI.SketchFloatProperty(property.FindPropertyRelative("VerticalLineFrequency"), true, manipulator:verFrequencyManipulator, nameOverride:"Number of Lines");
            SketchRendererUIUtils.AddWithMargins(verticalContainer, verticalFrequencyField.Container, SketchRendererUIData.RegularIndentCorners);
            
            var verOffset = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("VerticalLineOffset"), nameOverride:"Line Offset");
            SketchRendererUIUtils.AddWithMargins(verticalContainer, verOffset.Container, SketchRendererUIData.RegularIndentCorners);
            
            var verThickness = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("VerticalLineThickness"), nameOverride:"Line Thickness");
            SketchRendererUIUtils.AddWithMargins(verticalContainer, verThickness.Container, SketchRendererUIData.RegularIndentCorners);
            
            var verTint = SketchRendererUI.SketchColorProperty(property.FindPropertyRelative("VerticalLineTint"), nameOverride:"Line Tint");
            SketchRendererUIUtils.AddWithMargins(verticalContainer, verTint.Container, SketchRendererUIData.RegularIndentCorners);
            
            var commonContainer = new VisualElement();
            var commonLabel = new Label("Shared");
            commonContainer.Add(commonLabel);
            SketchRendererUIUtils.AddWithMargins(assetField, commonContainer, SketchRendererUIData.MajorIndentCorners);
            
            var granularitySensitivityField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("NotebookLineGranularitySensitivity"), nameOverride:"Granularity Sensitivity");
            SketchRendererUIUtils.AddWithMargins(commonContainer, granularitySensitivityField.Container, SketchRendererUIData.RegularIndentCorners);
            
            return assetField;
        }
    }
}