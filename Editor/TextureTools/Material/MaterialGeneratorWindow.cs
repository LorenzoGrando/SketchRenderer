using System;
using System.Collections;
using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Editor.Utils;
using SketchRenderer.Runtime.Data;
using TextureTools.Material;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools
{
    internal class MaterialGeneratorWindow : SketchEditorToolWindow<MaterialGeneratorWindow>
    {
        internal VisualElement root;
        
        internal ScrollView settingsPanel;
        internal VisualElement previewPanel;
        
        // Field binding elements
        internal Image previewImage;
        internal SketchElement<ObjectField> materialAssetField;
        internal UnityEditor.Editor materialAssetEditor;
        
        internal SketchElement<TextField> pathField;
        internal TextureToolGenerationStatusArgs toolStatusDisplay;
        
        internal override void InitializeTool(SketchResourceAsset resources)
        {
            MaterialGenerator.OnTextureUpdated += UpdatePreviewTargetTexture;
            TextureGenerator.OnRecreateTargetTexture += UpdatePreviewTargetTexture;
            
            MaterialGenerator.OnTextureUpdated += FinalizeProgressBar;
            
            MaterialGenerator.Init(resources);
            ApplyToMatchGeneratorSettings();
        }

        internal override void FinalizeTool()
        {
            MaterialGenerator.OnTextureUpdated -= UpdatePreviewTargetTexture;
            TextureGenerator.OnRecreateTargetTexture -= UpdatePreviewTargetTexture;
            
            MaterialGenerator.OnTextureUpdated -= FinalizeProgressBar;
        }

        internal void OnValidate()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if(window != null && window.hasFocus)
                    ForceRepaint();
            }
        }

        private void UpdateToolStatus(TextureToolGenerationStatusArgs args) => toolStatusDisplay = args;

        internal void Update()
        {
            if (MaterialGenerator.Generating)
            {
                EditorUtility.DisplayProgressBar(toolStatusDisplay.toolProcess, toolStatusDisplay.toolSubprocess, toolStatusDisplay.toolTotalProgress);
            }
        }

        internal void FinalizeProgressBar()
        {
            MaterialGenerator.OnMaterialGenerationStep -= UpdateToolStatus;
            EditorUtility.ClearProgressBar();
        }
        
        #region Create Window Interface

        internal void CreateGUI()
        {
            root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            
            //---- Configure Settings Scroll Panel
            
            settingsPanel = new ScrollView();
            settingsPanel.style.flexGrow = 0;
            settingsPanel.style.justifyContent = Justify.FlexStart;
            settingsPanel.style.alignItems = Align.FlexStart;
            root.Add(settingsPanel);
            
            var settingsMaterialPanel = CreateMaterialAssetRegion();
            settingsPanel.Add(settingsMaterialPanel);
            
            var exportRegion = CreateExportRegion();
            settingsPanel.Add(exportRegion);
            
            //---- Configure Preview Panel
            previewPanel = CreatePreviewRegion();
            root.Add(previewPanel);
            root.RegisterCallback<GeometryChangedEvent>(
                geometryEvent =>
                {
                    EnsureMaxPreviewSize();
                }
            );
        }
        
        internal VisualElement CreateMaterialAssetRegion()
        {
            var materialDataRegion = SketchRendererUI.SketchFoldout("Material Settings", applyMargins:false);

            if (materialAssetField == null)
                materialAssetField = SketchRendererUI.SketchObjectField("Material Data Asset", typeof(MaterialDataAsset), MaterialGenerator.MaterialDataAsset, changeCallback:MaterialAsset_Changed);
            else
                materialAssetField.Container.MarkDirtyRepaint();
            
            MaterialDataAsset materialAsset = materialAssetField.Field.value as MaterialDataAsset;
            
            CornerData assetMargins = (materialAsset != null ? SketchRendererUIData.BaseFieldMargins : SketchRendererUIData.BaseFieldNoVerticalMargins);
            SketchRendererUIUtils.AddWithMargins(materialDataRegion, materialAssetField.Container, assetMargins);

            Button createCustomMaterialAsset = null;
            if (materialAsset != null)
            {
                if (materialAssetEditor == null)
                    materialAssetEditor = UnityEditor.Editor.CreateEditor(materialAssetField.Field.value);
                var materialEditorElement = materialAssetEditor.CreateInspectorGUI();
                materialEditorElement.RegisterCallback<SerializedPropertyChangeEvent>(MaterialData_Changed);
                SketchRendererUIUtils.AddWithMargins(materialDataRegion, materialEditorElement, SketchRendererUIData.BaseFieldMargins);
            }
            else
            {
                createCustomMaterialAsset = new Button(CreateMaterialData_Clicked) { text = "Create New And Assign"};
                SketchRendererUIUtils.AddWithMargins(materialDataRegion, createCustomMaterialAsset, SketchRendererUIData.BaseFieldMargins);
            }
            
            TextureResolution initialResolution = TextureGenerator.Resolution;
            var resolutionField = SketchRendererUI.SketchEnumField("Texture Size", initialResolution, changeCallback:Resolution_Changed);

            SketchRendererUIUtils.AddWithMargins(materialDataRegion, resolutionField.Container, SketchRendererUIData.MinorFieldMargins);
            
            materialDataRegion.ToggleElementInteractions(MaterialGenerator.HasNonDefaultMaterialDataAsset);
            materialAssetField.Container.SetEnabled(true);
            if(createCustomMaterialAsset != null)
                createCustomMaterialAsset.SetEnabled(true);
            if(!MaterialGenerator.HasNonDefaultMaterialDataAsset && materialAsset != null)
                SketchRendererUIUtils.AddWithMargins(materialDataRegion, SketchRendererUI.SketchInmutableAssetHelpBox(), SketchRendererUIData.BaseFieldMargins);
            
            return materialDataRegion;
        }
        
        internal VisualElement CreateExportRegion()
        {
            var exportRegion = SketchRendererUI.SketchFoldout("Export Settings", applyMargins:false);
            var settingsContainer = new VisualElement();
            
            var pathManipulator = new AssetsDirectoryManipulator();
            pathField = SketchRendererUI.SketchTextField("Override Directory Path", isDelayed:true, pathManipulator);
            pathManipulator.OnValidated += Path_Changed;
            SketchRendererUIUtils.AddWithMargins(settingsContainer, pathField.Container, SketchRendererUIData.MinorFieldMargins);
            
            var pathButton = new Button(ChoosePath_Clicked) { text = "Choose Override Directory Path"};
            SketchRendererUIUtils.AddWithMargins(settingsContainer, pathButton, SketchRendererUIData.MinorFieldMargins);

            var generateTexButton = SketchRendererUI.SketchMajorButton("Generate Material Textures", GenerateTextures_Clicked);
            SketchRendererUIUtils.AddWithMargins(settingsContainer, generateTexButton, SketchRendererUIData.MinorFieldMargins);
                
            var generateAndAssignTexButton = SketchRendererUI.SketchMajorButton("Generate and Assign to Renderer Context", GenerateAndAssignTextures_Clicked);
            SketchRendererUIUtils.AddWithMargins(settingsContainer, generateAndAssignTexButton, SketchRendererUIData.MinorFieldMargins);
            
            SketchRendererUIUtils.AddWithMargins(exportRegion, settingsContainer, SketchRendererUIData.BaseFieldMargins);
            return exportRegion;
        }
        
        internal VisualElement CreatePreviewRegion()
        {
            previewPanel = SketchRendererUI.SketchMajorArea("Preview Texture");
            previewPanel.style.minHeight = ExpectedMinWindowSize.x;
            previewPanel.style.maxHeight = ExpectedMaxWindowSize.x;

            previewImage = new Image();
            previewImage.scaleMode = ScaleMode.ScaleToFit;

            UpdatePreviewTargetTexture();
            previewPanel.Add(previewImage);
            return previewPanel;
        }
        
        #endregion
        
        internal void EnsureMaxPreviewSize()
        {
            float windowWidth = root.resolvedStyle.width;
            previewPanel.style.height = windowWidth;
        }

        internal void UpdatePreviewTargetTexture()
        {
            if (previewImage != null)
            {
                previewImage.visible = (materialAssetField != null && materialAssetField.Field.value != null) && (IsActiveWindow || hasDirtyRepaint);
                previewImage.image = TextureGenerator.TargetRT;
            }
        }

        internal void ForceRepaint()
        {
            hasDirtyRepaint = true;
            MaterialGenerator.UpdateMaterialAlbedoTexture();
            Repaint();
            hasDirtyRepaint = false;
        }

        internal void ForceRebuildGUI()
        {
            if (materialAssetEditor != null)
            {
                DestroyImmediate(materialAssetEditor);
                materialAssetEditor = null;
            }

            if (settingsPanel != null)
            {
                root.Clear();
                CreateGUI();
            }

            ForceRepaint();
        }
        
        #region Action Handlers

        internal void ApplyToMatchGeneratorSettings()
        {
            //Container must be initialized for children to exist
            if (settingsPanel != null)
            {
                //--Stroke Panel
                materialAssetField.Field.SetValueWithoutNotify(MaterialGenerator.MaterialDataAsset);
            }

            ForceRebuildGUI();
        }
        
        // -- Material Panel
        internal void MaterialAsset_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            MaterialDataAsset materialAsset = (MaterialDataAsset)bind.newValue;
            MaterialGenerator.MaterialDataAsset = materialAsset;
            ForceRebuildGUI();
        }

        internal void CreateMaterialData_Clicked()
        {
            MaterialDataAsset asset = MaterialGeneratorWizard.CreateMaterialDataAsset();
            materialAssetField.Field.value = asset;
        }

        internal void MaterialData_Changed(SerializedPropertyChangeEvent bind) => ForceRepaint();
        
        internal void Resolution_Changed(ChangeEvent<Enum> bind)
        {
            MaterialGenerator.TargetResolution = (TextureResolution)bind.newValue;
            ForceRepaint();
        }
        
        internal void Path_Changed(string bind)
        {
            MaterialGenerator.OverrideOutputPath = bind;
        }

        internal void ChoosePath_Clicked()
        {
            string path = EditorUtility.OpenFolderPanel("Choose Path", "Assets/", "");
            pathField.Field.value = path;
        }
        
        internal void GenerateTextures_Clicked()
        {
            if(MaterialGenerator.Generating)
                return;
            MaterialGenerator.OnMaterialGenerationStep += UpdateToolStatus;
            MaterialGenerator.GenerateMaterialTextures();
        }

        internal void GenerateAndAssignTextures_Clicked()
        {
            if(MaterialGenerator.Generating)
                return;
            
            GenerateTextures_Clicked();
            StaticCoroutine.StartCoroutine(AwaitTextureGenerationCoroutine(AssignToCurrentContext));
        }

        private void AssignToCurrentContext()
        {
            MaterialGeneratorWizard.SetAsActiveAlbedo(MaterialGenerator.LastGeneratedAlbedoTexture);
            MaterialGeneratorWizard.SetAsActiveDirectional(MaterialGenerator.LastGeneratedDirectionalTexture);
        }

        private IEnumerator AwaitTextureGenerationCoroutine(Action callback)
        {
            while (MaterialGenerator.Generating)
                yield return null;

            callback?.Invoke();
        }
        
        #endregion
    }
}