using UnityEngine;
using SketchRenderer.Runtime.Data;
using Random = UnityEngine.Random;

namespace SketchRenderer.Runtime.TextureTools.Strokes
{
    [CreateAssetMenu(fileName = "FeatheringStrokeAsset", menuName = SketchRendererData.PackageAssetItemPath + "StrokeAssets/Feathering")]
    public class FeatheringStrokeAsset : StrokeAsset
    {
        public override StrokeSDFType PatternType => StrokeSDFType.FEATHERING;
        [Range(-1, 1f)]
        public float FirstSubStrokeDirectionOffset = 0.25f;
        public float FirstSubStrokeLengthMultiplier = 1f;
        [Range(-1, 1f)]
        public float SecondSubStrokeDirectionOffset = 0.25f;
        public float SecondSubStrokeLengthMultiplier = 1f;
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
                x = FirstSubStrokeDirectionOffset,
                y = FirstSubStrokeLengthMultiplier,
                z = SecondSubStrokeDirectionOffset,
                w = SecondSubStrokeLengthMultiplier
            };
            data.AdditionalPackedData = additonalData;
            data.Iterations = Repetitions;
            return data;
        }
        
        public void CopyFrom(FeatheringStrokeAsset asset)
        {
            if(asset == null)
                return;

            StrokeData = asset.StrokeData;
            SelectedFalloffFunction = asset.SelectedFalloffFunction;
            VariationData = asset.VariationData;
            FirstSubStrokeDirectionOffset = asset.FirstSubStrokeDirectionOffset;
            SecondSubStrokeDirectionOffset = asset.SecondSubStrokeDirectionOffset;
            FirstSubStrokeLengthMultiplier = asset.FirstSubStrokeLengthMultiplier;
            SecondSubStrokeLengthMultiplier = asset.SecondSubStrokeLengthMultiplier;
            Repetitions = asset.Repetitions;
        }
    }
}