using SketchRenderer.Editor.UIToolkit;
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
            var firstDirectionField = SketchRendererUI.SketchFloatSliderPropertyWithInput(firstDirectionProp, nameOverride:"Direction Offset");
            SketchRendererUIUtils.AddWithMargins(firstStrokeRegion, firstDirectionField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty firstLengthProp = serializedObject.FindProperty("FirstSubStrokeLengthMultiplier");
            var firstLengthField = SketchRendererUI.SketchFloatProperty(firstLengthProp, nameOverride:"Length Multiplier");
            SketchRendererUIUtils.AddWithMargins(firstStrokeRegion, firstLengthField.Container, SketchRendererUIData.MajorIndentCorners);
            SketchRendererUIUtils.AddWithMargins(featheringRegion, firstStrokeRegion, SketchRendererUIData.MajorIndentCorners);
            
            var secondStrokeRegion = new VisualElement();
            var secondStrokeLabel = new Label("Second Feathering Stroke");
            secondStrokeRegion.Add(secondStrokeLabel);
            SerializedProperty secondDirectionProp = serializedObject.FindProperty("SecondSubStrokeDirectionOffset");
            var secondDirectionField = SketchRendererUI.SketchFloatSliderPropertyWithInput(secondDirectionProp, nameOverride:"Direction Offset");
            SketchRendererUIUtils.AddWithMargins(secondStrokeRegion, secondDirectionField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty secondLengthProp = serializedObject.FindProperty("SecondSubStrokeLengthMultiplier");
            var secondLengthField = SketchRendererUI.SketchFloatProperty(secondLengthProp, nameOverride:"Length Multiplier");
            SketchRendererUIUtils.AddWithMargins(secondStrokeRegion, secondLengthField.Container, SketchRendererUIData.MajorIndentCorners);
            SketchRendererUIUtils.AddWithMargins(featheringRegion, secondStrokeRegion, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty repetitionsProp = serializedObject.FindProperty("Repetitions");
            var repetitionsField = SketchRendererUI.SketchIntSliderPropertyWithInput(repetitionsProp);
            SketchRendererUIUtils.AddWithMargins(featheringRegion, repetitionsField.Container, SketchRendererUIData.MajorIndentCorners);
            
            baseAsset.Add(featheringRegion);
            return baseAsset;
        }
    }
}