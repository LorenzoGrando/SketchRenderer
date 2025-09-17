using SketchRenderer.Editor.UIToolkit;
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
        VisualElement directionSlider;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var strokeDataField = new VisualElement();
            
            customDirectionProperty = property.FindPropertyRelative("Direction");
            directionSlider = SketchRendererUI.SketchSlider("Base Direction", 0f, 1f, Direction_Changed, customDirectionProperty, Direction_OnTrack).Container;
            
            SketchRendererUIUtils.AddWithMargins(strokeDataField, directionSlider, CornerData.Empty);

            var thicknessRegion = new VisualElement();
            var thicknessLabel = new Label("Thickness");
            thicknessRegion.Add(thicknessLabel);
            var thicknessField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("Thickness"));
            SketchRendererUIUtils.AddWithMargins(thicknessRegion, thicknessField.Container, SketchRendererUIData.MajorIndentCorners);
            var thicknessConstraintField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("ThicknessFalloffConstraint"));
            SketchRendererUIUtils.AddWithMargins(thicknessRegion, thicknessConstraintField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SketchRendererUIUtils.AddWithMargins(strokeDataField,thicknessRegion, CornerData.Empty);
            
            
            var lengthRegion = new VisualElement(); 
            var lengthLabel = new Label("Length");
            lengthRegion.Add(lengthLabel);
            var lengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("Length"));
            SketchRendererUIUtils.AddWithMargins(lengthRegion, lengthField.Container, SketchRendererUIData.MajorIndentCorners);
            var lengthFalloffField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("LengthThicknessFalloff"));
            SketchRendererUIUtils.AddWithMargins(lengthRegion, lengthFalloffField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SketchRendererUIUtils.AddWithMargins(strokeDataField, lengthRegion, CornerData.Empty);
            
            var pressureRegion = new VisualElement(); 
            var pressureLabel = new Label("Pressure");
            pressureRegion.Add(pressureLabel);
            var pressureField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("Pressure"));
            SketchRendererUIUtils.AddWithMargins(pressureRegion, pressureField.Container, SketchRendererUIData.MajorIndentCorners);
            var pressureFalloffField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("PressureFalloff"));
            SketchRendererUIUtils.AddWithMargins(pressureRegion, pressureFalloffField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SketchRendererUIUtils.AddWithMargins(strokeDataField, pressureRegion, CornerData.Empty);
        
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

        internal void Direction_OnTrack(Slider slider, SerializedProperty property)
        {
            slider.SetValueWithoutNotify(MathUtilities.GetNormalizedAngle(property.vector4Value));
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}