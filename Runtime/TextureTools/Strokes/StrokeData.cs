using UnityEngine;

namespace SketchRenderer.Runtime.TextureTools.Strokes
{
    [System.Serializable]
    public struct StrokeData
    {
        public Vector4 OriginPoint;
        public Vector4 Direction;
        [HideInInspector] 
        public Vector4 AdditionalPackedData;
        [Range(0, 1)]
        public float Thickness;
        [Range(0, 1)] 
        public float ThicknessFalloffConstraint;
        [Range(0, 1)]
        public float Length;
        [Range(0, 1)]
        public float LengthThicknessFalloff;
        [Range(0, 1)] 
        public float Pressure;
        [Range(0, 1)] 
        public float PressureFalloff;
        [HideInInspector] 
        public int Iterations;

        public int GetStrideLength()
        {
            //Vector4 + float + float
            return (sizeof(float) * 4) * 3 + sizeof(float) * 6 + sizeof(int);
        }
    }
}