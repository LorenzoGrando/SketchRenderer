using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEditor;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    [CustomPropertyDrawer(typeof(RenderUVsPassData))]
    public class RenderUVsPassDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var passDataField = new VisualElement();
            
            SerializedProperty skyboxRotationProp = property.FindPropertyRelative("SkyboxRotation");
            var skyboxRotationField = SketchRendererUI.SketchFloatSliderPropertyWithInput(skyboxRotationProp);
            SketchRendererUIUtils.AddWithMargins(passDataField, skyboxRotationField.Container, CornerData.Empty);
            
            return passDataField;
        }
    }
}