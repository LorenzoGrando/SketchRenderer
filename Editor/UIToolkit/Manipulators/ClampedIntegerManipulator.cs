using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    public class ClampedIntegerManipulator : Manipulator
    {
        private IntegerField integerField;
        public int minValue;
        public int maxValue;

        public ClampedIntegerManipulator(IntegerField integerField, int minValue, int maxValue)
        {
            this.integerField = integerField;
            this.minValue = minValue;
            this.maxValue = maxValue;

            integerField.RegisterValueChangedCallback(OnValueChanged);
        }

        private void OnValueChanged(ChangeEvent<int> evt)
        {
            int clamp = Mathf.Clamp(evt.newValue, minValue, maxValue);
            if (clamp != evt.newValue)
                integerField.SetValueWithoutNotify(clamp);
        }
        
        
        protected override void RegisterCallbacksOnTarget()
        {
        }
        protected override void UnregisterCallbacksFromTarget()
        {
            integerField.UnregisterValueChangedCallback(OnValueChanged);
        }
    }
}