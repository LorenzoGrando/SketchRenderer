#pragma once

#ifndef PI
#define PI 3.14159265
#endif
#ifndef TAU
#define TAU 6.28318531
#endif

float invLerp(float from, float to, float value){
    return (value - from) / (to - from);
}

uint2 bitwiseNot(uint2 v)
{
    return uint2(~v.x, ~v.y);
}

uint3 bitwiseNot(uint3 v)
{
    return uint3(~v.x, ~v.y, ~v.z);
}

uint4 bitwiseNot(uint4 v)
{
    return uint4(~v.x, ~v.y, ~v.z, ~v.w);
}

float RangeConstrainedSmoothRandom(float current, float range, float rand, float minValue, float maxValue)
{
    float clampedOrigin = invLerp(minValue, maxValue, current);
    float smoothFactor = 1.0 - abs(clampedOrigin - 0.5f) * 2.0;
    float smoothRange = range - ((range/2.0) * smoothFactor);
    float minStep = min(1.0, max(0.0, clampedOrigin - smoothRange));
    float maxStep = max(0.0, min(1.0, clampedOrigin + smoothRange));
    float t = lerp(minStep, maxStep, saturate(rand));
    return lerp(minValue, maxValue, t);
}