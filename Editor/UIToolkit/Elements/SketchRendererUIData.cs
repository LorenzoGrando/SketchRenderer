using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.UIToolkit
{
    internal struct CornerData
    {
        internal float top, bottom, right, left;
        
        internal CornerData(float left, float top, float right, float bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
        internal static readonly CornerData Empty = new CornerData(0f, 0f, 0f, 0f);

        public static CornerData operator +(CornerData left, CornerData right)
        {
            return new CornerData(left.left + right.left, left.top + right.top, left.right + right.right, left.bottom + right.bottom);
        }

        public static CornerData operator +(CornerData left, float scalar)
        {
            return new CornerData(left.left + scalar, left.top + scalar, left.right + scalar, left.bottom + scalar);
        }

        public static CornerData operator *(CornerData left, float scalar)
        {
            return new CornerData(left.left * scalar, left.top * scalar, left.right * scalar, left.bottom * scalar);
        }
        
        public static CornerData operator *(CornerData left, CornerData right)
        {
            return new CornerData(left.left * right.left, left.top * right.top, left.right * right.right, left.bottom * right.bottom);
        }
    }
    
    internal static class SketchRendererUIData
    {
        //Command Events
        public static readonly string RepaintEditorCommand = "RepaintSketchEditor";
        
        //- Generic
        internal static readonly float MajorIndentValue = 15f;
        internal static readonly float RegularIndentValue = 10f;
        internal static readonly float MinorIndentValue = 5f;
        internal static readonly CornerData BaseFieldNoVerticalMargins = new CornerData(0f, 0f, MinorIndentValue, 0f);
        internal static readonly CornerData BaseFieldMargins = new CornerData(0f, 0f, MinorIndentValue, RegularIndentValue);
        internal static readonly CornerData MinorFieldMargins = new CornerData(0f, 0f, 0f, MinorIndentValue);
        internal static readonly CornerData MajorIndentCorners = new CornerData(MajorIndentValue, 0f, 0, 0f);
        internal static readonly CornerData RegularIndentCorners = new CornerData(RegularIndentValue, 0f, 0, 0f);
        internal static readonly CornerData MinorIndentCorners = new CornerData(MinorIndentValue, 0f, 0, 0f);
        internal static readonly CornerData TitleIndent = new CornerData(MinorIndentValue, MinorIndentValue, 0, MinorIndentValue);
        internal static Color BorderColor
        {
            get { return EditorGUIUtility.isProSkin ? DarkBorderColor : LightBorderColor; }
        }
        internal static readonly float BorderWidth = 0.5f;
        internal static readonly float MajorBorderWidth = 2f;
        
        // Generic Labels
        internal static readonly float MajorTitleHeight = 20f;

        // -- Button
        internal static readonly float MajorButtonHeight = 30f;
        
        // -- Expandable
        private static readonly Color LightHeaderExpandableBackgroundColor = new Color(0.8f, 0.8f, 0.8f);
        private static readonly Color DarkHeaderExpandableBackgroundColor = new Color(0.196f, 0.196f, 0.196f);
        internal static Color ExpandableHeaderBackgroundColor
        {
            get { return EditorGUIUtility.isProSkin ? DarkHeaderExpandableBackgroundColor : LightHeaderExpandableBackgroundColor; }
        }
        internal static readonly float ExpandableHeaderHeightModifier = 1.2f;
        
        private static readonly Color LightBorderColor = new Color(0.8f, 0.8f, 0.8f);
        private static readonly Color DarkBorderColor = new Color(0.102f, 0.102f, 0.102f);
        
        // -- Labeled Property
        internal static readonly float MinLabelWidth = 40f;
        internal static readonly float MaxLabelWidth = 160f;
        internal static readonly float LabeledInputAreaMinWidth = 125f;
        internal static readonly float LabeledInputAreaMaxWidth = 225f;
        
        // -- Slider Specific
        internal static readonly float LabeledSliderFixedInputWidth = 50f;
        internal static CornerData LabeledSliderFixedInputMargin = new CornerData(2f, 0f, 0f, 0f);
        internal static readonly float LabeledInputSliderMinWidth = LabeledInputAreaMinWidth - LabeledSliderFixedInputWidth - LabeledSliderFixedInputMargin.left - LabeledSliderFixedInputMargin.right;
        internal static readonly float LabeledInputSliderMaxWidth = LabeledInputAreaMaxWidth - LabeledSliderFixedInputWidth - LabeledSliderFixedInputMargin.left - LabeledSliderFixedInputMargin.right;
        
    }
}