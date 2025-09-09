using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.Strokes
{
    [CustomPropertyDrawer(typeof(StrokeVariationData))]
    public class StrokeVariationDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var variationDataField = new VisualElement();
            
            var variationDataLabel = new Label("Variation Data");
            variationDataField.Add(variationDataLabel);
            
            var directionField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("DirectionVariationRange"));
            SketchRendererUIUtils.AddWithMargins(variationDataField,directionField.Container, SketchRendererUIData.MajorIndentCorners );
            var thicknessField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("ThicknessVariationRange"));
            SketchRendererUIUtils.AddWithMargins(variationDataField,thicknessField.Container, SketchRendererUIData.MajorIndentCorners );
            var lengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("LengthVariationRange"));
            SketchRendererUIUtils.AddWithMargins(variationDataField,lengthField.Container, SketchRendererUIData.MajorIndentCorners );
            var pressureField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("PressureVariationRange"));
            SketchRendererUIUtils.AddWithMargins(variationDataField,pressureField.Container, SketchRendererUIData.MajorIndentCorners );
        
            return variationDataField;
        }
    }
}