using System;
using SketchRenderer.Editor.TextureTools;
using SketchRenderer.Editor.Utils;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    public class AssetsDirectoryManipulator : Manipulator, ISketchManipulator<TextField>
    {
        private TextField pathField;
        
        public event Action<string> OnValidated;
        
        public void Initialize(TextField field)
        {
            pathField = field;
            pathField.RegisterValueChangedCallback(OnValueChanged);
        }

        public Manipulator GetBaseManipulator()
        {
            return this;
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