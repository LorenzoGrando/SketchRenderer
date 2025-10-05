#pragma once

float SampleSimpleLuminance(float3 col)
{
    return  (col.r * 2 + col.b + + col.g * 3)/6.0;
}

float SamplePerceivedLuminance(float3 col)
{
    return dot(col.rgb, float3(0.299, 0.587, 0.114));
}