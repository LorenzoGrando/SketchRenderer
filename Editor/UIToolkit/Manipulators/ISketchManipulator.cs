using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    internal interface ISketchManipulator<T> where T : BindableElement
    {
        public void Initialize(T field);
        
        public Manipulator GetBaseManipulator();
    }
}