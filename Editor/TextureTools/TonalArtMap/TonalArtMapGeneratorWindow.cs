using System;
using SketchRenderer.Editor.Rendering;
using SketchRenderer.Editor.TextureTools.Strokes;
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
        
        internal ScrollView settingsPanel;
        internal VisualElement previewPanel;
        
        // Field binding elements
        internal Image previewImage;
        internal SketchElement<ObjectField> strokeAssetField;
        internal UnityEditor.Editor strokeAssetEditor;
        internal EnumField strokeCreationField;
        
        internal SketchElement<ObjectField> tonalArtMapAssetField;
        internal UnityEditor.Editor tonalArtMapAssetEditor;
        internal HelpBox sanityHelpBox;
        
        internal SketchElement<TextField> pathField;
        internal TextureToolGenerationStatusArgs toolStatusDisplay;

        private SketchRendererManagerSettings settings;
        
        internal override void InitializeTool(SketchResourceAsset resources, SketchRendererManagerSettings settings)
        {
            this.settings = settings;
            
            TonalArtMapGenerator.OnTextureUpdated += UpdatePreviewTargetTexture;
            TextureGenerator.OnRecreateTargetTexture += UpdatePreviewTargetTexture;
            
            TonalArtMapGenerator.OnTextureUpdated += FinalizeProgressBar;

            StrokeAsset strokeAsset = settings.PersistentStrokeAsset != null ? settings.PersistentStrokeAsset : resources.Scriptables.Strokes.DefaultSimpleStroke;
            TonalArtMapAsset tamAsset = settings.PersistentTonalArtMapAsset != null ? settings.PersistentTonalArtMapAsset : resources.Scriptables.TonalArtMap;
            TonalArtMapGenerator.Init(resources, strokeAsset, tamAsset);
            ApplyToMatchGeneratorSettings();
            ForceRebuildGUI();
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
                if(window != null && window.hasFocus)
                    ForceRepaint();
            }
        }

        private void UpdateToolStatus(TextureToolGenerationStatusArgs args) => toolStatusDisplay = args;

        internal void Update()
        {
            if (TonalArtMapGenerator.Generating)
            {
                EditorUtility.DisplayProgressBar(toolStatusDisplay.toolProcess, toolStatusDisplay.toolSubprocess, toolStatusDisplay.toolTotalProgress);
            }
        }

        internal void FinalizeProgressBar(RenderTexture _)
        {
            TonalArtMapGenerator.OnTonalArtMapGenerationStep -= UpdateToolStatus;
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
            var strokeDataRegion = SketchRendererUI.SketchFoldout("Stroke Settings", applyMargins:false);

            if (strokeAssetField == null)
                strokeAssetField = SketchRendererUI.SketchObjectField("Stroke Asset", typeof(StrokeAsset), TonalArtMapGenerator.StrokeDataAsset, changeCallback:StrokeAsset_Changed);
            else
                strokeAssetField.Container.MarkDirtyRepaint();
            SketchRendererUIUtils.AddWithMargins(strokeDataRegion, strokeAssetField.Container, SketchRendererUIData.BaseFieldMargins);

            if (strokeAssetField.Field.value != null)
            {
                if (strokeAssetEditor == null)
                    strokeAssetEditor = UnityEditor.Editor.CreateEditor(strokeAssetField.Field.value);
                var strokeEditorElement = strokeAssetEditor.CreateInspectorGUI();
                strokeEditorElement.RegisterCallback<SerializedPropertyChangeEvent>(StrokeData_Changed);
                SketchRendererUIUtils.AddWithMargins(strokeDataRegion, strokeEditorElement, SketchRendererUIData.BaseFieldMargins);
                
                //Only allow editing if data asset is mutable or field is always necessary
                strokeDataRegion.ToggleElementInteractions(TonalArtMapGenerator.HasNonDefaultStrokeDataAsset);
                strokeAssetField.Container.SetEnabled(true);
                if(!TonalArtMapGenerator.HasNonDefaultStrokeDataAsset)
                    SketchRendererUIUtils.AddWithMargins(strokeDataRegion, SketchRendererUI.SketchInmutableAssetHelpBox(), SketchRendererUIData.BaseFieldMargins);
            }
            else
            {
                var strokeCreation = SketchRendererUI.SketchEnumField("Stroke To Create", StrokeSDFType.SIMPLE);
                strokeCreationField = strokeCreation.Field;
                SketchRendererUIUtils.AddWithMargins(strokeDataRegion, strokeCreation.Container, SketchRendererUIData.BaseFieldNoVerticalMargins);
                var createCustomStrokeAsset = new Button(CreateStrokeAsset_Clicked) { text = "Create from Type and Assign"};
                SketchRendererUIUtils.AddWithMargins(strokeDataRegion, createCustomStrokeAsset, SketchRendererUIData.BaseFieldMargins);
            }

            return strokeDataRegion;
        }
        
        internal VisualElement CreateOutputSettingsRegion()
        {
            var outputSettingsRegion = SketchRendererUI.SketchFoldout("Output Settings", applyMargins:false);
            outputSettingsRegion.style.justifyContent = Justify.FlexStart;
            
            if (tonalArtMapAssetField == null)
                tonalArtMapAssetField = SketchRendererUI.SketchObjectField("Tonal Art Map Asset", typeof(TonalArtMapAsset), TonalArtMapGenerator.TonalArtMapAsset, changeCallback:TonalArtMap_Changed);
            else
                tonalArtMapAssetField.Container.MarkDirtyRepaint();

            TonalArtMapAsset fieldAsset = (TonalArtMapAsset)tonalArtMapAssetField.Field.value;

            CornerData outputMargins = (fieldAsset != null && TonalArtMapWizard.IsCurrentTonalArtMap(fieldAsset)) ? SketchRendererUIData.BaseFieldMargins : SketchRendererUIData.BaseFieldNoVerticalMargins;
            SketchRendererUIUtils.AddWithMargins(outputSettingsRegion, tonalArtMapAssetField.Container, outputMargins);
            
            if (fieldAsset != null)
            {
                fieldAsset.OnTonesPacked += ForceRebuildGUI;
                Button setActiveButton = null;
                if (!TonalArtMapWizard.IsCurrentTonalArtMap(fieldAsset))
                {
                    setActiveButton = new Button(SetActiveTonalArtMap_Clicked) { text = "Assign to Renderer Context" };
                    SketchRendererUIUtils.AddWithMargins(outputSettingsRegion, setActiveButton, SketchRendererUIData.BaseFieldMargins);
                }

                if (fieldAsset.HasDirtyProperties())
                {
                    if (sanityHelpBox == null)
                    {
                        sanityHelpBox = new HelpBox();
                        sanityHelpBox.text = $"The selected TonalArtMap has baked settings different from the current displayed.\nPlease regenerate the TAM to apply the properties.";
                        sanityHelpBox.style.justifyContent = Justify.Center;
                        sanityHelpBox.style.unityTextAlign = TextAnchor.MiddleCenter;
                        TonalArtMapGenerator.OnTonalArtMapGenerated += CheckSanityBoxValid;
                    }
                    SketchRendererUIUtils.AddWithMargins(outputSettingsRegion, sanityHelpBox, SketchRendererUIData.BaseFieldMargins);
                }
                else if (!fieldAsset.HasDirtyProperties() && sanityHelpBox != null)
                {
                    TonalArtMapGenerator.OnTonalArtMapGenerated -= CheckSanityBoxValid;
                    sanityHelpBox = null;
                }

                var tonalArtMapRegion = new VisualElement();
                var tonalArtMapLabel = new Label("Tonal Art Map Settings");
                SketchRendererUIUtils.AddWithMargins(tonalArtMapRegion, tonalArtMapLabel, SketchRendererUIData.MinorFieldMargins);
                
                if (tonalArtMapAssetEditor == null)
                    tonalArtMapAssetEditor = UnityEditor.Editor.CreateEditor(fieldAsset);
                var tonesEditorElement = tonalArtMapAssetEditor.CreateInspectorGUI();
                tonesEditorElement.RegisterCallback<SerializedPropertyChangeEvent>(TonalArtMapAssetData_Changed);
                SketchRendererUIUtils.AddWithMargins(tonalArtMapRegion, tonesEditorElement, SketchRendererUIData.MinorFieldMargins);

                TextureResolution initialResolution;
                if(fieldAsset.Tones[0] != null)
                    initialResolution = TextureAssetManager.GetClosestResolutionFromTexture(fieldAsset.Tones[0]);
                else
                    initialResolution = TonalArtMapGenerator.TargetResolution;
                var resolutionField = SketchRendererUI.SketchEnumField("Texture Size", initialResolution, changeCallback:Resolution_Changed);
                
                SketchRendererUIUtils.AddWithMargins(tonalArtMapRegion, resolutionField.Container, SketchRendererUIData.MinorFieldMargins);
            
                ClampedIntegerManipulator manipulator = new ClampedIntegerManipulator(1, 100);
                var iterationsField = SketchRendererUI.SketchIntegerField("Strokes per Iteration", TonalArtMapGenerator.IterationsPerStroke, isDelayed:true, manipulator:manipulator, changeCallback:IterationsPerStroke_Changed);
                SketchRendererUIUtils.AddWithMargins(tonalArtMapRegion, iterationsField.Container, SketchRendererUIData.MinorFieldMargins);
                
                SketchRendererUIUtils.AddWithMargins(outputSettingsRegion, tonalArtMapRegion, SketchRendererUIData.BaseFieldMargins);
                
                
                //Only allow editing if data asset is mutable
                outputSettingsRegion.ToggleElementInteractions(TonalArtMapGenerator.HasNonDefaultTonalArtMapAsset);
                //Ensure required options are always enabled
                tonalArtMapAssetField.Container.SetEnabled(true);
                if(setActiveButton != null)
                    setActiveButton.SetEnabled(true);
                if(!TonalArtMapGenerator.HasNonDefaultTonalArtMapAsset)
                    SketchRendererUIUtils.AddWithMargins(outputSettingsRegion, SketchRendererUI.SketchInmutableAssetHelpBox(), SketchRendererUIData.BaseFieldMargins);
            }
            else
            {
                var createCustomTonalArtMap = new Button(CreateTonalArtMap_Clicked) { text = "Create And Assign to Renderer Context"};
                SketchRendererUIUtils.AddWithMargins(outputSettingsRegion, createCustomTonalArtMap, SketchRendererUIData.BaseFieldMargins);
            }
            
            return outputSettingsRegion;
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

            var generateTamButton = SketchRendererUI.SketchMajorButton("Generate Tonal Art Map", GenerateTonalArtMap_Clicked);
            SketchRendererUIUtils.AddWithMargins(settingsContainer, generateTamButton, SketchRendererUIData.MinorFieldMargins);
            generateTamButton.SetEnabled(TonalArtMapGenerator.IsGeneratorValid() && TonalArtMapGenerator.HasNonDefaultTonalArtMapAsset);
            
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

            UpdatePreviewTargetTexture(TonalArtMapGenerator.TargetTexture);
            previewPanel.Add(previewImage);
            return previewPanel;
        }
        
        #endregion

        internal void EnsureMaxPreviewSize()
        {
            float windowWidth = root.resolvedStyle.width;
            previewPanel.style.height = windowWidth;
        }

        internal void UpdatePreviewTargetTexture(RenderTexture texture)
        {
            if (previewImage != null && texture == TonalArtMapGenerator.TargetTexture)
            {
                previewImage.visible = (strokeAssetField != null && strokeAssetField.Field.value != null);
                previewImage.image = TonalArtMapGenerator.TargetTexture;
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
                strokeAssetField.Field.SetValueWithoutNotify(TonalArtMapGenerator.StrokeDataAsset);
                tonalArtMapAssetField.Field.SetValueWithoutNotify(TonalArtMapGenerator.TonalArtMapAsset);
            }
        }
        
        // -- Stroke Panel
        internal void StrokeAsset_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            StrokeAsset strokeAsset = (StrokeAsset)bind.newValue;
            TonalArtMapGenerator.StrokeDataAsset = strokeAsset;
            settings.PersistentStrokeAsset = strokeAsset;
            ForceRebuildGUI();
        }
        
        internal void CreateStrokeAsset_Clicked()
        {
            StrokeAsset strokeAsset = StrokeAssetWizard.CreateStrokeAsset((StrokeSDFType)strokeCreationField.value);
            strokeAssetField.Field.value = strokeAsset;
        }

        internal void StrokeData_Changed(SerializedPropertyChangeEvent bind) => ForceRepaint();
        
        // -- Output Panel

        internal void TonalArtMap_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            if (TonalArtMapGenerator.TonalArtMapAsset != null)
                TonalArtMapGenerator.TonalArtMapAsset.OnTonesPacked -= ForceRebuildGUI;
            
            TonalArtMapAsset tonalArtMapAsset = (TonalArtMapAsset)bind.newValue;
            if(tonalArtMapAsset != null)
                tonalArtMapAsset.OnTonesPacked += ForceRebuildGUI;
            TonalArtMapGenerator.TonalArtMapAsset = tonalArtMapAsset;
            settings.PersistentTonalArtMapAsset = tonalArtMapAsset;
            ForceRebuildGUI();
        }

        internal void TonalArtMapAssetData_Changed(SerializedPropertyChangeEvent bind)
        {
            CheckSanityBoxValid();
        }

        internal void CreateTonalArtMap_Clicked()
        {
            TonalArtMapAsset asset = TonalArtMapWizard.CreateTonalArtMapAndSetActive();
            tonalArtMapAssetField.Field.value = asset;
        }

        internal void SetActiveTonalArtMap_Clicked()
        {
            TonalArtMapWizard.SetAsCurrentTonalArtMap((TonalArtMapAsset)tonalArtMapAssetField.Field.value);
            ForceRebuildGUI();
        }

        internal void CheckSanityBoxValid()
        {
            if (tonalArtMapAssetField.Field.value == null)
                return;
            
            TonalArtMapAsset asset = (TonalArtMapAsset)tonalArtMapAssetField.Field.value;
            if ((asset.HasDirtyProperties() && sanityHelpBox == null) || (!asset.HasDirtyProperties() && sanityHelpBox != null))
            {
                ForceRebuildGUI();
                if(sanityHelpBox != null)
                    settingsPanel.ScrollTo(sanityHelpBox);
            }
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
            pathField.Field.value = path;
        }
        
        internal void GenerateTonalArtMap_Clicked()
        {
            TonalArtMapGenerator.OnTonalArtMapGenerationStep += UpdateToolStatus;
            TonalArtMapGenerator.GenerateTAMToneTextures();
        }
        
        #endregion
    }
}