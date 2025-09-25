using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    public static class UIToolkitExtensions
    {
        public static void ToggleElementInteractions(this Foldout foldout, bool enable)
        {
            var contentElement = foldout.Q<VisualElement>(className: Foldout.contentUssClassName);
            foreach(var child in contentElement.Children())
                child.SetEnabled(enable);
        }
    }
}