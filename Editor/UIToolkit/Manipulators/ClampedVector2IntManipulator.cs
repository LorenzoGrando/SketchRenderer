using UnityEngine;
using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    public class ClampedVector2IntManipulator : Manipulator, ISketchManipulator<Vector2IntField>
    {
        private Vector2IntField vectorField;
        public int minValue;
        public int maxValue;

        public ClampedVector2IntManipulator(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public void Initialize(Vector2IntField vectorField)
        {
            this.vectorField = vectorField;

            vectorField.RegisterValueChangedCallback(OnValueChanged);
        }

        public Manipulator GetBaseManipulator() => this;

        private void OnValueChanged(ChangeEvent<Vector2Int> evt)
        {
            int clampX = Mathf.Clamp(evt.newValue.x, minValue, maxValue);
            int clampY = Mathf.Clamp(evt.newValue.y, minValue, maxValue);
            if (clampX != evt.newValue.x || clampY != evt.newValue.y)
                vectorField.SetValueWithoutNotify(new Vector2Int(clampX, clampY));
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