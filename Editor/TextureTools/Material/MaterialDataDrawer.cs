using SketchRenderer.Editor.UIToolkit;
using TextureTools.Material;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools
{
    [CustomEditor(typeof(MaterialDataAsset))]
    public class MaterialDataDrawer : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            
            var assetField = new VisualElement();
            
            //- Granularity
            SerializedProperty useGranularityProp = serializedObject.FindProperty("UseGranularity");
            var granularityFoldout = SketchRendererUI.SketchFoldoutWithToggle("Granularity", useGranularityProp, false);
            
            SerializedProperty granularityDataProp = serializedObject.FindProperty("Granularity");
            var granularityDataField = new PropertyField(granularityDataProp);
            granularityDataField.BindProperty(granularityDataProp);
            granularityDataField.TrackPropertyValue(granularityDataProp);
            granularityDataField.RegisterValueChangeCallback(MaterialData_Changed);
            SketchRendererUIUtils.AddWithMargins(granularityFoldout, granularityDataField, CornerData.Empty);
            
            SketchRendererUIUtils.AddWithMargins(assetField, granularityFoldout, SketchRendererUIData.MajorIndentCorners);
            
            //- Wrinkles
            SerializedProperty useWrinklesProp = serializedObject.FindProperty("UseWrinkles");
            var wrinklesFoldout = SketchRendererUI.SketchFoldoutWithToggle("Wrinkles", useWrinklesProp, false);
            
            SerializedProperty wrinklesDataProp = serializedObject.FindProperty("Wrinkles");
            var wrinklesDataField = new PropertyField(wrinklesDataProp);
            wrinklesDataField.BindProperty(wrinklesDataProp);
            wrinklesDataField.TrackPropertyValue(wrinklesDataProp);
            wrinklesDataField.RegisterValueChangeCallback(MaterialData_Changed);
            SketchRendererUIUtils.AddWithMargins(wrinklesFoldout, wrinklesDataField, CornerData.Empty);
            
            SketchRendererUIUtils.AddWithMargins(assetField, wrinklesFoldout, SketchRendererUIData.MajorIndentCorners);
            
            //- LaidLine
            SerializedProperty useLaidLinesProp = serializedObject.FindProperty("UseLaidLines");
            var laidLineFouldout = SketchRendererUI.SketchFoldoutWithToggle("Laid Lines", useLaidLinesProp, false);
            
            SerializedProperty laidLineDataProp = serializedObject.FindProperty("LaidLines");
            var laidLineDataField = new PropertyField(laidLineDataProp);
            laidLineDataField.BindProperty(laidLineDataProp);
            laidLineDataField.TrackPropertyValue(laidLineDataProp);
            laidLineDataField.RegisterValueChangeCallback(MaterialData_Changed);
            SketchRendererUIUtils.AddWithMargins(laidLineFouldout, laidLineDataField, CornerData.Empty);
            
            SketchRendererUIUtils.AddWithMargins(assetField, laidLineFouldout, SketchRendererUIData.MajorIndentCorners);
            
            //- Crumple
            SerializedProperty useCrumples = serializedObject.FindProperty("UseCrumples");
            var crumplesFoldout = SketchRendererUI.SketchFoldoutWithToggle("Crumple", useCrumples, false);
            
            SerializedProperty crumpleDataProp = serializedObject.FindProperty("Crumples");
            var crumpleDataField = new PropertyField(crumpleDataProp);
            crumpleDataField.BindProperty(crumpleDataProp);
            crumpleDataField.TrackPropertyValue(crumpleDataProp);
            crumpleDataField.RegisterValueChangeCallback(MaterialData_Changed);
            SketchRendererUIUtils.AddWithMargins(crumplesFoldout, crumpleDataField, CornerData.Empty);
            
            SketchRendererUIUtils.AddWithMargins(assetField, crumplesFoldout, SketchRendererUIData.MajorIndentCorners);
            
            //- Notebook Lines
            SerializedProperty useNotebookLines = serializedObject.FindProperty("UseNotebookLines");
            var notebookLinesFoldout = SketchRendererUI.SketchFoldoutWithToggle("Notebook Lines", useNotebookLines, false);
            
            SerializedProperty notebookLinesProp = serializedObject.FindProperty("NotebookLines");
            var notebookLinesField = new PropertyField(notebookLinesProp);
            notebookLinesField.BindProperty(notebookLinesProp);
            notebookLinesField.TrackPropertyValue(notebookLinesProp);
            notebookLinesField.RegisterValueChangeCallback(MaterialData_Changed);
            SketchRendererUIUtils.AddWithMargins(notebookLinesFoldout, notebookLinesField, CornerData.Empty);
            
            SketchRendererUIUtils.AddWithMargins(assetField, notebookLinesFoldout, SketchRendererUIData.MajorIndentCorners);

            
            serializedObject.ApplyModifiedProperties();
            return assetField;
        }
        
        internal void MaterialData_Changed(SerializedPropertyChangeEvent prop)
        {
            serializedObject.Update();
            EditorUtility.SetDirty(prop.changedProperty.serializedObject.targetObject);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}