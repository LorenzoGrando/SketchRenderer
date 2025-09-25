using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;
using UnityEditor;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    [CustomPropertyDrawer(typeof(LuminancePassData))]
    public class LuminancePassDataDrawer : PropertyDrawer
    {
        private VisualElement passDataField;
        private SerializedProperty AlbedoTextureProp;
        private SerializedProperty DirectionalTextureProp;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            passDataField = new VisualElement();
            
            SerializedProperty tamProp = property.FindPropertyRelative("ActiveTonalMap");
            var tonalArtMapField = SketchRendererUI.SketchObjectField("Tonal Art Map Asset", typeof(TonalArtMapAsset), tamProp.objectReferenceValue);
            SketchRendererUIUtils.AddWithMargins(passDataField, tonalArtMapField.Container, CornerData.Empty);
            
            
            var projectionRegion = new VisualElement();
            var projectionLabel = new Label("Projection");
            projectionRegion.Add(projectionLabel);
            
            SerializedProperty projectionProp = property.FindPropertyRelative("ProjectionMethod");
            TextureProjectionGlobalData.TextureProjectionMethod method = (TextureProjectionGlobalData.TextureProjectionMethod)projectionProp.enumValueIndex;
            var projectionField = SketchRendererUI.SketchEnumProperty(projectionProp, method); 
            projectionField.Field.RegisterValueChangedCallback(_ => ForceRepaint());
            SketchRendererUIUtils.AddWithMargins(projectionRegion, projectionField.Container, SketchRendererUIData.MajorIndentCorners);
            if (method is TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_CONSTANT_SCALE or TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_REVERSED_CONSTANT_SCALE)
            {
                var falloffField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("ConstantScaleFalloffFactor"), nameOverride:"Falloff Factor");
                SketchRendererUIUtils.AddWithMargins(projectionRegion, falloffField.Container, SketchRendererUIData.MajorIndentCorners);
            }
            
            SketchRendererUIUtils.AddWithMargins(passDataField, projectionRegion, CornerData.Empty);
            
            
            var texturingRegion = new VisualElement();
            var texturingLabel = new Label("Texturing");
            texturingRegion.Add(texturingLabel);
            
            SerializedProperty scaleProp = property.FindPropertyRelative("ToneScales");
            var scaleField = SketchRendererUI.SketchVector2Property(scaleProp);
            SketchRendererUIUtils.AddWithMargins(texturingRegion, scaleField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty smoothProp = property.FindPropertyRelative("SmoothTransitions");
            var smoothField = SketchRendererUI.SketchBoolProperty(smoothProp);
            SketchRendererUIUtils.AddWithMargins(texturingRegion, smoothField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty lumOffsetProp = property.FindPropertyRelative("LuminanceScalar");
            var lumOffsetField = SketchRendererUI.SketchFloatSliderPropertyWithInput(lumOffsetProp, "Luminance Offset");
            SketchRendererUIUtils.AddWithMargins(texturingRegion, lumOffsetField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SketchRendererUIUtils.AddWithMargins(passDataField, texturingRegion, CornerData.Empty);
            
            
            return passDataField;
        }

        internal void ForceRepaint()
        {
            passDataField.SendEvent(ExecuteCommandEvent.GetPooled(SketchRendererUIData.RepaintEditorCommand));
        }
    }
}