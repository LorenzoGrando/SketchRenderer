using System;
using SketchRenderer.Editor.TextureTools;
using SketchRenderer.Editor.Utils;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    public class AssetsDirectoryManipulator : Manipulator
    {
        private TextField pathField;
        
        public event Action<string> OnValidated;

        public AssetsDirectoryManipulator(TextField pathField)
        {
            this.pathField = pathField;

            this.pathField.RegisterValueChangedCallback(OnValueChanged);
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            string path = ConvertToAssetsPath(evt.newValue);
            pathField.SetValueWithoutNotify(path);
            OnValidated?.Invoke(path);
        }
        
        protected override void RegisterCallbacksOnTarget() { }
        protected override void UnregisterCallbacksFromTarget()
        {
            pathField.UnregisterValueChangedCallback(OnValueChanged);
        }

        private string ConvertToAssetsPath(string path)
        {
            return SketchAssetCreationWrapper.ConvertToAssetsPath(path);
        }
    }
}