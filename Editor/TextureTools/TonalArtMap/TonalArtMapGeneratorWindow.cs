using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Extensions;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Image = UnityEngine.UIElements.Image;
using Slider = UnityEngine.UIElements.Slider;

namespace SketchRenderer.Editor.TextureTools
{
    [EditorWindowTitle(title = "Tonal Art Map Generator")]
    internal class TonalArtMapGeneratorWindow : SketchEditorToolWindow<TonalArtMapGeneratorWindow>
    {
        internal VisualElement root;
        
        internal VisualElement settingsPanel;
        internal VisualElement previewPanel;
        
        // Filed binding elements
        internal Image previewImage;
        internal ObjectField strokeAssetField;
        internal UnityEditor.Editor strokeAssetEditor;
        
        internal override void InitializeTool(SketchResourceAsset resources)
        {
            TonalArtMapGenerator.OnTextureUpdated += UpdatePreviewTargetTexture;
            TextureGenerator.OnRecreateTargetTexture += UpdatePreviewTargetTexture;
            
            TonalArtMapGenerator.OnTextureUpdated += FinalizeProgressBar;
            
            TonalArtMapGenerator.Init(resources);
            ApplyToMatchGeneratorSettings();
        }

        internal override void FinalizeTool()
        {
            TonalArtMapGenerator.OnTextureUpdated -= UpdatePreviewTargetTexture;
            TextureGenerator.OnRecreateTargetTexture -= UpdatePreviewTargetTexture;
            
            TonalArtMapGenerator.OnTextureUpdated -= FinalizeProgressBar;
            
            TonalArtMapGenerator.Finish();
        }

        internal void OnValidate()
        {
            if (!TonalArtMapGenerator.Generating)
            {
                ForceRepaint();
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
            
            settingsPanel = new ScrollView();
            settingsPanel.style.flexGrow = 0;
            settingsPanel.style.justifyContent = Justify.FlexStart;
            root.Add(settingsPanel);
            
            var settingsLabel = new Label("Tonal Art Map Generator");
            settingsPanel.Add(settingsLabel);
            
            var settingsStrokePanel = CreateStrokeAssetRegion();
            settingsPanel.Add(settingsStrokePanel);

            var settingsOutputPanel = CreateOutputSettingsRegion();
            settingsPanel.Add(settingsOutputPanel);
            
            var exportRegion = CreateExportRegion();
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

            if (strokeAssetField == null)
            {
                strokeAssetField = new ObjectField("Stroke Asset");
                strokeAssetField.objectType = typeof(StrokeAsset);
                strokeAssetField.RegisterValueChangedCallback(StrokeAsset_Changed);
                strokeAssetField.SetValueWithoutNotify(TonalArtMapGenerator.StrokeDataAsset);
            }
            else
            {
                strokeAssetField.MarkDirtyRepaint();
            }
            strokeDataRegion.Add(strokeAssetField);

            if (strokeAssetField.value != null)
            {
                if (strokeAssetEditor == null)
                    strokeAssetEditor = UnityEditor.Editor.CreateEditor(strokeAssetField.value);
                var strokeEditorElement = strokeAssetEditor.CreateInspectorGUI();
                strokeEditorElement.RegisterCallback<SerializedPropertyChangeEvent>(StrokeData_Changed);
                strokeDataRegion.Add(strokeEditorElement);
            }

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
            if (previewImage != null)
            {
                previewImage.image = TextureGenerator.TargetRT;
            }
        }

        internal void ForceRepaint()
        {
            TonalArtMapGenerator.DisplaySDF();
            Repaint();
        }

        internal void ForceRebuildGUI()
        {
            if (strokeAssetEditor != null)
            {
                DestroyImmediate(strokeAssetEditor);
                strokeAssetEditor = null;
            }

            root.Clear();
            CreateGUI();
            ForceRepaint();
        }
        
        #region Action Handlers

        internal void ApplyToMatchGeneratorSettings()
        {
            //Container must be initialized for children to exist
            if (settingsPanel != null)
            {
                //--Stroke Panel
                strokeAssetField.SetValueWithoutNotify(TonalArtMapGenerator.StrokeDataAsset);
            }

            ForceRepaint();
        }

        internal void AnySettings_Clicked(ClickEvent e)
        {
            ForceRepaint();
        }
        
        // -- Stroke Panel
        internal void StrokeAsset_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            StrokeAsset strokeAsset = (StrokeAsset)bind.newValue;
            TonalArtMapGenerator.StrokeDataAsset = strokeAsset;
            ForceRebuildGUI();
        }

        internal void StrokeData_Changed(SerializedPropertyChangeEvent bind) => ForceRepaint();
        
        // -- Export Panel
        internal void GenerateTonalArtMap_Clicked()
        {
            TonalArtMapGenerator.GenerateTAMToneTextures();
        }
        
        #endregion
    }
}