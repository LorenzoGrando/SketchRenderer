using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    public class ClampedFloatManipulator : Manipulator, ISketchManipulator<FloatField>
    {
        private FloatField floatField;
        public float minValue;
        public float maxValue;

        public ClampedFloatManipulator(float minValue, float maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public void Initialize(FloatField floatField)
        {
            this.floatField = floatField;
            
            floatField.RegisterValueChangedCallback(OnValueChanged);
        }
        
        public Manipulator GetBaseManipulator() => this;

        private void OnValueChanged(ChangeEvent<float> evt)
        {
            float clamp = Mathf.Clamp(evt.newValue, minValue, maxValue);
            if (clamp != evt.newValue)
                floatField.SetValueWithoutNotify(clamp);
        }
        
        
        protected override void RegisterCallbacksOnTarget()
        {
        }
        protected override void UnregisterCallbacksFromTarget()
        {
            floatField.UnregisterValueChangedCallback(OnValueChanged);
        }
    }
}