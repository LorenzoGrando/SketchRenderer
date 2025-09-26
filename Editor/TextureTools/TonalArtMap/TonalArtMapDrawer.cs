using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools
{
    [CustomEditor(typeof(TonalArtMapAsset))]
    public class TonalArtMapDrawer : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var assetField = new VisualElement();
            
            TonalArtMapAsset asset = (TonalArtMapAsset)target;
            
            var tonesField = SketchRendererUI.SketchIntegerField("Number of Tones", asset.ExpectedTones, isDelayed:true, manipulator:new ClampedIntegerManipulator(1, 9), changeCallback:Tones_Changed);
            tonesField.Field.TrackPropertyValue(serializedObject.FindProperty("ExpectedTones"));
            SketchRendererUIUtils.AddWithMargins(assetField, tonesField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty forceWhiteProp = serializedObject.FindProperty("ForceFirstToneFullWhite");
            var forceWhite = SketchRendererUI.SketchBoolProperty(forceWhiteProp, nameOverride:"Set First Tone to Full White");
            SketchRendererUIUtils.AddWithMargins(assetField, forceWhite.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty forceBlackProp = serializedObject.FindProperty("ForceFinalToneFullBlack");
            var forceBlack = SketchRendererUI.SketchBoolProperty(forceBlackProp, nameOverride:"Set Last Tone to Full Black");
            SketchRendererUIUtils.AddWithMargins(assetField, forceBlack.Container, SketchRendererUIData.MajorIndentCorners);
            
            return assetField;
        }

        private void Tones_Changed(ChangeEvent<int> evt)
        {
            serializedObject.Update();
            TonalArtMapAsset asset = (TonalArtMapAsset)target;
            asset.ExpectedTones = evt.newValue;
            serializedObject.ApplyModifiedProperties();
        }
    }
}