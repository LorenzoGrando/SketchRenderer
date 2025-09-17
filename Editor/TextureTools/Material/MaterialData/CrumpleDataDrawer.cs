using SketchRenderer.Editor.UIToolkit;
using TextureTools.Material;
using UnityEditor;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.MaterialData
{
    [CustomPropertyDrawer(typeof(CrumpleData))]
    public class CrumpleDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var assetField = new VisualElement();

            var scaleManipulator = new ClampedVector2IntManipulator(1, 10);
            var scaleField = SketchRendererUI.SketchVector2IntProperty(property.FindPropertyRelative("CrumpleScale"), scaleManipulator, nameOverride:"Scale");
            SketchRendererUIUtils.AddWithMargins(assetField, scaleField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var detailLevelField = SketchRendererUI.SketchIntSliderPropertyWithInput(property.FindPropertyRelative("CrumpleDetailLevel"), nameOverride:"Detail Level");
            SketchRendererUIUtils.AddWithMargins(assetField, detailLevelField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var detailFrequencyField = SketchRendererUI.SketchIntSliderPropertyWithInput(property.FindPropertyRelative("CrumpleDetailFrequency"), nameOverride:"Detail Frequency");
            SketchRendererUIUtils.AddWithMargins(assetField, detailFrequencyField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var detailPersistenceField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("CrumpleDetailPersistence"), nameOverride:"Detail Persistence");
            SketchRendererUIUtils.AddWithMargins(assetField, detailPersistenceField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var jitterField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("CrumpleJitter"), nameOverride:"Jitter");
            SketchRendererUIUtils.AddWithMargins(assetField, jitterField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var strengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("CrumpleStrength"), nameOverride:"Strength");
            SketchRendererUIUtils.AddWithMargins(assetField, strengthField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var tintStrengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("CrumpleTintStrength"), nameOverride:"Tint Strength");
            SketchRendererUIUtils.AddWithMargins(assetField, tintStrengthField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var tintSharpnessField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("CrumpleTintSharpness"), nameOverride:"Tint Sharpness");
            SketchRendererUIUtils.AddWithMargins(assetField, tintSharpnessField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var tintField = SketchRendererUI.SketchColorProperty(property.FindPropertyRelative("CrumpleTint"), nameOverride:"Tint");
            SketchRendererUIUtils.AddWithMargins(assetField, tintField.Container, SketchRendererUIData.MajorIndentCorners);

            return assetField;
        }
    }
}