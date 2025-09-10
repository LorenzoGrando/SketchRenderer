using System;
using SketchRenderer.Editor.UIToolkit;
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
            //TonalArtMapGenerator.OnTextureUpdated += UpdatePreviewTargetTexture;
            TextureGenerator.OnRecreateTargetTexture += UpdatePreviewTargetTexture;
            
            //TonalArtMapGenerator.OnTextureUpdated += FinalizeProgressBar;
            
            MaterialGenerator.Init(resources);
            ApplyToMatchGeneratorSettings();
        }

        internal override void FinalizeTool()
        {
            //TonalArtMapGenerator.OnTextureUpdated -= UpdatePreviewTargetTexture;
            TextureGenerator.OnRecreateTargetTexture -= UpdatePreviewTargetTexture;
            
            //TonalArtMapGenerator.OnTextureUpdated -= FinalizeProgressBar;
            
            //TonalArtMapGenerator.Finish();
        }

        internal void OnValidate()
        {
            if (/*!TonalArtMapGenerator.Generating ||*/ EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if(window.hasFocus)
                    ForceRepaint();
            }
        }

        private void UpdateToolStatus(TextureToolGenerationStatusArgs args) => toolStatusDisplay = args;

        internal void Update()
        {
            //if (TonalArtMapGenerator.Generating)
            //{
                //EditorUtility.DisplayProgressBar(toolStatusDisplay.toolProcess, toolStatusDisplay.toolSubprocess, toolStatusDisplay.toolTotalProgress);
            //}
        }

        internal void FinalizeProgressBar()
        {
            //TonalArtMapGenerator.OnTonalArtMapGenerationStep -= UpdateToolStatus;
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
            SketchRendererUIUtils.AddWithMargins(materialDataRegion, materialAssetField.Container, SketchRendererUIData.BaseFieldMargins);

            if (materialAssetField.Field.value != null)
            {
                if (materialAssetEditor == null)
                    materialAssetEditor = UnityEditor.Editor.CreateEditor(materialAssetField.Field.value);
                var materialEditorElement = materialAssetEditor.CreateInspectorGUI();
                materialEditorElement.RegisterCallback<SerializedPropertyChangeEvent>(MaterialData_Changed);
                SketchRendererUIUtils.AddWithMargins(materialDataRegion, materialEditorElement, SketchRendererUIData.BaseFieldMargins);
            }
            
            TextureResolution initialResolution = TextureGenerator.Resolution;
            var resolutionField = SketchRendererUI.SketchEnumField("Texture Size", initialResolution, changeCallback:Resolution_Changed);

            SketchRendererUIUtils.AddWithMargins(materialDataRegion, resolutionField.Container, SketchRendererUIData.MinorFieldMargins);
            
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

            var generateTamButton = SketchRendererUI.SketchMajorButton("Generate Material Textures", GenerateTextures_Clicked);
            SketchRendererUIUtils.AddWithMargins(settingsContainer, generateTamButton, SketchRendererUIData.MinorFieldMargins);
            
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
                previewImage.image = TextureGenerator.TargetRT;
            }
        }

        internal void ForceRepaint()
        {
            MaterialGenerator.UpdateMaterialAlbedoTexture();
            Repaint();
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
        
        // -- Stroke Panel
        internal void MaterialAsset_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            MaterialDataAsset materialAsset = (MaterialDataAsset)bind.newValue;
            MaterialGenerator.MaterialDataAsset = materialAsset;
            ForceRebuildGUI();
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
            //TonalArtMapGenerator.OnTonalArtMapGenerationStep += UpdateToolStatus;
            MaterialGenerator.GenerateMaterialTextures();
        }
        
        #endregion
    }
}