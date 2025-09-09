using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.Strokes
{
    [CustomEditor(typeof(FeatheringStrokeAsset))]
    public class FeatheringStrokeAssetDrawer : StrokeAssetDrawer
    {
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            
            VisualElement baseAsset = base.CreateInspectorGUI();
            var featheringRegion = new VisualElement();
            
            var featheringLabel = new Label { text = "Feathering Settings" };
            featheringRegion.Add(featheringLabel);
            
            var firstStrokeRegion = new VisualElement();
            var firstStrokeLabel = new Label("First Feathering Stroke");
            firstStrokeRegion.Add(firstStrokeLabel);
            SerializedProperty firstDirectionProp = serializedObject.FindProperty("FirstSubStrokeDirectionOffset");
            var firstDirectionField = new PropertyField(firstDirectionProp);
            firstDirectionField.BindProperty(firstDirectionProp);
            firstStrokeRegion.Add(firstDirectionField);
            
            SerializedProperty firstLengthProp = serializedObject.FindProperty("FirstSubStrokeLengthMultiplier");
            var firstLengthField = new PropertyField(firstLengthProp);
            firstLengthField.BindProperty(firstLengthProp);
            firstStrokeRegion.Add(firstLengthField);
            featheringRegion.Add(firstStrokeRegion);
            
            var secondStrokeRegion = new VisualElement();
            var secondStrokeLabel = new Label("Second Feathering Stroke");
            secondStrokeRegion.Add(secondStrokeLabel);
            SerializedProperty secondDirectionProp = serializedObject.FindProperty("SecondSubStrokeDirectionOffset");
            var secondDirectionField = new PropertyField(secondDirectionProp);
            secondDirectionField.BindProperty(secondDirectionProp);
            secondStrokeRegion.Add(secondDirectionField);
            
            SerializedProperty secondLengthProp = serializedObject.FindProperty("SecondSubStrokeLengthMultiplier");
            var secondLengthField = new PropertyField(secondLengthProp);
            secondLengthField.BindProperty(secondLengthProp);
            secondStrokeRegion.Add(secondLengthField);
            featheringRegion.Add(secondStrokeRegion);
            
            SerializedProperty repetitionsProp = serializedObject.FindProperty("Repetitions");
            var repetitionsField = new PropertyField(repetitionsProp);
            repetitionsField.BindProperty(repetitionsProp);
            featheringRegion.Add(repetitionsField);
            
            baseAsset.Add(featheringRegion);
            return baseAsset;
        }
    }
}