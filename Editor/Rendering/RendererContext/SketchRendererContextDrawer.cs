using SketchRenderer.Editor.TextureTools;
using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    [CustomEditor(typeof(SketchRendererContext))]
    internal class SketchRendererContextDrawer : UnityEditor.Editor
    {
        private VisualElement root;
        private bool hadSingleOutline;
        SerializedProperty UseSmoothOutlineProp;
        SerializedProperty UseSketchyOutlineProp;

        private bool assignedListeners;
        
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            
            if(root == null)
                root = new VisualElement();
            
            ConstructAssetField();
            
            return root;
        }

        internal void ConstructAssetField()
        {
            var assetField = new VisualElement();
            
            SketchRendererContext context = (SketchRendererContext)target;
            if (!assignedListeners)
            {
                context.OnRedrawSettings += ForceRepaint;
                assignedListeners = true;
            }

            //- UVS Feature
            if (context.UseUVsFeature)
            {
                var uvDataFoldout = SketchRendererUI.SketchFoldout("Object UV Feature", applyMargins:false, applyPadding:true);
            
                SerializedProperty uvDataProp = serializedObject.FindProperty("UVSFeatureData");
                var uvDataField = new PropertyField(uvDataProp);
                uvDataField.BindProperty(uvDataProp);
                uvDataField.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnFeatureDataChanged(SketchRendererFeatureType.UVS));
                SketchRendererUIUtils.AddWithMargins(uvDataFoldout, uvDataField, SketchRendererUIData.MinorFieldMargins);
                
                assetField.Add(uvDataFoldout);
            }
            
            //- MaterialSurface Feature
            SerializedProperty useMaterialDataProp = serializedObject.FindProperty("UseMaterialFeature");
            var materialDataFoldout = SketchRendererUI.SketchFoldoutWithToggle("Material Feature", useMaterialDataProp, applyMargins:false, applyPadding:true);
            
            SerializedProperty materialDataProp = serializedObject.FindProperty("MaterialFeatureData");
            var materialDataField = new PropertyField(materialDataProp);
            materialDataField.BindProperty(materialDataProp);
            materialDataField.RegisterCallback<ExecuteCommandEvent>(TriggerRepaint);
            materialDataField.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnFeatureDataChanged(SketchRendererFeatureType.MATERIAL));
            SketchRendererUIUtils.AddWithMargins(materialDataFoldout, materialDataField, SketchRendererUIData.MinorFieldMargins);
            if (!context.MaterialFeatureData.IsAllPassDataValid())
            {
                var openCreator = SketchRendererUI.SketchMajorButton("Open Material Generator", OpenMaterialGenerator_Clicked);
                SketchRendererUIUtils.AddWithMargins(materialDataFoldout, openCreator, SketchRendererUIData.MinorFieldMargins);
            }
            
            assetField.Add(materialDataFoldout);
            
            //- Luminance Feature
            SerializedProperty useLuminanceDataProp = serializedObject.FindProperty("UseLuminanceFeature");
            var luminanceDataFoldout = SketchRendererUI.SketchFoldoutWithToggle("Luminance Feature", useLuminanceDataProp, applyMargins:false, applyPadding:true);
            
            SerializedProperty luminanceDataProp = serializedObject.FindProperty("LuminanceFeatureData");
            var luminanceDataField = new PropertyField(luminanceDataProp);
            luminanceDataField.BindProperty(luminanceDataProp);
            luminanceDataField.RegisterCallback<ExecuteCommandEvent>(TriggerRepaint);
            luminanceDataField.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnFeatureDataChanged(SketchRendererFeatureType.LUMINANCE));
            SketchRendererUIUtils.AddWithMargins(luminanceDataFoldout, luminanceDataField, SketchRendererUIData.MinorFieldMargins);
            if (!context.LuminanceFeatureData.IsAllPassDataValid())
            {
                if (context.LuminanceFeatureData.ActiveTonalMap != null && !context.LuminanceFeatureData.ActiveTonalMap.IsPacked)
                {
                    var warningBox = SketchRendererUI.SketchHelpBox($"The current textures in the asset have not properly been packed.\n Please regenerate the tones to automatically pack them");
                    SketchRendererUIUtils.AddWithMargins(luminanceDataFoldout, warningBox, SketchRendererUIData.MinorFieldMargins);
                    context.LuminanceFeatureData.ActiveTonalMap.OnTonesPacked += OnActiveTonalArtMapPacked;
                }
                var openCreatorTam = SketchRendererUI.SketchMajorButton("Open Tonal Art Map Generator", OpenTonalArtMapGenerator_Clicked);
                SketchRendererUIUtils.AddWithMargins(luminanceDataFoldout, openCreatorTam, SketchRendererUIData.MinorFieldMargins);
            }
            
            assetField.Add(luminanceDataFoldout);
            
            //- Outline Features
            
            //Toggle between outline features
            UseSmoothOutlineProp = serializedObject.FindProperty("UseSmoothOutlineFeature");
            UseSketchyOutlineProp = serializedObject.FindProperty("UseSketchyOutlineFeature");
            
            //Never allow two features to be present
            if (UseSmoothOutlineProp.boolValue && UseSketchyOutlineProp.boolValue)
            {
                UseSmoothOutlineProp.boolValue = false;
                UseSketchyOutlineProp.boolValue = false;
                serializedObject.ApplyModifiedProperties();
            }
            //Only display the currently selected option
            if (UseSmoothOutlineProp.boolValue)
                assetField.Add(ConstructSmoothOutlineFeature(UseSmoothOutlineProp));
            else if (UseSketchyOutlineProp.boolValue)
                assetField.Add(ConstructSketchyOutlineFeature(UseSketchyOutlineProp));
            //Display both as inactive
            else
            {
                assetField.Add(ConstructSmoothOutlineFeature(UseSmoothOutlineProp));
                assetField.Add(ConstructSketchyOutlineFeature(UseSketchyOutlineProp));
            }
            
            //- Composition Field
            var compositionFoldout = SketchRendererUI.SketchFoldout("Composition Feature", applyMargins:false, applyPadding:true);
            
            SerializedProperty compositionDataProp = serializedObject.FindProperty("CompositionFeatureData");
            var compositionDataField = new PropertyField(compositionDataProp);
            compositionDataField.BindProperty(compositionDataProp);
            compositionDataField.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnFeatureDataChanged(SketchRendererFeatureType.COMPOSITOR));
            SketchRendererUIUtils.AddWithMargins(compositionFoldout, compositionDataField, SketchRendererUIData.MinorFieldMargins);
            
            assetField.Add(compositionFoldout);
            
            
            SketchRendererUIUtils.AddWithMargins(root, assetField, CornerData.Empty);
        }

        internal PropertyField ConstructEdgeDetectionElement()
        {
            SerializedProperty edgeDetectionProp = serializedObject.FindProperty("EdgeDetectionFeatureData");
            var edgeField = new PropertyField(edgeDetectionProp);
            edgeField.BindProperty(edgeDetectionProp);
            return edgeField;
        }

        internal Foldout ConstructSmoothOutlineFeature(SerializedProperty UseSmoothOutlineProp)
        {
            var smoothFoldout = SketchRendererUI.SketchFoldoutWithToggle("Smooth Outline Feature", UseSmoothOutlineProp, false, true, DynamicToggleCallback);
            
            var edgeField = ConstructEdgeDetectionElement();
            edgeField.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnFeatureDataChanged(SketchRendererFeatureType.OUTLINE_SMOOTH));
            edgeField.RegisterCallback<ExecuteCommandEvent>(TriggerRepaint);
            SketchRendererUIUtils.AddWithMargins(smoothFoldout, edgeField, SketchRendererUIData.MinorFieldMargins);
            
            SerializedProperty accentedDataProp = serializedObject.FindProperty("AccentedOutlineFeatureData");
            SerializedProperty useAccentedProp = accentedDataProp.FindPropertyRelative("UseAccentedOutlines");
            var accentedFoldout = SketchRendererUI.SketchFoldoutWithToggle("Accented Effects", useAccentedProp, applyMargins:false, applyPadding:true);
            
            var accentedDataField = new PropertyField(accentedDataProp);
            accentedDataField.RegisterCallback<ExecuteCommandEvent>(TriggerRepaint);
            accentedDataField.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnFeatureDataChanged(SketchRendererFeatureType.OUTLINE_SMOOTH));
            accentedDataField.BindProperty(accentedDataProp);
            SketchRendererUIUtils.AddWithMargins(accentedFoldout, accentedDataField, CornerData.Empty);
            SketchRendererUIUtils.AddWithMargins(smoothFoldout, accentedFoldout, SketchRendererUIData.MinorFieldMargins);
            
            SerializedProperty thicknessDataProp = serializedObject.FindProperty("ThicknessDilationFeatureData");
            SerializedProperty useThicknessProp = thicknessDataProp.FindPropertyRelative("UseThicknessDilation");
            var thicknessFoldout = SketchRendererUI.SketchFoldoutWithToggle("Thickness Control", useThicknessProp, applyMargins:false, applyPadding:true);
            
            var thicknessDataField = new PropertyField(thicknessDataProp);
            thicknessDataField.BindProperty(thicknessDataProp);
            thicknessDataField.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnFeatureDataChanged(SketchRendererFeatureType.OUTLINE_SMOOTH));
            SketchRendererUIUtils.AddWithMargins(thicknessFoldout, thicknessDataField, CornerData.Empty);
            SketchRendererUIUtils.AddWithMargins(smoothFoldout, thicknessFoldout, SketchRendererUIData.MinorFieldMargins);
            
            return smoothFoldout;
        }
        
        internal Foldout ConstructSketchyOutlineFeature(SerializedProperty UseSketchyOutlineProp)
        {
            var sketchFoldout = SketchRendererUI.SketchFoldoutWithToggle("Sketchy Outline Feature", UseSketchyOutlineProp, false, true, DynamicToggleCallback);
            
            var edgeField = ConstructEdgeDetectionElement();
            edgeField.RegisterCallback<ExecuteCommandEvent>(TriggerRepaint);
            edgeField.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnFeatureDataChanged(SketchRendererFeatureType.OUTLINE_SKETCH));
            SketchRendererUIUtils.AddWithMargins(sketchFoldout, edgeField, SketchRendererUIData.MinorFieldMargins);
            
            SerializedProperty sketchyDataProp = serializedObject.FindProperty("SketchyOutlineFeatureData");
            var sketchyDataField = new PropertyField(sketchyDataProp);
            sketchyDataField.BindProperty(sketchyDataProp);
            sketchyDataField.RegisterCallback<ExecuteCommandEvent>(TriggerRepaint);
            sketchyDataField.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnFeatureDataChanged(SketchRendererFeatureType.OUTLINE_SKETCH));
            SketchRendererUIUtils.AddWithMargins(sketchFoldout, sketchyDataField, SketchRendererUIData.MinorFieldMargins);
            
            return sketchFoldout;
        }

        internal void DynamicToggleCallback(ChangeEvent<bool> toggleChange)
        {
            if (hadSingleOutline)
            {
                if (UseSmoothOutlineProp.boolValue == UseSketchyOutlineProp.boolValue)
                {
                    hadSingleOutline = false;
                    ForceRepaint();
                }
            }
            else
            {
                if (UseSmoothOutlineProp.boolValue != UseSketchyOutlineProp.boolValue &&
                    (UseSmoothOutlineProp.boolValue || UseSketchyOutlineProp.boolValue))
                {
                    hadSingleOutline = true;
                    ForceRepaint();
                }
            }
        }

        internal void TriggerRepaint(ExecuteCommandEvent evt)
        {
            if(evt.commandName != SketchRendererUIData.RepaintEditorCommand)
                return;
            
            ForceRepaint();
        }

        private void ForceRepaint()
        {
            root.Clear();
            ConstructAssetField();
            Repaint();
        }

        private void OnFeatureDataChanged(SketchRendererFeatureType featureType)
        {
            SketchRendererContext context = (SketchRendererContext)target;
            context.SetFeatureDirty(featureType, true);
            context.SetFeatureDirty(SketchRendererFeatureType.COMPOSITOR, true);
        }

        private void OnActiveTonalArtMapPacked()
        {
            SketchRendererContext context = (SketchRendererContext)target;
            context.LuminanceFeatureData.ActiveTonalMap.OnTonesPacked -= OnActiveTonalArtMapPacked;
            ForceRepaint();
        }

        private void OpenTonalArtMapGenerator_Clicked()
        {
            TextureToolWizard.CreateTonalArtMapWindow();
        }

        private void OpenMaterialGenerator_Clicked()
        {
            TextureToolWizard.CreateMaterialWindow();
        }
    }
}