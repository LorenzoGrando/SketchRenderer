using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    [CustomPropertyDrawer(typeof(MaterialSurfacePassData))]
    internal class MaterialSurfacePassDataDrawer : PropertyDrawer
    {
        private VisualElement passDataField;
        private SerializedProperty AlbedoTextureProp;
        private SerializedProperty DirectionalTextureProp;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            passDataField = new VisualElement();
            
            var projectionRegion = new VisualElement();
            var projectionLabel = new Label("Projection");
            projectionRegion.Add(projectionLabel);
            
            SerializedProperty projectionProp = property.FindPropertyRelative("ProjectionMethod");
            TextureProjectionGlobalData.TextureProjectionMethod method = (TextureProjectionGlobalData.TextureProjectionMethod)projectionProp.enumValueIndex;
            var projectionField = SketchRendererUI.SketchEnumProperty(projectionProp, method); 
            projectionField.Field.RegisterValueChangedCallback(_ => ForceRepaint());
            SketchRendererUIUtils.AddWithMargins(projectionRegion, projectionField.Container, SketchRendererUIData.MajorIndentCorners);
            if (method is TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_CONSTANT_SCALE) //or TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_REVERSED_CONSTANT_SCALE)
            {
                var falloffField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("ConstantScaleFalloffFactor"), nameOverride:"Falloff Factor");
                SketchRendererUIUtils.AddWithMargins(projectionRegion, falloffField.Container, SketchRendererUIData.MajorIndentCorners);
            }
            
            SketchRendererUIUtils.AddWithMargins(passDataField, projectionRegion, CornerData.Empty);
            
            var texturingRegion = new VisualElement();
            var texturingLabel = new Label("Texturing");
            texturingRegion.Add(texturingLabel);
            
            AlbedoTextureProp = property.FindPropertyRelative("AlbedoTexture");
            var albedoTextureField = SketchRendererUI.SketchObjectField("Albedo Texture", typeof(Texture2D), AlbedoTextureProp.objectReferenceValue, changeCallback:AlbedoField_Changed);
            SketchRendererUIUtils.AddWithMargins(texturingRegion, albedoTextureField.Container, SketchRendererUIData.MajorIndentCorners);
            
            DirectionalTextureProp = property.FindPropertyRelative("NormalTexture");
            var directionalTextureField = SketchRendererUI.SketchObjectField("Normal Texture", typeof(Texture2D), DirectionalTextureProp.objectReferenceValue, changeCallback:DirectionalField_Changed);
            SketchRendererUIUtils.AddWithMargins(texturingRegion, directionalTextureField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var scalesField = SketchRendererUI.SketchVector2IntProperty(property.FindPropertyRelative("Scale"));
            SketchRendererUIUtils.AddWithMargins(texturingRegion, scalesField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var colorBlendField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("BaseColorBlendFactor"), nameOverride:"Base Color Blend");
            SketchRendererUIUtils.AddWithMargins(texturingRegion, colorBlendField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SketchRendererUIUtils.AddWithMargins(passDataField, texturingRegion, CornerData.Empty);

            return passDataField;
        }

        internal void ForceRepaint()
        {
            passDataField.SendEvent(ExecuteCommandEvent.GetPooled(SketchRendererUIData.RepaintEditorCommand));
        }

        internal void AlbedoField_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            AlbedoTextureProp.serializedObject.Update();
            AlbedoTextureProp.objectReferenceValue = bind.newValue;
            AlbedoTextureProp.serializedObject.ApplyModifiedProperties();
        }
        
        internal void DirectionalField_Changed(ChangeEvent<UnityEngine.Object> bind)
        {
            DirectionalTextureProp.serializedObject.Update();
            DirectionalTextureProp.objectReferenceValue = bind.newValue;
            DirectionalTextureProp.serializedObject.ApplyModifiedProperties();
        }
    }
}