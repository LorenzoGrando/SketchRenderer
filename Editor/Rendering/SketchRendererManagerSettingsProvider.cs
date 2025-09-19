using SketchRenderer.Runtime.Data;
using SketchRenderer.Editor.UIToolkit;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    internal class SketchRendererManagerSettingsProvider : SettingsProvider
    {
        internal SketchRendererManagerSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) 
            : base(path, scope) { }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            ConstructGUI(rootElement);
        }

        [SettingsProvider]
        internal static SettingsProvider CreateSettingsProvider()
        {
            SketchRendererManagerSettingsProvider provider = new(SketchRendererData.PackageProjectSettingsPath);
            
            return provider;
        }

        internal void ConstructGUI(VisualElement root)
        {
            var labelArea = SketchRendererUI.SketchMajorArea("Sketch Renderer Settings", applyMargins: false, fontSize: SketchRendererUIData.MajorTitleHeight);
            SketchRendererUIUtils.AddWithMargins(root, labelArea, SketchRendererUIData.BaseFieldMargins);
            var contextField = SketchRendererUI.SketchObjectField("Renderer Context", typeof(SketchRendererContext),
                SketchRendererManager.CurrentRendererContext, changeCallback:RendererContext_Changed);
            SketchRendererUIUtils.AddWithMargins(root, contextField.Container, SketchRendererUIData.MajorIndentCorners);
            
            if(contextField.Field.value == null)
                SketchRendererManager.ClearRenderer();
        }

        private void RendererContext_Changed(ChangeEvent<Object> evt)
        {
            SketchRendererManager.CurrentRendererContext = (SketchRendererContext)evt.newValue;
            if (evt.newValue != null)
                SketchRendererManager.UpdateRendererToCurrentContext();
            else
                SketchRendererManager.ClearRenderer();
        }
    }
}