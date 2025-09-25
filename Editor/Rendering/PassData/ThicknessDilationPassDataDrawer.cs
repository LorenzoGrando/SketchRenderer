using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEditor;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    [CustomPropertyDrawer(typeof(ThicknessDilationPassData))]
    internal class ThicknessDilationPassDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var passDataField = new VisualElement();
            
            SerializedProperty rangeProp = property.FindPropertyRelative("ThicknessRange");
            var rangeField = SketchRendererUI.SketchIntSliderPropertyWithInput(rangeProp, nameOverride: "Dilation Range");
            SketchRendererUIUtils.AddWithMargins(passDataField, rangeField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty strengthProp = property.FindPropertyRelative("ThicknessStrength");
            var strengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(strengthProp, nameOverride: "Dilation Strength");
            SketchRendererUIUtils.AddWithMargins(passDataField, strengthField.Container, SketchRendererUIData.MajorIndentCorners);
            
            return passDataField;
        }
    }
}