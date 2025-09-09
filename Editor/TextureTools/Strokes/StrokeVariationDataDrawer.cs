using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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
            
            var directionField = new PropertyField(property.FindPropertyRelative("DirectionVariationRange"));
            variationDataField.Add(directionField);
            var thicknessField = new PropertyField(property.FindPropertyRelative("ThicknessVariationRange"));
            variationDataField.Add(thicknessField);
            var lengthField = new PropertyField(property.FindPropertyRelative("LengthVariationRange"));
            variationDataField.Add(lengthField);
            var pressureField = new PropertyField(property.FindPropertyRelative("PressureVariationRange"));
            variationDataField.Add(pressureField);
        
            return variationDataField;
        }
    }
}