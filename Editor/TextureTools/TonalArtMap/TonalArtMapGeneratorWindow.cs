using System;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools
{
    [EditorWindowTitle(title = "Tonal Art Map Generator")]
    internal class TonalArtMapGeneratorWindow : SketchEditorToolWindow<TonalArtMapGeneratorWindow>
    {
        internal VisualElement root;
        internal VisualElement settingsPanel;
        internal VisualElement previewPanel;
        internal Image previewImage;
        
        internal RenderTexture previewTexture;
        internal StrokeAsset strokeAsset;

        internal ObjectField strokeAssetField;
        
        internal override void InitializeTool(SketchResourceAsset resources)
        {
            TonalArtMapGenerator.OnTextureUpdated += UpdatePreviewTargetTexture;
            TonalArtMapGenerator.OnTextureUpdated += FinalizeProgressBar;
            TonalArtMapGenerator.Init(resources);
        }

        internal override void FinalizeTool()
        {
            TonalArtMapGenerator.OnTextureUpdated -= UpdatePreviewTargetTexture;
            TonalArtMapGenerator.OnTextureUpdated -= FinalizeProgressBar;
            TonalArtMapGenerator.Finish();
        }

        internal void OnValidate()
        {
            if (!TonalArtMapGenerator.Generating)
            {
                TonalArtMapGenerator.DisplaySDF();
            }
        }

        internal void Update()
        {
            if (TonalArtMapGenerator.Generating)
            {
                EditorUtility.DisplayProgressBar("Generating Tam Tones", "Test", 0.5f);
            }
        }

        internal void FinalizeProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }

        #region Create Window Interface

        internal void CreateGUI()
        {
            root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            
            //---- Configure Settings Scroll Panel
            
            ScrollView settingsPanel = new ScrollView();
            settingsPanel.style.flexGrow = 0;
            settingsPanel.style.justifyContent = Justify.FlexStart;
            root.Add(settingsPanel);
            
            var settingsLabel = new Label("Tonal Art Map Generator");
            settingsPanel.Add(settingsLabel);
            
            VisualElement strokeRegion = CreateStrokeAssetRegion();
            settingsPanel.Add(strokeRegion);

            VisualElement outputRegion = CreateOutputSettingsRegion();
            settingsPanel.Add(outputRegion);
            
            VisualElement exportRegion = CreateExportRegion();
            settingsPanel.Add(exportRegion);
            
            settingsPanel.RegisterCallback<ClickEvent>(AnySettings_Clicked);
            
            //---- Configure Preview Panel
            
            previewPanel = new VisualElement();
            previewPanel.style.minHeight = ExpectedMinWindowSize.x;
            previewPanel.style.maxHeight = ExpectedMaxWindowSize.x;
            
            var previewLabel = new Label("Preview Texture");
            previewPanel.Add(previewLabel);
            
            previewImage = new Image();
            previewImage.scaleMode = ScaleMode.ScaleToFit;

            UpdatePreviewTargetTexture();
            previewPanel.Add(previewImage);
            root.Add(previewPanel);
            root.RegisterCallback<GeometryChangedEvent>(
                geometryEvent =>
                {
                    EnsureMaxPreviewSize();
                }
            );
        }

        internal VisualElement CreateStrokeAssetRegion()
        {
            var strokeDataRegion = new Foldout();
            strokeDataRegion.text = "Stroke Settings";

            strokeAssetField = new ObjectField("Stroke Asset");
            strokeAssetField.objectType = typeof(StrokeAsset);
            strokeDataRegion.Add(strokeAssetField);
            return strokeDataRegion;
        }
        
        internal VisualElement CreateOutputSettingsRegion()
        {
            var outputSettingsRegion = new Foldout();
            outputSettingsRegion.text = "Output Settings";

            var resolutionField = new EnumField("Texture Size", TextureResolution.SIZE_512);
            outputSettingsRegion.Add(resolutionField);
            return outputSettingsRegion;
        }
        
        internal VisualElement CreateExportRegion()
        {
            var exportRegion = new VisualElement();
            var exportLabel = new Label("Export");
            exportRegion.Add(exportLabel);

            var generateTamButton = new Button(GenerateTonalArtMap_Clicked);
            generateTamButton.text = "Generate Tonal Art Map";
            exportRegion.Add(generateTamButton);
            return exportRegion;
        }
        
        #endregion

        internal void EnsureMaxPreviewSize()
        {
            float windowWidth = root.resolvedStyle.width;
            previewPanel.style.height = windowWidth;
        }

        internal void UpdatePreviewTargetTexture()
        {
            if(previewImage != null)
                previewImage.image = TextureGenerator.TargetRT;
        }
        
        #region Action Handlers

        internal void AnySettings_Clicked(ClickEvent e)
        {
            UpdatePreviewToMatchSettings();
        }

        internal void UpdatePreviewToMatchSettings()
        {
            TonalArtMapGenerator.DisplaySDF();
        }

        internal void GenerateTonalArtMap_Clicked()
        {
            TonalArtMapGenerator.GenerateTAMToneTextures();
        }
        
        #endregion
    }
}