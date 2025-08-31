#pragma once

//Matrices in HLSL are columns based, so this visual below is the transposed kernel
static float3x3 BaseSobel3X3HorizontalKernel = {
    float3(-1, -2, -1),
    float3(0, 0, 0),
    float3(1, 2, 1)
};

static float3x3 BaseSobel3X3VerticalKernel = {
    float3(-1, 0, 1),
    float3(-2, 0, 2),
    float3(-1, 0, 1)
};

static float3x3 ModifiedSobel3X3HorizontalKernel = {
    float3(-3, -10, -3),
    float3(0, 0, 0),
    float3(3, 10, 3)
};

static float3x3 ModifiedSobel3X3VerticalKernel = {
    float3(-3, 0, 3),
    float3(-10, 0, 10),
    float3(-3, 0, 3)
};

static float3 Sobel1X3Kernel = {
    1, 0, -1
};