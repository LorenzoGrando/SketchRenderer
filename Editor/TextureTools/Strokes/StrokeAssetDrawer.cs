using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.Strokes
{
    [CustomEditor(typeof(StrokeAsset))]
    public class StrokeAssetEditor : UnityEditor.Editor
    {
        
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            
            var assetField = new VisualElement();
            var settingsField = new VisualElement();
            
            var settingsLabel = new Label("General Stroke Data");
            settingsField.Add(settingsLabel);

            SerializedProperty strokeDataProp = serializedObject.FindProperty("StrokeData");
            var strokeDataField = new PropertyField(strokeDataProp);
            strokeDataField.BindProperty(strokeDataProp);
            strokeDataField.RegisterValueChangeCallback(StrokeData_Changed);
            settingsField.Add(strokeDataField);
            
            SerializedProperty variationDataProp = serializedObject.FindProperty("VariationData");
            var variationDataField = new PropertyField(variationDataProp);
            variationDataField.BindProperty(variationDataProp);
            settingsField.Add(variationDataField);
            
            SerializedProperty falloffProp = serializedObject.FindProperty("SelectedFalloffFunction");
            var falloffField = new PropertyField(falloffProp);
            falloffField.BindProperty(falloffProp);
            settingsField.Add(falloffField);
            
            assetField.Add(settingsField);
            
            serializedObject.ApplyModifiedProperties();
            return assetField;
        }

        internal void StrokeData_Changed(SerializedPropertyChangeEvent prop)
        {
            serializedObject.Update();
            EditorUtility.SetDirty(prop.changedProperty.serializedObject.targetObject);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}