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
            var minHatchingField = new PropertyField(minHatchingProp);
            minHatchingField.label = "CrossHatching Start Threshold";
            minHatchingField.BindProperty(minHatchingProp);
            minHatchingField.RegisterValueChangeCallback(MinHatching_Changed);
            hatchingRegion.Add(minHatchingField);
            
            maxHatchingProp = serializedObject.FindProperty("MaxCrossHatchingThreshold");
            var maxHatchingField = new PropertyField(maxHatchingProp);
            maxHatchingField.label = "CrossHatching Mix Direction Threshold";
            maxHatchingField.BindProperty(maxHatchingProp);
            maxHatchingField.RegisterValueChangeCallback(MaxHatching_Changed);
            hatchingRegion.Add(maxHatchingField);
            
            baseAsset.Add(hatchingRegion);
            return baseAsset;
        }

        private void MinHatching_Changed(SerializedPropertyChangeEvent bind)
        {
            float newValue = bind.changedProperty.floatValue;
            float maxHatchingValue = maxHatchingProp.floatValue;
            bind.changedProperty.floatValue = Mathf.Min(newValue, maxHatchingValue);
            bind.changedProperty.serializedObject.ApplyModifiedProperties();
            Repaint();
        }

        private void MaxHatching_Changed(SerializedPropertyChangeEvent bind)
        {
            float newValue = bind.changedProperty.floatValue;
            float minHatchingValue = minHatchingProp.floatValue;
            bind.changedProperty.floatValue = Mathf.Max(newValue, minHatchingValue);
            bind.changedProperty.serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    }
}