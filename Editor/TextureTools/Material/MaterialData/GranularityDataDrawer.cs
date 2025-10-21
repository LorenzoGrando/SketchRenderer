using SketchRenderer.Editor.UIToolkit;
using TextureTools.Material;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.MaterialData
{
    [CustomPropertyDrawer(typeof(GranularityData))]
    public class GranularityDataDrawer : PropertyDrawer
    {
        SerializedProperty minStrengthProp;
        SerializedProperty maxStrengthProp;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var assetField = new VisualElement();

            var scaleManipulator = new ClampedVector2IntManipulator(2, 20);
            var scaleField = SketchRendererUI.SketchVector2IntProperty(property.FindPropertyRelative("Scale"), scaleManipulator);
            SketchRendererUIUtils.AddWithMargins(assetField, scaleField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var detailField = SketchRendererUI.SketchIntSliderPropertyWithInput(property.FindPropertyRelative("DetailLevel"));
            SketchRendererUIUtils.AddWithMargins(assetField, detailField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var detailFrequencyField = SketchRendererUI.SketchIntSliderPropertyWithInput(property.FindPropertyRelative("DetailFrequency"));
            SketchRendererUIUtils.AddWithMargins(assetField, detailFrequencyField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var detailPersistenceField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("DetailPersistence"));
            SketchRendererUIUtils.AddWithMargins(assetField, detailPersistenceField.Container, SketchRendererUIData.MajorIndentCorners);
            
            minStrengthProp = property.FindPropertyRelative("MinimumGranularity");
            var minimumStrengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(minStrengthProp, nameOverride:"Minimum Strength", evt => MinStrength_Changed(evt, maxStrengthProp.floatValue));
            SketchRendererUIUtils.AddWithMargins(assetField, minimumStrengthField.Container, SketchRendererUIData.MajorIndentCorners);
            
            maxStrengthProp = property.FindPropertyRelative("MaximumGranularity");
            var maximumStrengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(maxStrengthProp, nameOverride:"Maximum Strength", evt => MaxStrength_Changed(evt, minStrengthProp.floatValue));
            SketchRendererUIUtils.AddWithMargins(assetField, maximumStrengthField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var tintField = SketchRendererUI.SketchColorProperty(property.FindPropertyRelative("GranularityTint"), nameOverride:"Tint");
            SketchRendererUIUtils.AddWithMargins(assetField, tintField.Container, SketchRendererUIData.MajorIndentCorners);
            
            return assetField;
        }
        
        private void MinStrength_Changed(ChangeEvent<float> bind, float maxValue)
        {
            float newValue = bind.newValue;
            minStrengthProp.floatValue = Mathf.Min(newValue, maxValue);
            minStrengthProp.serializedObject.ApplyModifiedProperties();
        }

        private void MaxStrength_Changed(ChangeEvent<float>  bind, float minValue)
        {
            float newValue = bind.newValue;
            maxStrengthProp.floatValue = Mathf.Max(newValue, minValue);
            maxStrengthProp.serializedObject.ApplyModifiedProperties();
        }
    }
}