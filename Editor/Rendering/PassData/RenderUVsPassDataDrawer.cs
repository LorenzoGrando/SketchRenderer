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
            
            SerializedProperty skyboxRotationProp = property.FindPropertyRelative("SkyboxRotationStep");
            var skyboxRotationField = SketchRendererUI.SketchIntSliderPropertyWithInput(skyboxRotationProp, nameOverride:"Skybox Orientation");
            SketchRendererUIUtils.AddWithMargins(passDataField, skyboxRotationField.Container, CornerData.Empty);
            
            SerializedProperty skyboxScaleProp = property.FindPropertyRelative("SkyboxScale");
            var skyboxScaleField = SketchRendererUI.SketchIntegerProperty(skyboxScaleProp);
            SketchRendererUIUtils.AddWithMargins(passDataField, skyboxScaleField.Container, CornerData.Empty);
            
            return passDataField;
        }
    }
}