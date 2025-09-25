using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    [CustomPropertyDrawer(typeof(AccentedOutlinePassData))]
    internal class AccentedOutlinePassDataDrawer : PropertyDrawer
    {
        private SerializedProperty outlineMaskProp;
        
        VisualElement passDataField;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            passDataField = new VisualElement();
            
            var distortionField = new VisualElement();
            var distortionLabel = new Label("Distortion");
            distortionField.Add(distortionLabel);
            SerializedProperty bakeProp = property.FindPropertyRelative("BakeDistortionDuringRuntime");
            var bakeField = SketchRendererUI.SketchBoolProperty(bakeProp, nameOverride: "Bake Distortion At Awake");
            bakeField.Field.RegisterValueChangedCallback(_ => TriggerRepaint());
            SketchRendererUIUtils.AddWithMargins(distortionField, bakeField.Container, SketchRendererUIData.MajorIndentCorners);

            if (bakeProp.boolValue)
            {
                var bakedScaleFactorProp = property.FindPropertyRelative("BakedTextureScaleFactor");
                var bakedScaleFactorField = SketchRendererUI.SketchFloatSliderPropertyWithInput(bakedScaleFactorProp, "Bake Resolution Scale");
                SketchRendererUIUtils.AddWithMargins(distortionField, bakedScaleFactorField.Container, SketchRendererUIData.MajorIndentCorners);
            }
            
            SerializedProperty rateProp = property.FindPropertyRelative("Rate");
            var rateField = SketchRendererUI.SketchFloatProperty(rateProp);
            SketchRendererUIUtils.AddWithMargins(distortionField, rateField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty strengthProp = property.FindPropertyRelative("Strength");
            var strengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(strengthProp);
            SketchRendererUIUtils.AddWithMargins(distortionField, strengthField.Container, SketchRendererUIData.MajorIndentCorners);
            
            passDataField.Add(distortionField);
            
            
            var additionalLinesField = new VisualElement();
            var additionalLinesLabel = new Label("Additional Lines");
            additionalLinesField.Add(additionalLinesLabel);
            SerializedProperty additionalLineNumberProp = property.FindPropertyRelative("AdditionalLines");
            var additionalLineNumberField = SketchRendererUI.SketchIntSliderPropertyWithInput(additionalLineNumberProp);
            SketchRendererUIUtils.AddWithMargins(additionalLinesField, additionalLineNumberField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty additionalLineTintProp = property.FindPropertyRelative("AdditionalLineTintPersistence");
            var additionalLineTintField = SketchRendererUI.SketchFloatSliderPropertyWithInput(additionalLineTintProp, nameOverride: "Tint Persistence");
            SketchRendererUIUtils.AddWithMargins(additionalLinesField, additionalLineTintField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty additionalLineDistortionProp = property.FindPropertyRelative("AdditionalLineDistortionJitter");
            var additionalLineDistortionField = SketchRendererUI.SketchFloatSliderPropertyWithInput(additionalLineDistortionProp, nameOverride: "Distortion Jitter");
            SketchRendererUIUtils.AddWithMargins(additionalLinesField, additionalLineDistortionField.Container, SketchRendererUIData.MajorIndentCorners);
            
            passDataField.Add(additionalLinesField);
            
            var maskingField = new VisualElement();
            var maskingLabel = new Label("Additional Lines");
            maskingField.Add(maskingLabel);
            
            outlineMaskProp = property.FindPropertyRelative("PencilOutlineMask");
            var outlineMaskField = SketchRendererUI.SketchObjectField("Outline Mask Texture", typeof(Texture2D), outlineMaskProp.objectReferenceValue, changeCallback:OutlineMask_Changed);
            SketchRendererUIUtils.AddWithMargins(maskingField, outlineMaskField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty maskScaleProp = property.FindPropertyRelative("MaskScale");
            var maskScaleField = SketchRendererUI.SketchVector2Property(maskScaleProp, nameOverride: "Mask Scale");
            SketchRendererUIUtils.AddWithMargins(maskingField, maskScaleField.Container, SketchRendererUIData.MajorIndentCorners);
            
            passDataField.Add(maskingField);
            
            
            return passDataField;
        }

        internal void TriggerRepaint()
        {
            passDataField.SendEvent(ExecuteCommandEvent.GetPooled(SketchRendererUIData.RepaintEditorCommand));
        }
        
        internal void OutlineMask_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            outlineMaskProp.serializedObject.Update();
            outlineMaskProp.objectReferenceValue = bind.newValue;
            outlineMaskProp.serializedObject.ApplyModifiedProperties();
        }
    }
}   