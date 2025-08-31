#pragma once

#include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/HashingFunctions.hlsl"

inline float2 VoronoiRandomVector (float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
}

float Voronoi(float2 UV, float AngleOffset, float CellDensity)
{
    float Out;
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float3 res = float3(8.0, 0.0, 0.0);

    for(int y=-1; y<=1; y++)
    {
        for(int x=-1; x<=1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = VoronoiRandomVector(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);
            if(d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
            }
        }
    }

    return Out;
}

float Voronoi_Chebyshev(float2 UV, float AngleOffset, float CellDensity)
{
    float Out;
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float3 res = float3(8.0, 0.0, 0.0);

    for(int y=-1; y<=1; y++)
    {
        for(int x=-1; x<=1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = VoronoiRandomVector(lattice + g, AngleOffset);
            float2 r = lattice - f + offset;
            float d = max(abs(r.x), abs(r.y));
            if(d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
            }
        }
    }

    return Out;
}

// ############# Seamless Noise #############
// These were all adapted from the excellent repo by tuxalin:
// https://github.com/tuxalin/procedural-tileable-shaders/tree/master

// glsl style mod, as pointed by bgolus: https://discussions.unity.com/t/translating-a-glsl-shader-noise-algorithm-to-hlsl-cg/672575/3
#define mod(x, y) (x - y * floor(x / y))


float3 perlinNoiseDirTileable(float2 pos, float2 scale, float seed)
{
    // based on Modifications to Classic Perlin Noise by Brian Sharpe: https://archive.is/cJtlS
    pos *= scale;
    float4 i = floor(pos).xyxy + float2(0.0, 1.0).xxyy;
    float4 f = (pos.xyxy - i.xyxy) - float2(0.0, 1.0).xxyy;
    i = mod(i, scale.xyxy) + seed;

    // grid gradients
    float4 gradientX, gradientY;
    betterHash2D(i, gradientX, gradientY);
    gradientX -= 0.49999;
    gradientY -= 0.49999;

    // perlin surflet
    float4 gradients = rsqrt(gradientX * gradientX + gradientY * gradientY) * (gradientX * f.xzxz + gradientY * f.yyww);
    float4 m = f * f;
    m = m.xzxz + m.yyww;
    m = max(1.0 - m, 0.0);
    float4 m2 = m * m;
    float4 m3 = m * m2;
    // compute the derivatives
    float4 m2Gradients = -6.0 * m2 * gradients;
    float2 grad = float2(dot(m2Gradients, f.xzxz), dot(m2Gradients, f.yyww)) + float2(dot(m3, gradientX), dot(m3, gradientY));
    // sum the surflets and normalize: 1.0 / 0.75^3
    return float3(dot(m3, gradients), grad) * 2.3703703703703703703703703703704;
}

float3 cellularNoiseDirTileable(float2 pos, float2 scale, float jitter, float seed) 
{       
    pos *= scale;
    float2 i = floor(pos);
    float2 f = pos - i;
    
    const float3 offset = float3(-1.0, 0.0, 1.0);
    float4 cells = (i.xyxy + offset.xxzz) % scale.xyxy + seed;
    i = i % scale + seed;
    float4 dx0, dy0, dx1, dy1;
    betterHash2D(float4(cells.xy, float2(i.x, cells.y)), float4(cells.zyx, i.y), dx0, dy0);
    betterHash2D(float4(cells.zwz, i.y), float4(cells.xw, float2(i.x, cells.w)), dx1, dy1);
    
    dx0 = offset.xyzx + dx0 * jitter - f.xxxx; // -1 0 1 -1
    dy0 = offset.xxxy + dy0 * jitter - f.yyyy; // -1 -1 -1 0
    dx1 = offset.zzxy + dx1 * jitter - f.xxxx; // 1 1 -1 0
    dy1 = offset.zyzz + dy1 * jitter - f.yyyy; // 1 0 1 1
    float4 d0 = dx0 * dx0 + dy0 * dy0; 
    float4 d1 = dx1 * dx1 + dy1 * dy1;
    
    float2 centerPos = betterHash2D(i) * jitter - f; // 0 0
    float dCenter = dot(centerPos, centerPos);
    float4 d = min(d0, d1);
    float4 less = step(d1, d0);
    float4 dx = lerp(dx0, dx1, less);
    float4 dy = lerp(dy0, dy1, less);

    float3 t1 = d.x < d.y ? float3(d.x, dx.x, dy.x) : float3(d.y, dx.y, dy.y);
    float3 t2 = d.z < d.w ? float3(d.z, dx.z, dy.z) : float3(d.w, dx.w, dy.w);
    t2 = t2.x < dCenter ? t2 : float3(dCenter, centerPos);
    float3 t = t1.x < t2.x ? t1 : t2;
    t.x = sqrt(t.x);
    // normalize: 0.75^2 * 2.0  == 1.125
    return  t * float3(1.0, -2.0, -2.0) * (1.0 / 1.125);
}