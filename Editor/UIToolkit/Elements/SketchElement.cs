using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    public class SketchElement<T> where T : BindableElement
    {
        public VisualElement Container;
        public Label Label;
        public T Field;
    }
}