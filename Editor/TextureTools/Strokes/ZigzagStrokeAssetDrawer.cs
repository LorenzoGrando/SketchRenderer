using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.Strokes
{
    [CustomEditor(typeof(ZigzagStrokeAsset))]
    public class ZigzagStrokeAssetDrawer : StrokeAssetDrawer
    {
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            
            VisualElement baseAsset = base.CreateInspectorGUI();
            var zigzagRegion = new VisualElement();
            
            var zigzagLabel = new Label { text = "Zigzag Settings" };
            zigzagRegion.Add(zigzagLabel);
            
            SerializedProperty directionOffsetProp = serializedObject.FindProperty("SubStrokeDirectionOffset");
            var directionOffsetField = SketchRendererUI.SketchFloatSliderPropertyWithInput(directionOffsetProp);
            SketchRendererUIUtils.AddWithMargins(zigzagRegion, directionOffsetField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty lengthModifierProp = serializedObject.FindProperty("SubStrokeLengthMultiplier");
            var lengthModifierField = SketchRendererUI.SketchFloatProperty(lengthModifierProp);
            SketchRendererUIUtils.AddWithMargins(zigzagRegion, lengthModifierField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty onlyZigProp = serializedObject.FindProperty("OnlyMultiplyZigStroke");
            var onlyZigField = SketchRendererUI.SketchBoolProperty(onlyZigProp);
            SketchRendererUIUtils.AddWithMargins(zigzagRegion, onlyZigField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty repetitionsProp = serializedObject.FindProperty("Repetitions");
            var repetitionsField = SketchRendererUI.SketchIntSliderPropertyWithInput(repetitionsProp);
            SketchRendererUIUtils.AddWithMargins(zigzagRegion, repetitionsField.Container, SketchRendererUIData.MajorIndentCorners);
            
            baseAsset.Add(zigzagRegion);
            return baseAsset;
        }
    }
}