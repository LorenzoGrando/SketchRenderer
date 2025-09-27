using System.Collections;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    internal class SketchRendererManagerSettingsProvider : SettingsProvider
    {
        internal SketchRendererManagerSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) 
            : base(path, scope) { }

        internal bool visible;
        internal VisualElement root;
        internal SketchElement<ObjectField> contextField;
        internal SketchRendererContext listenerContext;
        
        private Coroutine delayedValidateRoutine;
        private float validateFrameProgress;
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            ConstructGUI(rootElement);
            visible = true;
        }

        public override void OnDeactivate()
        {
            visible = false;
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
            contextField = SketchRendererUI.SketchObjectField("Active Renderer Context", typeof(SketchRendererContext),
                SketchRendererManager.CurrentRendererContext, changeCallback:RendererContext_Changed);

            if (contextField.Field.value == null)
            {
                SketchRendererUIUtils.AddWithMargins(root, contextField.Container, SketchRendererUIData.BaseFieldNoVerticalMargins);
                SketchRendererManager.ClearRenderer();
                var createButton = SketchRendererUI.SketchMajorButton("Create Renderer Context", CreateAndApplyActive_Clicked);
                SketchRendererUIUtils.AddWithMargins(root, createButton, SketchRendererUIData.BaseFieldMargins);
            }
            else
            {
                SketchRendererUIUtils.AddWithMargins(root, contextField.Container, SketchRendererUIData.BaseFieldMargins);
                if (listenerContext != null)
                    listenerContext.OnValidated -= RendererContext_OnValidate;
                
                listenerContext = (SketchRendererContext)contextField.Field.value;
                listenerContext.OnValidated += RendererContext_OnValidate;
                if (listenerContext.IsDirty)
                {
                    if (SketchRendererManager.ManagerSettings.AlwaysUpdateRendererData)
                        UpdateOnSettingsChange();
                    else
                    {
                        var helpBox = GetContextDirtyHelpBox();
                        SketchRendererUIUtils.AddWithMargins(root, helpBox,
                            SketchRendererUIData.BaseFieldNoVerticalMargins);
                        var reapplyContextField = SketchRendererUI.SketchMajorButton("Reapply Context to Renderer Data",
                            ReapplyContext_Clicked);
                        SketchRendererUIUtils.AddWithMargins(root, reapplyContextField,
                            SketchRendererUIData.BaseFieldMargins);
                    }
                }
            }

            SerializedObject settingsObject = new SerializedObject(SketchRendererManager.ManagerSettings);
            SerializedProperty alwaysApplyProp = settingsObject.FindProperty("AlwaysUpdateRendererData");
            var alwaysApplyField = SketchRendererUI.SketchBoolProperty(alwaysApplyProp, nameOverride: "Update on Settings Change");
            SketchRendererUIUtils.AddWithMargins(root, alwaysApplyField.Container, SketchRendererUIData.BaseFieldMargins);
            alwaysApplyField.Field.RegisterValueChangedCallback(evt => ValidateSettings());
            
            SerializedProperty sceneViewProp = settingsObject.FindProperty("DisplayInSceneView");
            var sceneViewField = SketchRendererUI.SketchBoolProperty(sceneViewProp);
            SketchRendererUIUtils.AddWithMargins(root, sceneViewField.Container, SketchRendererUIData.BaseFieldMargins);
            sceneViewField.Field.RegisterValueChangedCallback(evt => ValidateSettings());
            
            this.root = root;
        }

        private void RendererContext_Changed(ChangeEvent<Object> evt)
        {
            if (evt.newValue != null)
                UpdateActiveRendererContext((SketchRendererContext)evt.newValue);
            else
            {
                SketchRendererManager.CurrentRendererContext = null;
                SketchRendererManager.ClearRenderer();
            }

            ForceRepaint();
        }

        private void UpdateActiveRendererContext(SketchRendererContext context)
        {
            SketchRendererManager.CurrentRendererContext = context;
            SketchRendererManager.UpdateRendererToCurrentContext();
            SketchRendererManager.CurrentRendererContext.IsDirty = false;
            if(visible)
                ForceRepaint();
            
            Selection.activeObject = context;
            EditorGUIUtility.PingObject(context);
            EditorUtility.FocusProjectWindow();
        }

        private void RendererContext_OnValidate()
        {
            if(visible && !SketchRendererManager.ManagerSettings.AlwaysUpdateRendererData)
                ForceRepaint();
            else
                UpdateOnSettingsChange();
        }

        private VisualElement GetContextDirtyHelpBox()
        {
            HelpBox dirtyHelpBox = new HelpBox();
            dirtyHelpBox.text = $"The current renderer data has settings that differ from the active sketch context.\nConsider reapplying the context to fix this.";
            dirtyHelpBox.style.justifyContent = Justify.Center;
            dirtyHelpBox.style.unityTextAlign = TextAnchor.MiddleCenter;
            return dirtyHelpBox;
        }
        
        private void ForceRepaint()
        {
            root.Clear();
            ConstructGUI(root);
            Repaint();
        }

        private void ReapplyContext_Clicked()
        {
            if (contextField.Field.value != null)
            {
                UpdateOnSettingsChange();
                ForceRepaint();
            }
        }

        private void UpdateOnSettingsChange()
        {
            SketchRendererContext context = (SketchRendererContext)contextField.Field.value;
            SketchRendererManager.UpdateRendererToCurrentContext();
            context.IsDirty = false;
        }

        private void CreateAndApplyActive_Clicked()
        {
            SketchRendererContext context = SketchRendererContextWizard.CreateSketchRendererContext();
            UpdateActiveRendererContext(context);
        }

        private void ValidateSettings()
        {
            SketchRendererManager.ManagerSettings.ApplyGlobalSettings();
        }
    }
}