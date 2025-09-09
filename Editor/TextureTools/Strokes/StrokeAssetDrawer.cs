using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.Strokes
{
    [CustomEditor(typeof(StrokeAsset))]
    public class StrokeAssetDrawer : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            
            var assetField = new VisualElement();
            var settingsField = new VisualElement();
            
            SerializedProperty strokeDataProp = serializedObject.FindProperty("StrokeData");
            var strokeDataField = new PropertyField(strokeDataProp);
            strokeDataField.BindProperty(strokeDataProp);
            strokeDataField.RegisterValueChangeCallback(StrokeData_Changed);
            SketchRendererUIUtils.AddWithMargins(settingsField, strokeDataField, SketchRendererUIData.MinorFieldMargins);
            
            SerializedProperty variationDataProp = serializedObject.FindProperty("VariationData");
            var variationDataField = new PropertyField(variationDataProp);
            variationDataField.BindProperty(variationDataProp);
            SketchRendererUIUtils.AddWithMargins(settingsField, variationDataField, SketchRendererUIData.MinorFieldMargins);
            
            SerializedProperty falloffProp = serializedObject.FindProperty("SelectedFalloffFunction");
            var falloffField = SketchRendererUI.SketchEnumProperty(falloffProp, ((StrokeAsset)target).SelectedFalloffFunction, nameOverride:"Falloff Function");
            SketchRendererUIUtils.AddWithMargins(settingsField, falloffField.Container, SketchRendererUIData.MinorFieldMargins);
            
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