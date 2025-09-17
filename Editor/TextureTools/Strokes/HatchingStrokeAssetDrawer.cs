using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.Strokes
{
    [CustomEditor(typeof(HatchingStrokeAsset))]
    public class HatchingStrokeAssetDrawer : StrokeAssetDrawer
    {
        SerializedProperty minHatchingProp;
        SerializedProperty maxHatchingProp;
        
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            
            VisualElement baseAsset = base.CreateInspectorGUI();
            var hatchingRegion = new VisualElement();
            
            var hatchingLabel = new Label { text = "Hatching Settings" };
            hatchingRegion.Add(hatchingLabel);
            
            minHatchingProp = serializedObject.FindProperty("MinCrossHatchingThreshold");
            var minHatchingField = SketchRendererUI.SketchFloatSliderPropertyWithInput(minHatchingProp, nameOverride: "CrossHatching Start", MinHatching_Changed);
            SketchRendererUIUtils.AddWithMargins(hatchingRegion, minHatchingField.Container, SketchRendererUIData.MajorIndentCorners);
            
            maxHatchingProp = serializedObject.FindProperty("MaxCrossHatchingThreshold");
            var maxHatchingField = SketchRendererUI.SketchFloatSliderPropertyWithInput(maxHatchingProp, nameOverride: "CrossHatching Mix Start", MaxHatching_Changed);
            SketchRendererUIUtils.AddWithMargins(hatchingRegion, maxHatchingField.Container, SketchRendererUIData.MajorIndentCorners);
            
            baseAsset.Add(hatchingRegion);
            return baseAsset;
        }

        private void MinHatching_Changed(ChangeEvent<float> bind)
        {
            float newValue = bind.newValue;
            float maxHatchingValue = maxHatchingProp.floatValue;
            minHatchingProp.floatValue = Mathf.Min(newValue, maxHatchingValue);
            minHatchingProp.serializedObject.ApplyModifiedProperties();
            Repaint();
        }

        private void MaxHatching_Changed(ChangeEvent<float>  bind)
        {
            float newValue = bind.newValue;
            float minHatchingValue = minHatchingProp.floatValue;
            maxHatchingProp.floatValue = Mathf.Max(newValue, minHatchingValue);
            maxHatchingProp.serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    }
}