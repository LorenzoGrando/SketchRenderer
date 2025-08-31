#ifndef FALLOFF_FUNCTIONS
#define FALLOFF_FUNCTIONS

#include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/MathUtils.hlsl"

float LinearFalloff(float t)
{
    return t;
}

float EaseInOutSineFalloff(float t)
{
    return -(cos(PI * t) - 1)/ 2;
}

float EaseOutElasticFalloff(float t)
{
    const float c4 = (2*PI)/3;
    
    return t == 0 ? 0
        : (t == 1 ? 1 : pow(2.0, -10.0 * t) * sin((t * 10.0 - 0.75) * c4) + 1);
}

float FalloffFunction(float t)
{
    #if defined(FALLOFF_LINEAR)
    t = LinearFalloff(t);
    #elif defined(FALLOFF_EASE_INOUT_SINE)
    t = EaseInOutSineFalloff(t);
    #elif defined(FALLOFF_EASE_OUT_ELASTIC)
    t = EaseOutElasticFalloff(t) * 0.5;
    #endif
    
    return t;
}

#endif