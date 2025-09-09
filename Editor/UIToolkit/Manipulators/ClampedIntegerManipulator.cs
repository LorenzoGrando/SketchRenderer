using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    public class ClampedIntegerManipulator : Manipulator, ISketchManipulator<IntegerField>
    {
        private IntegerField integerField;
        public int minValue;
        public int maxValue;

        public ClampedIntegerManipulator(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public void Initialize(IntegerField integerField)
        {
            this.integerField = integerField;
            
            integerField.RegisterValueChangedCallback(OnValueChanged);
        }
        
        public Manipulator GetBaseManipulator() => this;

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