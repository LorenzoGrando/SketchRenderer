using SketchRenderer.Runtime.Extensions;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.Strokes
{
    [CustomPropertyDrawer(typeof(StrokeData))]
    public class StrokeDataDrawer : PropertyDrawer
    {
        SerializedProperty customDirectionProperty;
        Slider directionSlider;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var strokeDataField = new VisualElement();
            
            var strokeDataLabel = new Label("Stroke Data");
            strokeDataField.Add(strokeDataLabel);
            
            customDirectionProperty = property.FindPropertyRelative("Direction");
            directionSlider = new Slider("Base Direction", 0f, 1f);
            directionSlider.RegisterValueChangedCallback(Direction_Changed);
            directionSlider.TrackPropertyValue(customDirectionProperty, Direction_OnTrack);
            Direction_OnTrack(customDirectionProperty);
            strokeDataField.Add(directionSlider);

            var thicknessRegion = new VisualElement();
            var thicknessLabel = new Label("Thickness");
            thicknessRegion.Add(thicknessLabel);
            var thicknessField = new PropertyField(property.FindPropertyRelative("Thickness"));
            thicknessRegion.Add(thicknessField);
            var thicknessConstraintField = new PropertyField(property.FindPropertyRelative("ThicknessFalloffConstraint"));
            thicknessRegion.Add(thicknessConstraintField);
            
            strokeDataField.Add(thicknessRegion);
            
            var lengthRegion = new VisualElement(); 
            var lengthLabel = new Label("Length");
            lengthRegion.Add(lengthLabel);
            var lengthField = new PropertyField(property.FindPropertyRelative("Length"));
            lengthRegion.Add(lengthField);
            var lengthFalloffField = new PropertyField(property.FindPropertyRelative("LengthThicknessFalloff"));
            lengthRegion.Add(lengthFalloffField);
            
            strokeDataField.Add(lengthRegion);
            
            var pressureRegion = new VisualElement(); 
            var pressureLabel = new Label("Pressure");
            pressureRegion.Add(pressureLabel);
            var pressureField = new PropertyField(property.FindPropertyRelative("Pressure"));
            pressureRegion.Add(pressureField);
            var pressureFalloffField = new PropertyField(property.FindPropertyRelative("PressureFalloff"));
            pressureRegion.Add(pressureFalloffField);
            
            strokeDataField.Add(pressureRegion);
        
            return strokeDataField;
        }
        
        internal void Direction_Changed(ChangeEvent<float> bind)
        {
            var so = customDirectionProperty.serializedObject;
            so.Update();
            Vector4 direction = MathUtilities.GetUnitDirection(bind.newValue * Mathf.PI * 2f);
            customDirectionProperty.vector4Value = direction;
            so.ApplyModifiedProperties();
        }

        internal void Direction_OnTrack(SerializedProperty property)
        {
            directionSlider.SetValueWithoutNotify(MathUtilities.GetNormalizedAngle(property.vector4Value));
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}