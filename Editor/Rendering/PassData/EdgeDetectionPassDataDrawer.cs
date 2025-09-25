using SketchRenderer.Editor.UIToolkit;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEditor;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.Rendering
{
    [CustomPropertyDrawer(typeof(EdgeDetectionPassData))]
    public class EdgeDetectionPassDataDrawer : PropertyDrawer
    {
        private VisualElement passDataField;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            passDataField = new VisualElement();
            var passDataLabel = new Label("Edge Detection");
            passDataField.Add(passDataLabel);
            
            SerializedProperty methodProp = property.FindPropertyRelative("Method");
            EdgeDetectionGlobalData.EdgeDetectionMethod method = (EdgeDetectionGlobalData.EdgeDetectionMethod )methodProp.enumValueIndex;
            var methodField = SketchRendererUI.SketchEnumProperty(methodProp, method); 
            SketchRendererUIUtils.AddWithMargins(passDataField, methodField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty sourceProp = property.FindPropertyRelative("Source");
            EdgeDetectionGlobalData.EdgeDetectionSource source = (EdgeDetectionGlobalData.EdgeDetectionSource )sourceProp.enumValueIndex;
            var sourceField = SketchRendererUI.SketchEnumProperty(sourceProp, source); 
            sourceField.Field.RegisterValueChangedCallback(_ => Source_Changed());
            SketchRendererUIUtils.AddWithMargins(passDataField, sourceField.Container, SketchRendererUIData.MajorIndentCorners);
            
            SerializedProperty thresholdProp = property.FindPropertyRelative("OutlineThreshold");
            var thresholdField = SketchRendererUI.SketchFloatSliderPropertyWithInput(thresholdProp, "Threshold");
            SketchRendererUIUtils.AddWithMargins(passDataField, thresholdField.Container, SketchRendererUIData.MajorIndentCorners);

            SerializedProperty offsetProp = property.FindPropertyRelative("OutlineOffset");
            var offsetField = SketchRendererUI.SketchIntSliderPropertyWithInput(offsetProp, "Offset");
            SketchRendererUIUtils.AddWithMargins(passDataField, offsetField.Container, SketchRendererUIData.MajorIndentCorners);
            
            if (source != EdgeDetectionGlobalData.EdgeDetectionSource.COLOR)
            {
                var depthOnlyField = new VisualElement();
                var depthOnlyLabel = new Label("Depth Only");
                depthOnlyField.Add(depthOnlyLabel);
                
                SerializedProperty distanceFalloffProp = property.FindPropertyRelative("OutlineDistanceFalloff");
                var distanceFalloffField = SketchRendererUI.SketchFloatSliderPropertyWithInput(distanceFalloffProp, "Distance Falloff");
                SketchRendererUIUtils.AddWithMargins(depthOnlyField, distanceFalloffField.Container, SketchRendererUIData.MajorIndentCorners);
                SketchRendererUIUtils.AddWithMargins(passDataField, depthOnlyField, SketchRendererUIData.MajorIndentCorners);
                
                if (source != EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH)
                {
                    var normalsOnlyField = new VisualElement();
                    var normalsOnlyLabel = new Label("Normals Only");
                    normalsOnlyField.Add(normalsOnlyLabel);
                    SerializedProperty normalSensitivityProp = property.FindPropertyRelative("OutlineNormalSensitivity");
                    var normalSensitivityField = SketchRendererUI.SketchFloatSliderPropertyWithInput(normalSensitivityProp, "Normal Sensitivity");
                    SketchRendererUIUtils.AddWithMargins(normalsOnlyField, normalSensitivityField.Container, SketchRendererUIData.MajorIndentCorners);
                    
                    
                    SerializedProperty angleSensitivityProp = property.FindPropertyRelative("OutlineAngleSensitivity");
                    var angleSensitivityField = SketchRendererUI.SketchFloatSliderPropertyWithInput(angleSensitivityProp, "Angle Sensitivity");
                    SketchRendererUIUtils.AddWithMargins(normalsOnlyField, angleSensitivityField.Container, SketchRendererUIData.MajorIndentCorners);
                    
                    SerializedProperty angleConstraintProp = property.FindPropertyRelative("OutlineAngleConstraint");
                    var angleConstraintField = SketchRendererUI.SketchFloatSliderPropertyWithInput(angleConstraintProp, "Angle Constraint");
                    SketchRendererUIUtils.AddWithMargins(normalsOnlyField, angleConstraintField.Container, SketchRendererUIData.MajorIndentCorners);
                    SketchRendererUIUtils.AddWithMargins(passDataField, normalsOnlyField, SketchRendererUIData.MajorIndentCorners);
                }
            }
            
            return passDataField;
        }

        internal void Source_Changed()
        {
            passDataField.SendEvent(ExecuteCommandEvent.GetPooled(SketchRendererUIData.RepaintEditorCommand));
        }
    }
}