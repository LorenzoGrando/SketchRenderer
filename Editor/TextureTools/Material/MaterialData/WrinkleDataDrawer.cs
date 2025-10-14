using SketchRenderer.Editor.UIToolkit;
using TextureTools.Material;
using UnityEditor;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.TextureTools.MaterialData
{
    [CustomPropertyDrawer(typeof(WrinkleData))]
    public class WrinkleDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var assetField = new VisualElement();

            var scaleManipulator = new ClampedVector2IntManipulator(2, 20);
            var scaleField = SketchRendererUI.SketchVector2IntProperty(property.FindPropertyRelative("WrinkleScale"), scaleManipulator, nameOverride:"Scale");
            SketchRendererUIUtils.AddWithMargins(assetField, scaleField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var detailLevelField = SketchRendererUI.SketchIntSliderPropertyWithInput(property.FindPropertyRelative("WrinkleDetailLevel"), nameOverride:"Detail Level");
            SketchRendererUIUtils.AddWithMargins(assetField, detailLevelField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var detailFrequencyField = SketchRendererUI.SketchIntSliderPropertyWithInput(property.FindPropertyRelative("WrinkleDetailFrequency"), nameOverride:"Detail Frequency");
            SketchRendererUIUtils.AddWithMargins(assetField, detailFrequencyField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var detailPersistenceField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("WrinkleDetailPersistence"), nameOverride:"Detail Persistence");
            SketchRendererUIUtils.AddWithMargins(assetField, detailPersistenceField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var jitterField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("WrinkleJitter"), nameOverride:"Jitter");
            SketchRendererUIUtils.AddWithMargins(assetField, jitterField.Container, SketchRendererUIData.MajorIndentCorners);
            
            var strengthField = SketchRendererUI.SketchFloatSliderPropertyWithInput(property.FindPropertyRelative("WrinkleStrength"), nameOverride:"Strength");
            SketchRendererUIUtils.AddWithMargins(assetField, strengthField.Container, SketchRendererUIData.MajorIndentCorners);

            return assetField;
        }
    }
}