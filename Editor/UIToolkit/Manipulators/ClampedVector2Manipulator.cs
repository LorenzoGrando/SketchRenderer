using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    public class ClampedVector2Manipulator : Manipulator, ISketchManipulator<Vector2Field>
    {
        private Vector2Field vectorField;
        public float minValue;
        public float maxValue;

        public ClampedVector2Manipulator(float minValue, float maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public void Initialize(Vector2Field vectorField)
        {
            this.vectorField = vectorField;

            vectorField.RegisterValueChangedCallback(OnValueChanged);
        }

        public Manipulator GetBaseManipulator() => this;

        private void OnValueChanged(ChangeEvent<Vector2> evt)
        {
            float clampX = Mathf.Clamp(evt.newValue.x, minValue, maxValue);
            float clampY = Mathf.Clamp(evt.newValue.y, minValue, maxValue);
            if (clampX != evt.newValue.x || clampY != evt.newValue.y)
                vectorField.SetValueWithoutNotify(new Vector2(clampX, clampY));
        }


        protected override void RegisterCallbacksOnTarget()
        {
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            vectorField.UnregisterValueChangedCallback(OnValueChanged);
        }
    }
}