#pragma once

#include "Packages/com.lorenzogrando.sketchrenderer/Shader/Outlining/EdgeDetection/SobelInclude.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/LuminanceSample.hlsl"

float SobelColorHorizontal3x3(float3x3 kernel, float2 c, float2 uL, float2 cL, float2 dL, float2 uR, float2 cR, float2 dR)
{
    float vUL = SamplePerceivedLuminance(SampleSceneColor(uL)) * kernel._11;
    float vCL = SamplePerceivedLuminance(SampleSceneColor(cL)) * kernel._21;
    float vDL = SamplePerceivedLuminance(SampleSceneColor(dL)) * kernel._31;
    float vUR = SamplePerceivedLuminance(SampleSceneColor(uR)) * kernel._13;
    float vCR = SamplePerceivedLuminance(SampleSceneColor(cR)) * kernel._23;
    float vDR = SamplePerceivedLuminance(SampleSceneColor(dR)) * kernel._33;

    return vUL + vCL + vDL + vUR + vCR + vDR;
    
    return clamp((vUL + vCL + vDL + vUR + vCR + vDR), -1, 1);
}

float SobelColorVertical3x3(float3x3 kernel, float2 c, float2 uL, float2 uC, float2 uR, float2 dL, float2 dC, float2 dR)
{
    float vUL = SamplePerceivedLuminance(SampleSceneColor(uL)) * kernel._11;
    float vUC = SamplePerceivedLuminance(SampleSceneColor(uC)) * kernel._12;
    float vUR = SamplePerceivedLuminance(SampleSceneColor(uR)) * kernel._13;
    float vDL = SamplePerceivedLuminance(SampleSceneColor(dL)) * kernel._31;
    float vDC = SamplePerceivedLuminance(SampleSceneColor(dC)) * kernel._32;
    float vDR = SamplePerceivedLuminance(SampleSceneColor(dR)) * kernel._33;

    return vUL + vUC + vUR + vDL + vDC + vDR;

    return clamp((vUL + vUC + vUR + vDL + vDC + vDR), -1, 1);
}

float SobelColor1X3(float3 kernel, float2 uv0, float2 uv1, float2 uv2)
{
    float v0 = SamplePerceivedLuminance(SampleSceneColor(uv0)) * kernel.r;
    float v1 = SamplePerceivedLuminance(SampleSceneColor(uv1)) * kernel.g;
    float v2 = SamplePerceivedLuminance(SampleSceneColor(uv2)) * kernel.b;

    return v0 + v1 + v2;

    return clamp((v0 + v1 + v2), -1, 1);
}