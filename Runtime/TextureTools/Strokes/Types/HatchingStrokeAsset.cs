using UnityEngine;
using SketchRenderer.Runtime.Data;

namespace SketchRenderer.Runtime.TextureTools.Strokes
{
    [CreateAssetMenu(fileName = "HatchingStrokeAsset",
        menuName = SketchRendererData.PackageAssetItemPath + "StrokeAssets/Hatching")]
    public class HatchingTAMStrokeAsset : StrokeAsset
    {
        public override StrokeSDFType PatternType => StrokeSDFType.HATCHING;

        [Space(5)] [Header("Hatching Specific")] [Range(0, 1)]
        public float MinCrossHatchingThreshold;

        [Range(0, 1)] public float MaxCrossHatchingThreshold;

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
            if (ShouldCrossHatch(fillRate))
                output.Direction = GetPerpendicularCrosshatch(output.Direction);
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
                        GetRangeConstrainedSmoothRandom(StrokeData.Direction.x, VariationData.DirectionVariationRange,
                            -1, 1),
                        GetRangeConstrainedSmoothRandom(StrokeData.Direction.y, VariationData.DirectionVariationRange,
                            -1, 1), 0, 0),
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
                Iterations = StrokeData.Iterations
            };
            if (ShouldCrossHatch(fillRate))
                output.Direction = GetPerpendicularCrosshatch(output.Direction);
            output.OriginPoint = new Vector4(Random.value, Random.value, 0, 0);
            output = PackAdditionalData(output);
            return output;
        }

        private bool ShouldCrossHatch(float fillRate)
        {
            if (fillRate > MinCrossHatchingThreshold && fillRate < MaxCrossHatchingThreshold)
                return true;
            else if (fillRate > MaxCrossHatchingThreshold)
                return Random.value > 0.5f;
            else
                return false;
        }

        private Vector4 GetPerpendicularCrosshatch(Vector4 Direction)
        {
            Vector3 Perpendicular = Vector3.Cross(new Vector3(Direction.x, Direction.y, Direction.z), Vector3.forward);
            return new Vector4(Perpendicular.x, Perpendicular.y, 0, 0);
        }
    }
}