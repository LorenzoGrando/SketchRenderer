#pragma once

#include "Packages/com.lorenzogrando.sketchrenderer/Shader/Outlining/EdgeDetection/SobelInclude.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

float SobelColorHorizontal3x3(float3x3 kernel, float2 c, float2 uL, float2 cL, float2 dL, float2 uR, float2 cR, float2 dR)
{
    float3 vUL = SampleSceneColor(uL) * kernel._11;
    float3 vCL = SampleSceneColor(cL) * kernel._21;
    float3 vDL = SampleSceneColor(dL) * kernel._31;
    float3 vUR = SampleSceneColor(uR) * kernel._13;
    float3 vCR = SampleSceneColor(cR) * kernel._23;
    float3 vDR = SampleSceneColor(dR) * kernel._33;

    float x = vUL.x + vCL.x + vDL.x + vUR.x + vCR.x + vDR.x;
    float y = vUL.y + vCL.y + vDL.y + vUR.y + vCR.y + vDR.y;
    float z = vUL.z + vCL.z + vDL.z + vUR.z + vCR.z + vDR.z;

    float n = max(x, max(y, z))/6.0;
    
    return clamp(n, -1, 1);
}

float SobelColorVertical3x3(float3x3 kernel, float2 c, float2 uL, float2 uC, float2 uR, float2 dL, float2 dC, float2 dR)
{
    float3 vUL = SampleSceneColor(uL) * kernel._11;
    float3 vUC = SampleSceneColor(uC) * kernel._12;
    float3 vUR = SampleSceneColor(uR) * kernel._13;
    float3 vDL = SampleSceneColor(dL) * kernel._31;
    float3 vDC = SampleSceneColor(dC) * kernel._32;
    float3 vDR = SampleSceneColor(dR) * kernel._33;
    

    float x = vUL.x + vUC.x + vDL.x + vUR.x + vDC.x + vDR.x;
    float y = vUL.y + vUC.y + vDL.y + vUR.y + vDC.y + vDR.y;
    float z = vUL.z + vUC.z + vDL.z + vUR.z + vDC.z + vDR.z;

    float n = max(x, max(y, z))/6.0;
    
    return clamp(n, -1, 1);
}

float SobelColor1X3(float3 kernel, float2 uv0, float2 uv1, float2 uv2)
{
    float3 v0 = SampleSceneColor(uv0) * kernel.r;
    //float3 v1 = SampleSceneColor(uv1);
    float3 v2 = SampleSceneColor(uv2) * kernel.b;

    float x = v0.x + v2.x;
    float y = v0.y + v2.y;
    float z = v0.z + v2.z;

    float n = max(x, max(y, z))/2.0;

    return clamp(n, -1, 1);
}