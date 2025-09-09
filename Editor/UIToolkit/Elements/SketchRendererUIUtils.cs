using UnityEngine.UIElements;

namespace SketchRenderer.Editor.UIToolkit
{
    internal static class SketchRendererUIUtils
    {
        internal static void AddWithMargins(VisualElement container, VisualElement element, CornerData margins)
        { 
            ApplyToMargins(element, margins);
            container.Add(element);
        }
        
        internal static void ApplyToMargins(VisualElement element, CornerData cornerData)
        {
            element.style.marginLeft = cornerData.left;
            element.style.marginTop = cornerData.top;
            element.style.marginRight = cornerData.right;
            element.style.marginBottom = cornerData.bottom;
        }

        internal static void ApplyToPadding(VisualElement element, CornerData cornerData)
        {
            element.style.paddingLeft = cornerData.left;
            element.style.paddingTop = cornerData.top;
            element.style.paddingRight = cornerData.right;
            element.style.paddingBottom = cornerData.bottom;
        }
    }
}