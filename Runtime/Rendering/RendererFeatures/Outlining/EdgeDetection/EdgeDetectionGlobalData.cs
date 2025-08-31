namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public static class EdgeDetectionGlobalData
    {
        public enum EdgeDetectionSource
        {
            COLOR, DEPTH, DEPTH_NORMALS, ALL
        }
    
        public static readonly string DEPTH_KEYWORD = "SOURCE_DEPTH";
        public static readonly string DEPTH_NORMALS_KEYWORD = "SOURCE_DEPTH_NORMALS";
    
        public enum EdgeDetectionMethod
        {
            SOBEL_3X3, SOBEL_1X3
        }
    
        public static readonly string SOBEL_3X3_KEYWORD = "SOBEL_KERNEL_3X3";
        public static readonly string SOBEL_1X3_KEYWORD = "SOBEL_KERNEL_1X3";

        public enum EdgeDetectionOutputType
        {
            OUTPUT_GREYSCALE, OUTPUT_DIRECTION_DATA_ANGLE, OUTPUT_DIRECTION_DATA_VECTOR
        }
    
        public static readonly string OUTPUT_GREYSCALE_KEYWORD = "OUTPUT_GREYSCALE";
        public static readonly string OUPUT_DIRECTION_ANGLE_KEYWORD = "OUTPUT_DIRECTION_DATA_ANGLE";
        public static readonly string OUPUT_DIRECTION_VECTOR_KEYWORD = "OUTPUT_DIRECTION_DATA_VECTOR";
    }
}