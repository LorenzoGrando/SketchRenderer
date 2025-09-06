using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;
using UnityEditor;
using UnityEditor.UIElements;
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
            
            var tonesField = new IntegerField("Number of Tones");
            tonesField.AddManipulator(new ClampedIntegerManipulator(tonesField, 1, 9));
            tonesField.RegisterValueChangedCallback(Tones_Changed);
            tonesField.SetValueWithoutNotify(asset.ExpectedTones);
            assetField.Add(tonesField);
            
            SerializedProperty forceWhiteProp = serializedObject.FindProperty("ForceFirstToneFullWhite");
            var forceWhite = new PropertyField(forceWhiteProp);
            forceWhite.BindProperty(forceWhiteProp);
            forceWhite.label = "Set First Tone to full white";
            assetField.Add(forceWhite);
            
            SerializedProperty forceBlackProp = serializedObject.FindProperty("ForceFinalToneFullBlack");
            var forceBlack = new PropertyField(forceBlackProp);
            forceBlack.BindProperty(forceBlackProp);
            forceBlack.label = "Set Last Tone to full black";
            assetField.Add(forceBlack);
            
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