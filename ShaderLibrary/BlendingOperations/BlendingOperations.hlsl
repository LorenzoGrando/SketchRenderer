#pragma once

#define BLEND_0P_MULTIPLY(a, b) ((a) * (b))
#define BLEND_0P_SCREEN(a, b) (1.0 - ((1.0 - (a))*(1.0 - (b))))
#define BLEND_0P_ADD(a, b) ((a) + (b))
#define BLEND_0P_SUBTRACT(a, b) ((a) - (b))

#ifdef BLEND_MULTIPLY
    #define BLENDING_OPERATION(a, b) BLEND_0P_MULTIPLY(a, b)
#elif defined BLEND_SCREEN
    #define BLENDING_OPERATION(a, b) BLEND_0P_SCREEN(a, b)
#elif defined BLEND_ADD
    #define BLENDING_OPERATION(a, b) BLEND_0P_ADD(a, b)
#elif defined BLEND_SUBTRACT
    #define BLENDING_OPERATION(a, b) BLEND_0P_SUBTRACT(a, b)
#else
    #define BLENDING_OPERATION(a, b) BLEND_0P_MULTIPLY(a, b)
#endif


inline float BlendMultiply(float a, float b)
{
    return a * b;
}

inline float4 BlendMultiply(float4 a, float4 b)
{
    return a * b;
}

inline float BlendScreen(float a, float b)
{
    return 1.0 - ((1.0 - a) * (1.0 - b));
}

inline float4 BlendScreen(float4 a, float4 b)
{
    return (float4)1.0 - (((float4)1.0 - a) * ((float4)1.0 - b));
}

inline float BlendAdditive(float a, float b)
{
    return a + b;
}

inline float4 BlendAdditive(float4 a, float4 b)
{
    return a + b;
}

inline float BlendSubtractive(float a, float b)
{
    return a - b;
}

inline float4 BlendSubtractive(float4 a, float4 b)
{
    return a - b;
}