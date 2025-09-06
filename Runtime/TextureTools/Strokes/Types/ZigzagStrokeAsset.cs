using System;
using UnityEngine;
using SketchRenderer.Runtime.Data;
using Random = UnityEngine.Random;

namespace SketchRenderer.Runtime.TextureTools.Strokes
{
    [CreateAssetMenu(fileName = "ZigzagStrokeAsset", menuName = SketchRendererData.PackageAssetItemPath + "StrokeAssets/Zigzag")]
    public class ZigzagStrokeAsset : StrokeAsset
    {
        public override StrokeSDFType PatternType => StrokeSDFType.ZIGZAG;
        [Range(-1, 1f)]
        public float SubStrokeDirectionOffset = 0.25f;
        public float SubStrokeLengthMultiplier = 1f;
        public bool OnlyMultiplyZigStroke;
        [Range(1, 5)]
        public int Repetitions = 1;
        
        public override StrokeData UpdatedDataByFillRate(float fillRate)
        {
            StrokeData output = new StrokeData()
            {
                OriginPoint = StrokeData.OriginPoint,
                Direction = StrokeData.Direction,
                AdditionalPackedData = StrokeData.AdditionalPackedData,
                Thickness = StrokeData.Thickness,
                ThicknessFalloffConstraint = StrokeData.ThicknessFalloffConstraint,
                Length = StrokeData.Length,
                LengthThicknessFalloff = StrokeData.LengthThicknessFalloff,
                Pressure = StrokeData.Pressure,
                PressureFalloff = StrokeData.PressureFalloff,
                Iterations = StrokeData.Iterations,
            };
            output = PackAdditionalData(output);
            return output;
        }

        public override StrokeData Randomize(float fillRate)
        {
            StrokeData output = new StrokeData()
            {
                OriginPoint = StrokeData.OriginPoint,
                Direction = VariationData.DirectionVariationRange == 0
                    ? StrokeData.Direction
                    : new Vector4(
                        GetRangeConstrainedSmoothRandom(StrokeData.Direction.x, VariationData.DirectionVariationRange, -1,
                            1),
                        GetRangeConstrainedSmoothRandom(StrokeData.Direction.y, VariationData.DirectionVariationRange, -1,
                            1), 0, 0),
                AdditionalPackedData = StrokeData.AdditionalPackedData,
                Thickness = VariationData.ThicknessVariationRange == 0
                    ? StrokeData.Thickness
                    : GetRangeConstrainedSmoothRandom(StrokeData.Thickness, VariationData.ThicknessVariationRange),
                ThicknessFalloffConstraint = StrokeData.ThicknessFalloffConstraint,
                Length = VariationData.LengthVariationRange == 0
                    ? StrokeData.Length
                    : GetRangeConstrainedSmoothRandom(StrokeData.Length, VariationData.LengthVariationRange),
                LengthThicknessFalloff = StrokeData.LengthThicknessFalloff,
                Pressure = VariationData.PressureVariationRange == 0
                    ? StrokeData.Pressure
                    : GetRangeConstrainedSmoothRandom(StrokeData.Pressure, VariationData.PressureVariationRange),
                PressureFalloff = StrokeData.PressureFalloff,
                Iterations = StrokeData.Iterations,
            };
            output.OriginPoint = new Vector4(Random.value, Random.value, 0, 0);
            output = PackAdditionalData(output);
            return output;
        }

        protected override StrokeData PackAdditionalData(StrokeData data)
        {
            Vector4 additonalData = new Vector4
            {
                x = SubStrokeDirectionOffset,
                y = SubStrokeLengthMultiplier,
                z = 0f,
                w = OnlyMultiplyZigStroke ? 1f : SubStrokeLengthMultiplier,
            };
            data.AdditionalPackedData = additonalData;
            data.Iterations = Repetitions;
            return data;
        }
    }
}