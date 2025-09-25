using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using SketchRenderer.ShaderLibrary;
using UnityEditor;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    [CustomPropertyDrawer(typeof(SketchCompositionPassData))]
    public class SketchCompositionPassDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var passDataField = new VisualElement();
            var passDataFieldLabel = new Label("Composition");
            passDataField.Add(passDataFieldLabel);
            
            var debugFieldOffset = new VisualElement();
            SerializedProperty debugProp = property.FindPropertyRelative("debugMode");
            SketchCompositionPassData.DebugMode debugMode = (SketchCompositionPassData.DebugMode)debugProp.enumValueIndex;
            var debugModeField = SketchRendererUI.SketchEnumProperty(debugProp, debugMode); 
            SketchRendererUIUtils.AddWithMargins(debugFieldOffset, debugModeField.Container, SketchRendererUIData.MajorIndentCorners);
            SketchRendererUIUtils.AddWithMargins(passDataField, debugFieldOffset, SketchRendererUIData.BaseFieldMargins);
            
            SerializedProperty outlineColorProp = property.FindPropertyRelative("OutlineStrokeColor");
            var outlineColorField = SketchRendererUI.SketchColorProperty(outlineColorProp);
            SketchRendererUIUtils.AddWithMargins(passDataField, outlineColorField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty shadingColorProp = property.FindPropertyRelative("ShadingStrokeColor");
            var shadingColorField = SketchRendererUI.SketchColorProperty(shadingColorProp);
            SketchRendererUIUtils.AddWithMargins(passDataField, shadingColorField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty blendProp = property.FindPropertyRelative("StrokeBlendMode");
            BlendingOperations blendMode = (BlendingOperations)blendProp.enumValueIndex;
            var blendModeField = SketchRendererUI.SketchEnumProperty(blendProp, blendMode); 
            SketchRendererUIUtils.AddWithMargins(passDataField, blendModeField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty blendStrengthProp = property.FindPropertyRelative("BlendStrength");
            var blendStrengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(blendStrengthProp);
            SketchRendererUIUtils.AddWithMargins(passDataField, blendStrengthField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty materialAccumulationProp = property.FindPropertyRelative("MaterialAccumulationStrength");
            var materialAccumulationFiled = SketchRendererUI.SketchFloatSliderPropertyWithInput(materialAccumulationProp, nameOverride: "Graphite Accumulation");
            SketchRendererUIUtils.AddWithMargins(passDataField, materialAccumulationFiled.Container, SketchRendererUIData.MajorIndentCorners);
            
            return passDataField;
        }
    }
}