using UnityEngine;


namespace SketchRenderer.Runtime.TextureTools.Strokes
{
    [System.Serializable]
    public struct TAMVariationData
    {
        [Range(0f, 1f)]
        public float DirectionVariationRange;
        [Range(0f, 1f)]
        public float ThicknessVariationRange;
        [Range(0f, 1f)]
        public float LengthVariationRange;
        [Range(0f, 1f)]
        public float PressureVariationRange;
    
        public int GetStrideLength()
        {
            //Vector4 + float + float
            return sizeof(float) * 4;
        }
    }
}