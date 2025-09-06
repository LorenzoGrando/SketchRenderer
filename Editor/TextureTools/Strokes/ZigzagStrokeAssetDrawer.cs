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
            var directionOffsetField = new PropertyField(directionOffsetProp);
            directionOffsetField.BindProperty(directionOffsetProp);
            zigzagRegion.Add(directionOffsetField);
            
            SerializedProperty lengthModifierProp = serializedObject.FindProperty("SubStrokeLengthMultiplier");
            var lengthModifierField = new PropertyField(lengthModifierProp);
            lengthModifierField.BindProperty(lengthModifierProp);
            zigzagRegion.Add(lengthModifierField);
            
            SerializedProperty onlyZigProp = serializedObject.FindProperty("OnlyMultiplyZigStroke");
            var onlyZigField = new PropertyField(onlyZigProp);
            onlyZigField.BindProperty(onlyZigProp);
            zigzagRegion.Add(onlyZigField);
            
            SerializedProperty repetitionsProp = serializedObject.FindProperty("Repetitions");
            var repetitionsField = new PropertyField(repetitionsProp);
            repetitionsField.BindProperty(repetitionsProp);
            zigzagRegion.Add(repetitionsField);
            
            baseAsset.Add(zigzagRegion);
            return baseAsset;
        }
    }
}