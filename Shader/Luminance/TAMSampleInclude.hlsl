#ifndef TAM_SAMPLING
#define TAM_SAMPLING

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/TextureOperations.hlsl"

float2 _TamScales;
TEXTURE2D(_Tam0_2);
float4 _Tam0_2_TexelSize;
TEXTURE2D(_Tam3_5);
float4 _Tam3_5_TexelSize;
TEXTURE2D(_Tam6_8);
float4 _Tam6_8_TexelSize;


//The only reason we split into preprocessor directives here is to prevent extra texture sampling when we dont care about the result in those TAMs
//Even if our total tones amount does not equal the full range of channels in a tam (say, only 2 textures)
//Since the luminance only takes into account the total expected tones, those always return 0 weight when clamped

float3 GetWeightsFromQuantizedLuminance(float luminance, int tones, int offset)
{
    luminance *= tones;
    luminance = (float)tones - luminance;
    float3 luminance3 = float3(luminance.rrr);
    float3 weights = float3(0 + offset, 1 + offset, 2 + offset);
    return saturate(luminance3 - weights);
}

float SingleTAMSample(float luminance, int tones, float2 uv, float mip)
{
    float4 tam0_2 = SAMPLE_TEX(_Tam0_2, sampler_PointRepeat, _Tam0_2_TexelSize.z, _BlitMipLevel, uv, _TamScales);
    float3 toneWeights = GetWeightsFromQuantizedLuminance(luminance, tones, 0);
    toneWeights.xy -= toneWeights.yz;
    float3 col = tam0_2.rgb * toneWeights;
    return 1 - (col.r + col.g + col.b);
}

float DoubleTAMSample(float luminance, int tones, float2 uv, float mip)
{
    float4 tam0_2 = SAMPLE_TEX(_Tam0_2, sampler_PointRepeat, _Tam0_2_TexelSize.z, _BlitMipLevel, uv, _TamScales);
    float4 tam3_5 = SAMPLE_TEX(_Tam3_5, sampler_PointRepeat, _Tam3_5_TexelSize.z, _BlitMipLevel, uv, _TamScales);
    float3 toneWeights1 = GetWeightsFromQuantizedLuminance(luminance, tones, 0);
    float3 toneWeights2 = GetWeightsFromQuantizedLuminance(luminance, tones, 3);
    toneWeights1.xy -= toneWeights1.yz;
    toneWeights1.z -= toneWeights2.x;
    toneWeights2.xy -= toneWeights2.yz;
    float3 col1 = tam0_2.rgb * toneWeights1;
    float3 col2 = tam3_5.rgb * toneWeights2;
    return 1 - (col1.r + col1.g + col1.b + col2.r + col2.g + col2.b);
}

float TripleTAMSample(float luminance, int tones, float2 uv, float mip)
{
    float4 tam0_2 = SAMPLE_TEX(_Tam0_2, sampler_PointRepeat, _Tam0_2_TexelSize.z, _BlitMipLevel, uv, _TamScales);
    float4 tam3_5 = SAMPLE_TEX(_Tam3_5, sampler_PointRepeat, _Tam3_5_TexelSize.z, _BlitMipLevel, uv, _TamScales);
    float4 tam6_8 = SAMPLE_TEX(_Tam6_8, sampler_PointRepeat, _Tam6_8_TexelSize.z, _BlitMipLevel, uv, _TamScales);
    float3 toneWeights1 = GetWeightsFromQuantizedLuminance(luminance, tones, 0);
    float3 toneWeights2 = GetWeightsFromQuantizedLuminance(luminance, tones, 3);
    float3 toneWeights3 = GetWeightsFromQuantizedLuminance(luminance, tones, 6);
    toneWeights1.xy -= toneWeights1.yz;
    toneWeights1.z -= toneWeights2.x;
    toneWeights2.xy -= toneWeights2.yz;
    toneWeights2.z -= toneWeights3.x;
    toneWeights3.xy -= toneWeights3.yz;
    float3 col1 = tam0_2.rgb * toneWeights1;
    float3 col2 = tam3_5.rgb * toneWeights2;
    float3 col3 = tam6_8.rgb * toneWeights3;
    return 1 - (col1.r + col1.g + col1.b + col2.r + col2.g + col2.b + col3.r + col3.g + col3.b);
}

float SampleTAM(float luminance, int tones, float2 uv, float mip)
{
    #if defined TAM_DOUBLE
    return DoubleTAMSample(luminance, tones, uv, mip);
    #elif defined TAM_TRIPLE
    return TripleTAMSample(luminance, tones, uv, mip);
    #endif

    return SingleTAMSample(luminance, tones, uv, mip);
}

#endif