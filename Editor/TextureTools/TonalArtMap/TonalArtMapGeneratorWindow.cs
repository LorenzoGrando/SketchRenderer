using System;
using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Extensions;
using SketchRenderer.Runtime.TextureTools.Strokes;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Image = UnityEngine.UIElements.Image;

namespace SketchRenderer.Editor.TextureTools
{
    [EditorWindowTitle(title = "Tonal Art Map Generator")]
    internal class TonalArtMapGeneratorWindow : SketchEditorToolWindow<TonalArtMapGeneratorWindow>
    {
        internal VisualElement root;
        
        internal VisualElement settingsPanel;
        internal VisualElement previewPanel;
        
        // Field binding elements
        internal Image previewImage;
        internal ObjectField strokeAssetField;
        internal UnityEditor.Editor strokeAssetEditor;
        
        internal ObjectField tonalArtMapAssetField;
        internal UnityEditor.Editor tonalArtMapAssetEditor;
        internal TextField pathField;
        
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
            if (!TonalArtMapGenerator.Generating || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
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
            
            if (tonalArtMapAssetField == null)
            {
                tonalArtMapAssetField = new ObjectField("Tonal Art Map Asset");
                tonalArtMapAssetField.objectType = typeof(TonalArtMapAsset);
                tonalArtMapAssetField.RegisterValueChangedCallback(TonalArtMap_Changed);
                tonalArtMapAssetField.SetValueWithoutNotify(TonalArtMapGenerator.TonalArtMapAsset);
            }
            else
            {
                tonalArtMapAssetField.MarkDirtyRepaint();
            }
            outputSettingsRegion.Add(tonalArtMapAssetField);
            
            if (tonalArtMapAssetField.value != null && !TonalArtMapWizard.IsCurrentTonalArtMap((TonalArtMapAsset)tonalArtMapAssetField.value))
            {
                var setActiveButton = new Button(SetActiveTonalArtMap_Clicked) { text = "Assign to Renderer Context" };
                outputSettingsRegion.Add(setActiveButton);
            }

            if (tonalArtMapAssetField.value != null)
            {
                var tonalArtMapRegion = new VisualElement();
                var tonalArtMapLabel = new Label("Tonal Art Map Settings");
                tonalArtMapRegion.Add(tonalArtMapLabel);
                
                if (tonalArtMapAssetEditor == null)
                    tonalArtMapAssetEditor = UnityEditor.Editor.CreateEditor(tonalArtMapAssetField.value);
                var tonesEditorElement = tonalArtMapAssetEditor.CreateInspectorGUI();
                tonalArtMapRegion.Add(tonesEditorElement);
                
                var resolutionField = new EnumField("Texture Size", TextureResolution.SIZE_512);
                resolutionField.RegisterValueChangedCallback(Resolution_Changed);
                resolutionField.SetValueWithoutNotify(TonalArtMapGenerator.TargetResolution);
                tonalArtMapRegion.Add(resolutionField);
            
                var iterationsField = new IntegerField("Iterations Per Stroke Applied");
                iterationsField.isDelayed = true;
                iterationsField.AddManipulator(new ClampedIntegerManipulator(iterationsField, 1, 100));
                iterationsField.RegisterValueChangedCallback(IterationsPerStroke_Changed);
                iterationsField.SetValueWithoutNotify(TonalArtMapGenerator.IterationsPerStroke);
                tonalArtMapRegion.Add(iterationsField);
                
                outputSettingsRegion.Add(tonalArtMapRegion);
            }
            else
            {
                var createCustomTonalArtMap = new Button(CreateTonalArtMap_Clicked) { text = "Create And Assign to Renderer Context"};
                outputSettingsRegion.Add(createCustomTonalArtMap);
            }
            
            return outputSettingsRegion;
        }
        
        internal VisualElement CreateExportRegion()
        {
            var exportRegion = new VisualElement();
            var exportLabel = new Label("Export Settings");
            exportRegion.Add(exportLabel);
            
            pathField = new TextField("Override Directory Path");
            pathField.isDelayed = true;
            var pathManipulator = new AssetsDirectoryManipulator(pathField);
            pathField.AddManipulator(pathManipulator);
            pathManipulator.OnValidated += Path_Changed;
            exportRegion.Add(pathField);
            
            var pathButton = new Button(ChoosePath_Clicked) { text = "Set Override Directory Path"};
            exportRegion.Add(pathButton);

            var generateTamButton = new Button(GenerateTonalArtMap_Clicked) { text = "Generate Tonal Art Map" };
            exportRegion.Add(generateTamButton);
            return exportRegion;
        }

        internal VisualElement CreatePreviewRegion()
        {
            previewPanel = new VisualElement();
            previewPanel.style.minHeight = ExpectedMinWindowSize.x;
            previewPanel.style.maxHeight = ExpectedMaxWindowSize.x;
            
            var previewLabel = new Label("Preview Texture");
            previewPanel.Add(previewLabel);
            
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

            if (tonalArtMapAssetEditor != null)
            {
                DestroyImmediate(tonalArtMapAssetEditor);
                tonalArtMapAssetEditor = null;
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
                strokeAssetField.SetValueWithoutNotify(TonalArtMapGenerator.StrokeDataAsset);
            }

            ForceRebuildGUI();
        }
        
        // -- Stroke Panel
        internal void StrokeAsset_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            StrokeAsset strokeAsset = (StrokeAsset)bind.newValue;
            TonalArtMapGenerator.StrokeDataAsset = strokeAsset;
            ForceRebuildGUI();
        }

        internal void StrokeData_Changed(SerializedPropertyChangeEvent bind) => ForceRepaint();
        
        // -- Output Panel

        internal void TonalArtMap_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            TonalArtMapAsset tonalArtMapAsset = (TonalArtMapAsset)bind.newValue;
            TonalArtMapGenerator.TonalArtMapAsset = tonalArtMapAsset;
            ForceRebuildGUI();
        }

        internal void CreateTonalArtMap_Clicked()
        {
            TonalArtMapAsset asset = TonalArtMapWizard.CreateTonalArtMapAndSetActive();
            tonalArtMapAssetField.value = asset;
        }

        internal void SetActiveTonalArtMap_Clicked()
        {
            TonalArtMapWizard.SetAsCurrentTonalArtMap((TonalArtMapAsset)tonalArtMapAssetField.value);
            ForceRebuildGUI();
        }
        
        internal void Resolution_Changed(ChangeEvent<Enum> bind)
        {
            TonalArtMapGenerator.TargetResolution = (TextureResolution)bind.newValue;
            ForceRepaint();
        }
        
        internal void IterationsPerStroke_Changed(ChangeEvent<int> bind)
        {
            TonalArtMapGenerator.IterationsPerStroke = bind.newValue;
        }
        
        // -- Export Panel

        internal void Path_Changed(string bind)
        {
            TonalArtMapGenerator.OverrideOutputPath = bind;
        }

        internal void ChoosePath_Clicked()
        {
            string path = EditorUtility.OpenFolderPanel("Choose Path", "Assets/", "");
            pathField.value = path;
        }
        
        internal void GenerateTonalArtMap_Clicked()
        {
            TonalArtMapGenerator.GenerateTAMToneTextures();
        }
        
        #endregion
    }
}