#ifndef SKETCH_NEIGHBORHOOD_SAMPLE
#define SKETCH_NEIGHBORHOOD_SAMPLE

void Get3X3NeighborhoodPositions(float2 uv, int offset, float2 texelSize,
    out float2 upLeft, out float2 upCenter, out float2 upRight,
    out float2 centerLeft, out float2 centerRight,
    out float2 downLeft, out float2 downCenter, out float2 downRight)
{
    upLeft = uv + (float2(-1.0, 1.0) *  (1 + offset)) * texelSize.xy;
    upCenter = uv + (float2(0, 1.0) * (1 + offset)) * texelSize.xy;
    upRight = uv + (float2(1.0, 1.0) * (1 + offset)) * texelSize.xy;
    centerLeft = uv + (float2(-1.0, 0) * (1 + offset)) * texelSize.xy;
    centerRight = uv +(float2(1.0, 0) * (1 + offset)) * texelSize.xy;
    downLeft = uv +  (float2(-1.0, -1.0) * (1 + offset)) * texelSize.xy;
    downCenter = uv + (float2(0, -1.0) * (1 + offset)) * texelSize.xy;
    downRight = uv + (float2(1.0, -1.0) * (1 + offset)) * texelSize.xy;
}

#endif