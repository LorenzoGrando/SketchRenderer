#pragma once

#include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/MathUtils.hlsl"

//----------------------------------------------------------------------------------------
// Hashes found in: https://www.shadertoy.com/view/4djSRW

//  1 out, 1 in...
float hash11(float p)
{
    p = frac(p * .1031);
    p *= p + 33.33;
    p *= p + p;
    return frac(p);
}

//  1 out, 2 in...
float hash12(float2 p)
{
	float3 p3  = frac(float3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

//  1 out, 3 in...
float hash13(float3 p3)
{
	p3  = frac(p3 * .1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return frac((p3.x + p3.y) * p3.z);
}
// 1 out 4 in...
float hash14(float4 p4)
{
	p4 = frac(p4  * float4(.1031, .1030, .0973, .1099));
    p4 += dot(p4, p4.wzxy+33.33);
    return frac((p4.x + p4.y) * (p4.z + p4.w));
}

//  2 out, 1 in...
float2 hash21(float p)
{
	float3 p3 = frac(float3(p.xxx) * float3(.1031, .1030, .0973));
	p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.xx+p3.yz)*p3.zy);
}

///  2 out, 2 in...
float2 hash22(float2 p)
{
	float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yzx+33.33);
    return frac((p3.xx+p3.yz)*p3.zy);
}

///  2 out, 3 in...
float2 hash23(float3 p3)
{
	p3 = frac(p3 * float3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yzx+33.33);
    return frac((p3.xx+p3.yz)*p3.zy);
}

//  3 out, 1 in...
float3 hash31(float p)
{
   float3 p3 = frac(float3(p.xxx) * float3(.1031, .1030, .0973));
   p3 += dot(p3, p3.yzx+33.33);
   return frac((p3.xxy+p3.yzz)*p3.zyx); 
}


///  3 out, 2 in...
float3 hash32(float2 p)
{
	float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yxz+33.33);
    return frac((p3.xxy+p3.yzz)*p3.zyx);
}

///  3 out, 3 in...
float3 hash33(float3 p3)
{
	p3 = frac(p3 * float3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yxz+33.33);
    return frac((p3.xxy + p3.yxx)*p3.zyx);
}

// 4 out, 1 in...
float4 hash41(float p)
{
	float4 p4 = frac(float4(p.xxxx) * float4(.1031, .1030, .0973, .1099));
    p4 += dot(p4, p4.wzxy+33.33);
    return frac((p4.xxyz+p4.yzzw)*p4.zywx);
}

// 4 out, 2 in...
float4 hash42(float2 p)
{
	float4 p4 = frac(float4(p.xyxy) * float4(.1031, .1030, .0973, .1099));
    p4 += dot(p4, p4.wzxy+33.33);
    return frac((p4.xxyz+p4.yzzw)*p4.zywx);
}

// 4 out, 3 in...
float4 hash43(float3 p)
{
	float4 p4 = frac(float4(p.xyzx)  * float4(.1031, .1030, .0973, .1099));
    p4 += dot(p4, p4.wzxy+33.33);
    return frac((p4.xxyz+p4.yzzw)*p4.zywx);
}

// 4 out, 4 in...
float4 hash44(float4 p4)
{
	p4 = frac(p4  * float4(.1031, .1030, .0973, .1099));
    p4 += dot(p4, p4.wzxy+33.33);
    return frac((p4.xxyz+p4.yzzw)*p4.zywx);
}


// ########## Hashing from tuxalin ###########
// Adapted from this amazing repo: https://github.com/tuxalin/procedural-tileable-shaders/blob/master/multiHash.glsl

uint ihash1D(uint q)
{
    // hash by Hugo Elias, Integer Hash - I, 2017
    q = q * 747796405u + 2891336453u;
    q = (q << 13u) ^ q;
    return q * (q * q * 15731u + 789221u) + 1376312589u;
}

uint2 ihash1D(uint2 q)
{
    // hash by Hugo Elias, Integer Hash - I, 2017
    q = q * 747796405u + 2891336453u;
    q = (q << 13u) ^ q;
    return q * (q * q * 15731u + 789221u) + 1376312589u;
}

uint4 ihash1D(uint4 q)
{
    // hash by Hugo Elias, Integer Hash - I, 2017
    q = q * 747796405u + 2891336453u;
    q = (q << 13u) ^ q;
    return q * (q * q * 15731u + 789221u) + 1376312589u;
}

// generates 2 random numbers for the coordinate
float2 betterHash2D(float2 x)
{
    uint2 q = uint2(x);
    uint h0 = ihash1D(ihash1D(q.x) + q.y);
    uint h1 = h0 * 1933247u + ~h0 ^ 230123u;
    return float2(h0, h1)  * (1.0 / float(0xffffffffu));
}

// generates a random number for each of the 4 cell corners
float4 betterHash2D(float4 cell)    
{
    uint4 i = uint4(cell);
    uint4 hash = ihash1D(ihash1D(i.xzxz) + i.yyww);
    return float4(hash) * (1.0 / float(0xffffffffu));
}

// generates 2 random numbers for each of the 4 cell corners
void betterHash2D(float4 cell, out float4 hashX, out float4 hashY)
{
    uint4 i = uint4(cell);
    uint4 hash0 = ihash1D(ihash1D(i.xzxz) + i.yyww);
    uint4 hash1 = ihash1D(hash0 ^ 1933247u);
    hashX = float4(hash0) * (1.0 / float(0xffffffffu));
    hashY = float4(hash1) * (1.0 / float(0xffffffffu));
}

// generates 2 random numbers for each of the 2D coordinates
float4 betterHash2D(float2 coords0, float2 coords1)
{
    uint4 i = uint4(coords0, coords1);
    uint4 hash = ihash1D(ihash1D(i.xz) + i.yw).xxyy;
    hash.yw = hash.yw * 1933247u + bitwiseNot(hash.yw) ^ 230123u;
    return float4(hash) * (1.0 / float(0xffffffffu));;
}

// generates 2 random numbers for each of the four 2D coordinates
void betterHash2D(float4 coords0, float4 coords1, out float4 hashX, out float4 hashY)
{
    uint4 hash0 = ihash1D(ihash1D(uint4(coords0.xz, coords1.xz)) + uint4(coords0.yw, coords1.yw));
    uint4 hash1 = hash0 * 1933247u + (bitwiseNot(hash0) ^ 230123u);
    hashX = float4(hash0) * (1.0 / float(0xffffffffu));
    hashY = float4(hash1) * (1.0 / float(0xffffffffu));
}