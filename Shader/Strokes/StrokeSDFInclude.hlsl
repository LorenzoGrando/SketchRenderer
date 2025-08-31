#ifndef STROKE_SDFS
#define STROKE_SDFS

#include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/Falloff/FalloffFunctions.hlsl"
#include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/HashingFunctions.hlsl"
#include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/MathUtils.hlsl"

#define MAX_STROKE_ITERATIONS 5

struct StrokeData
{
    //Variables here have different formats to account for Unity`s own recommendation
    //See Using buffers with GPU buffers: https://docs.unity3d.com/6000.1/Documentation/Manual/SL-PlatformDifferences.html
    float4 coords;
    float4 direction;
    float4 additionalPackedData;
    float thickness;
    float thicknessFalloffConstraint;
    float length;
    float lengthThicknessFalloff;
    float pressure;
    float pressureFalloff;
    int iterations;
};

struct VariationData
{
    float directionVariation;
    float thicknessVariation;
    float lengthVariation;
    float pressureVariation;
};

float2 GetOriginCoordinate(float2 coords, float dimension)
{
    return float2((coords.x % 1) * (float)dimension, (coords.y % 1) * (float)dimension);
}

float2 GetOriginCoordinate(float2 coords, float2 dimensions)
{
    return float2((coords.x % 1) * (float)dimensions.x, (coords.y % 1) * (float)dimensions.y);
}

float GetInterpolatedFloatValue(float data, float dimension, float rate)
{
    return data * (float)dimension/rate;
}

float2 GetRotatedDirection(float2 direction, float normalizedAngleFactor)
{
    float angle = normalizedAngleFactor * PI;
    float2x2 rot = {
        float2(cos(angle), sin(angle)),
        float2(-sin(angle), cos(angle))
    };
    float2 dir = mul(rot, normalize(direction));
    return dir;
}

StrokeData RandomizeStrokePosition(StrokeData data, float uniqueID)
{
    float2 hash = hash21(uniqueID);

    StrokeData randPosData = {float4(hash.xy, 0.0, 0.0), data.direction, data.additionalPackedData,
        data.thickness, data.thicknessFalloffConstraint,
        data.length, data.lengthThicknessFalloff,
        data.pressure, data.pressureFalloff, data.iterations};
    return randPosData;
}

StrokeData RandomizeStrokePosition(StrokeData data, int uniqueID)
{
    return RandomizeStrokePosition(data, float(uniqueID));
}

StrokeData GetRandomizedParamsStrokeData(StrokeData baseData, VariationData variationData, float uniqueID, float dimension)
{
    float2 dirHash = hash21(uniqueID + dimension);
    float dirX = RangeConstrainedSmoothRandom(baseData.direction.x, variationData.directionVariation, dirHash.x, -1.0, 1.0);
    float dirY = RangeConstrainedSmoothRandom(baseData.direction.y, variationData.directionVariation, dirHash.y, -1.0, 1.0);
    float4 dir = float4(dirX, dirY, 0.0, 0.0);
    //offset to get different values
    float3 paramHash = hash31(uniqueID + (dimension * 2.0f));
    float thickness = RangeConstrainedSmoothRandom(baseData.thickness, variationData.thicknessVariation, paramHash.x, 0, 1.0);
    float length = RangeConstrainedSmoothRandom(baseData.length, variationData.lengthVariation, paramHash.y, 0, 1.0);
    float pressure = RangeConstrainedSmoothRandom(baseData.pressure, variationData.pressureVariation, paramHash.z, 0, 1.0);

    StrokeData randData = {baseData.coords, dir, baseData.additionalPackedData,
        thickness, baseData.thicknessFalloffConstraint,
        length, baseData.lengthThicknessFalloff,
        pressure, baseData.pressureFalloff, baseData.iterations};
    
    return randData;
}

StrokeData GetRandomizedParamsStrokeData(StrokeData baseData, VariationData variationData, int uniqueID, float dimension)
{
    return GetRandomizedParamsStrokeData(baseData, variationData, float(uniqueID), dimension);
}

// BASE SDF

uint BaseSDFBehaviour(StrokeData data, float2 pointID, float2 origin, float thickness, float length, float dimension)
{
    float2 endPoint = origin + normalize(data.direction).xy * length;
    
    //check closest point in all adjacent toroidal space grids
    float minDist = dimension;
    float interpolation = 0;
    [unroll(3)]
    for(int offsetX = -1; offsetX <= 1; offsetX++)
    {
        [unroll(3)]
        for(int offsetY = -1; offsetY <= 1; offsetY++) {
            float2 offset = float2(offsetX * dimension, offsetY * dimension);
            float2 offsetOrigin = origin + offset;
            float2 offsetLength = endPoint + offset;
            
            float2 v = offsetLength - offsetOrigin;
            float2 u = pointID - offsetOrigin;
            
            float t = saturate(dot(v, u)/dot(v, v));
            float2 closestPoint = offsetOrigin + t * v;
            float2 dir = pointID - closestPoint;
            float dist = sqrt(dir.x * dir.x + dir.y * dir.y);
            if(dist < minDist) {
                minDist = dist;
                interpolation = t;
            }
        }
    }
    //TODO: To alter the starting position of the curve, pass interpolation through another curve function to alter where 0 and where 1 are in relation to t
        
    float minThickness = lerp(0, thickness, data.thicknessFalloffConstraint);
    //attenuate falloff if length is too short
    float lengthFalloff = data.lengthThicknessFalloff * step(thickness/2, length + minThickness/2);
    float fallOff = lerp(thickness, minThickness, FalloffFunction(lengthFalloff * interpolation));
    float sample = step(fallOff, minDist);
    float expectedPressure = data.pressure * 1 - FalloffFunction(data.pressureFalloff * interpolation);
    sample = (1 - sample) * expectedPressure;
    
    return (1 - sample) * 255;
}

uint BaseSDFClampBehaviour(StrokeData data, float2 pointID, float2 origin, float thickness, float length)
{
    float2 endPoint = origin + normalize(data.direction).xy * length;
    float2 v = endPoint - origin;
    float2 u = pointID - origin;
    
    float t = saturate(dot(v, u)/dot(v, v));
    float2 closestPoint = origin + t * v;
    float2 dir = pointID - closestPoint;
    float minDist = sqrt(dir.x * dir.x + dir.y * dir.y);
    //TODO: To alter the starting position of the curve, pass interpolation through another curve function to alter where 0 and where 1 are in relation to t
        
    float minThickness = lerp(0, thickness, data.thicknessFalloffConstraint);
    //attenuate falloff if length is too short
    float lengthFalloff = data.lengthThicknessFalloff * step(thickness/2, length + minThickness/2);
    float fallOff = lerp(thickness, minThickness, FalloffFunction(lengthFalloff * t));
    float sample = step(fallOff, minDist);
    float expectedPressure = data.pressure * 1 - FalloffFunction(data.pressureFalloff * t);
    sample = (1 - sample) * expectedPressure;
    
    return (1 - sample) * 255;
}

uint SampleBaseSDF(StrokeData data, float2 pointID, float dimension) {
    float2 origin = GetOriginCoordinate(data.coords.xy, dimension);
    float thickness = GetInterpolatedFloatValue(data.thickness, dimension, 8.0);
    float length = GetInterpolatedFloatValue(data.length, dimension, 2.0);
    
   return BaseSDFBehaviour(data, pointID, origin, thickness, length, dimension);
}

uint SampleBaseSDFClamp(StrokeData data, float2 pointID, float dimension)
{
    float2 origin = GetOriginCoordinate(data.coords.xy, dimension);
    float thickness = GetInterpolatedFloatValue(data.thickness, dimension, 8.0);
    float length = GetInterpolatedFloatValue(data.length, dimension, 2.0);
    
    return BaseSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint SampleBaseSDFClamp(StrokeData data, float2 pointID, float2 dimensions)
{
    float largestDimension = max(dimensions.x, dimensions.y);
    float2 origin = GetOriginCoordinate(data.coords.xy, dimensions);
    float thickness = GetInterpolatedFloatValue(data.thickness, largestDimension, 8.0);
    float length = GetInterpolatedFloatValue(data.length, largestDimension, 2.0);
    
    return BaseSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint SampleBaseSDFClampParamScalar(StrokeData data, float2 pointID, float dimension, float thicknessScalar, float lengthScalar)
{
    float2 origin = GetOriginCoordinate(data.coords.xy, dimension);
    float thickness = GetInterpolatedFloatValue(data.thickness, dimension, 8.0 * thicknessScalar);
    float length = GetInterpolatedFloatValue(data.length, dimension, 2.0 * lengthScalar);
    
    return BaseSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint SampleBaseSDFClampParamScalar(StrokeData data, float2 pointID, float2 dimensions,  float thicknessScalar, float lengthScalar)
{
    float largestDimension = max(dimensions.x, dimensions.y);
    float2 origin = GetOriginCoordinate(data.coords.xy, dimensions);
    float thickness = GetInterpolatedFloatValue(data.thickness, largestDimension, 8.0 * thicknessScalar);
    float length = GetInterpolatedFloatValue(data.length, largestDimension, 2.0 * lengthScalar);
    
    return BaseSDFClampBehaviour(data, pointID, origin, thickness, length);
}

// REPEATING SDF
float SubStrokeSDFPattern(StrokeData data, float2 pointID, float dimension, float length, float thickness, float totalLength, int repetitions, int subStrokes, inout float2 lastOrigin,  inout float lengthSum)
{
    float isFirstSubStroke = 1.0 - abs(subStrokes - 1);
    float2 firstSubStrokeAngleLength = data.additionalPackedData.xy * isFirstSubStroke;
    float isSecondSubStroke = max(0.0, subStrokes - 1);
    float2 secondSubStrokeAngleLength = data.additionalPackedData.zw * isSecondSubStroke;
    
    float2 offsetDir = GetRotatedDirection(data.direction.xy, firstSubStrokeAngleLength.x + secondSubStrokeAngleLength.x);
    float baseLengthFactor = 1.0 - (isFirstSubStroke + isSecondSubStroke);
    float modifiedLength = length * (baseLengthFactor + pow(firstSubStrokeAngleLength.y, repetitions + 1) + pow(secondSubStrokeAngleLength.y, repetitions + 1));
    
    float2 endPoint = lastOrigin + (normalize(offsetDir).xy * modifiedLength);

    //check closest point in all adjacent toroidal space grids
    float minDist = dimension;
    float interpolation = 0;
    [loop]
    for(int offsetX = -1; offsetX <= 1; offsetX++)
    {
        [loop]
        for(int offsetY = -1; offsetY <= 1; offsetY++) {
            float2 offset = float2(offsetX * dimension, offsetY * dimension);
            float2 offsetOrigin = lastOrigin + offset;
            float2 offsetLength = endPoint + offset;
    
            float2 v = offsetLength - offsetOrigin;
            float2 u = pointID - offsetOrigin;
    
            float t = saturate(dot(v, u)/dot(v, v));
            float2 closestPoint = offsetOrigin + t * v;
            float2 dir = pointID - closestPoint;
            float dist = sqrt(dir.x * dir.x + dir.y * dir.y);
            if(dist < minDist) {
                minDist = dist;
                interpolation = t;
            }
        }
    }
    //TODO: To alter the starting position of the curve, pass interpolation through another curve function to alter where 0 and where 1 are in relation to t
    float strokeWideInterpolation = invLerp(0.0, totalLength, lengthSum + modifiedLength * interpolation);
    float minThickness = lerp(0, thickness, data.thicknessFalloffConstraint);
    //attenuate falloff if length is too short
    float lengthFalloff = data.lengthThicknessFalloff * step(thickness/2.0, modifiedLength + minThickness/2.0);
    float fallOff = lerp(thickness, minThickness, FalloffFunction(lengthFalloff * strokeWideInterpolation));
    float sample = step(fallOff, minDist);
    float expectedPressure = data.pressure * 1 - FalloffFunction(data.pressureFalloff * strokeWideInterpolation);
    sample = 1.0 - ((1 - sample) * expectedPressure);
    lastOrigin = endPoint;
    lengthSum += modifiedLength;

    return sample;
}

float SubStrokeSDFClampPattern(StrokeData data, float2 pointID, float length, float thickness, float totalLength, int repetitions, int subStrokes, inout float2 lastOrigin,  inout float lengthSum)
{
    float isFirstSubStroke = 1.0 - abs(subStrokes - 1);
    float2 firstSubStrokeAngleLength = data.additionalPackedData.xy * isFirstSubStroke;
    float isSecondSubStroke = max(0.0, subStrokes - 1);
    float2 secondSubStrokeAngleLength = data.additionalPackedData.zw * isSecondSubStroke;
    
    float2 offsetDir = GetRotatedDirection(data.direction.xy, firstSubStrokeAngleLength.x + secondSubStrokeAngleLength.x);
    float baseLengthFactor = 1.0 - (isFirstSubStroke + isSecondSubStroke);
    float modifiedLength = length * (baseLengthFactor + pow(firstSubStrokeAngleLength.y, repetitions + 1) + pow(secondSubStrokeAngleLength.y, repetitions + 1));
    
    float2 endPoint = lastOrigin + (normalize(offsetDir).xy * modifiedLength);

    float2 v = endPoint - lastOrigin;
    float2 u = pointID - lastOrigin;

    float interpolation = saturate(dot(v, u)/dot(v, v));
    float2 closestPoint = lastOrigin + interpolation * v;
    float2 dir = pointID - closestPoint;
    float minDist = sqrt(dir.x * dir.x + dir.y * dir.y);

    //TODO: To alter the starting position of the curve, pass interpolation through another curve function to alter where 0 and where 1 are in relation to t
    float strokeWideInterpolation = invLerp(0.0, totalLength, lengthSum + modifiedLength * interpolation);
    float minThickness = lerp(0, thickness, data.thicknessFalloffConstraint);
    //attenuate falloff if length is too short
    float lengthFalloff = data.lengthThicknessFalloff * step(thickness/2.0, modifiedLength + minThickness/2.0);
    float fallOff = lerp(thickness, minThickness, FalloffFunction(lengthFalloff * strokeWideInterpolation));
    float sample = step(fallOff, minDist);
    float expectedPressure = data.pressure * 1 - FalloffFunction(data.pressureFalloff * strokeWideInterpolation);
    sample = 1.0 - ((1 - sample) * expectedPressure);
    lastOrigin = endPoint;
    lengthSum += modifiedLength;

    return sample;
}

uint RepeatingSDFBehaviour(StrokeData data, float2 pointID, float2 origin, float thickness, float length, float dimension)
{
    float2 lastOrigin = origin;
    float totalSample = 1.0;
    float totalLength = length + (length * data.additionalPackedData.y) + (length * data.additionalPackedData.y * data.additionalPackedData.w);
    float lengthSum = 0;
    
    [unroll(MAX_STROKE_ITERATIONS)]
    for (int its = 0; its < MAX_STROKE_ITERATIONS; its++)
    {
        if (its >= data.iterations)
            break;

        float isFirst = 1.0 - step(0.5, (float)its);
        
        [loop]
        for(int subStrokes = 1 - isFirst; subStrokes < 3; subStrokes++)
        {
            float sample = SubStrokeSDFPattern(data, pointID, dimension, length, thickness, totalLength, its, subStrokes, lastOrigin, lengthSum);
            totalSample = min(totalSample, sample);
        }
    } 
    
    return totalSample * 255;
}

uint RepeatingSDFClampBehaviour(StrokeData data, float2 pointID, float2 origin, float thickness, float length)
{
    float2 lastOrigin = origin;
    float totalSample = 1.0;
    float totalLength = length + (length * data.additionalPackedData.y) + (length * data.additionalPackedData.y * data.additionalPackedData.w);
    float lengthSum = 0;
    
    [unroll(MAX_STROKE_ITERATIONS)]
    for (int its = 0; its < MAX_STROKE_ITERATIONS; its++)
    {
        if (its >= data.iterations)
            break;

        float isFirst = 1.0 - step(0.5, (float)its);
        
        [loop]
        for(int subStrokes = 1 - isFirst; subStrokes < 3; subStrokes++)
        {
            float sample = SubStrokeSDFClampPattern(data, pointID, length, thickness, totalLength, its, subStrokes, lastOrigin, lengthSum);
            totalSample = min(totalSample, sample);
        }
    } 
    
    return totalSample * 255;
}

uint SampleRepeatingSDF(StrokeData data, float2 pointID, float dimension) {
    float2 origin = GetOriginCoordinate(data.coords.xy, dimension);
    float thickness = GetInterpolatedFloatValue(data.thickness, dimension, 8.0);
    float length = GetInterpolatedFloatValue(data.length, dimension, 2.0);
    
    return RepeatingSDFBehaviour(data, pointID, origin, thickness, length, dimension);
}

uint SampleRepeatingSDFClamp(StrokeData data, float2 pointID, float dimension)
{
    float2 origin = GetOriginCoordinate(data.coords.xy, dimension);
    float thickness = GetInterpolatedFloatValue(data.thickness, dimension, 8.0);
    float length = GetInterpolatedFloatValue(data.length, dimension, 2.0);
    
    return RepeatingSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint SampleRepeatingSDFClamp(StrokeData data, float2 pointID, float2 dimensions)
{
    float largestDimension = max(dimensions.x, dimensions.y);
    float2 origin = GetOriginCoordinate(data.coords.xy, dimensions);
    float thickness = GetInterpolatedFloatValue(data.thickness, largestDimension, 8.0);
    float length = GetInterpolatedFloatValue(data.length, largestDimension, 2.0);
    
    return RepeatingSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint SampleRepeatingSDFClampParamScalar(StrokeData data, float2 pointID, float dimension, float thicknessScalar, float lengthScalar)
{
    float2 origin = GetOriginCoordinate(data.coords.xy, dimension);
    float thickness = GetInterpolatedFloatValue(data.thickness, dimension, 8.0 * thicknessScalar);
    float length = GetInterpolatedFloatValue(data.length, dimension, 2.0 * lengthScalar);
    
    return RepeatingSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint SampleRepeatingSDFClampParamScalar(StrokeData data, float2 pointID, float2 dimensions,  float thicknessScalar, float lengthScalar)
{
    float largestDimension = max(dimensions.x, dimensions.y);
    float2 origin = GetOriginCoordinate(data.coords.xy, dimensions);
    float thickness = GetInterpolatedFloatValue(data.thickness, largestDimension, 8.0 * thicknessScalar);
    float length = GetInterpolatedFloatValue(data.length, largestDimension, 2.0 * lengthScalar);
    
    return RepeatingSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint LoopRepeatingSDFBehaviour(StrokeData data, float2 pointID, float2 origin, float thickness, float length, float dimension)
{
    float2 lastOrigin = origin;
    float totalSample = 1.0;
    float totalLength = length + (length * data.additionalPackedData.y) + (length * data.additionalPackedData.y * data.additionalPackedData.w);
    float lengthSum = 0;
    
    [unroll(MAX_STROKE_ITERATIONS)]
    for (int its = 0; its < MAX_STROKE_ITERATIONS; its++)
    {
        if (its >= data.iterations)
            break;

        float isFirst = 1.0 - step(0.5, (float)its);
        
        [loop]
        for(int subStrokes = 1 - isFirst; subStrokes < 3; subStrokes++)
        {
            float sample = SubStrokeSDFPattern(data, pointID, dimension, length, thickness, totalLength, its, subStrokes, lastOrigin, lengthSum);
            totalSample = min(totalSample, sample);
        }

        [loop]
        for (int loopStrokes = 1; loopStrokes >= 0; loopStrokes--)
        {
            float sample = SubStrokeSDFPattern(data, pointID, dimension, length, thickness, totalLength, its, loopStrokes, lastOrigin, lengthSum);
            totalSample = min(totalSample, sample);
        }
    }
    
    return totalSample * 255;
}

uint LoopRepeatingSDFClampBehaviour(StrokeData data, float2 pointID, float2 origin, float thickness, float length)
{
    float2 lastOrigin = origin;
    float totalSample = 1.0;
    float totalLength = length + (length * data.additionalPackedData.y) + (length * data.additionalPackedData.y * data.additionalPackedData.w);
    float lengthSum = 0;
    
    [unroll(MAX_STROKE_ITERATIONS)]
    for (int its = 0; its < MAX_STROKE_ITERATIONS; its++)
    {
        if (its >= data.iterations)
            break;

        float isFirst = 1.0 - step(0.5, (float)its);
        
        [loop]
        for(int subStrokes = 1 - isFirst; subStrokes < 3; subStrokes++)
        {
            float sample = SubStrokeSDFClampPattern(data, pointID, length, thickness, totalLength, its, subStrokes, lastOrigin, lengthSum);
            totalSample = min(totalSample, sample);
        }

        [loop]
        for (int loopStrokes = 1; loopStrokes >= 0; loopStrokes--)
        {
            float sample = SubStrokeSDFClampPattern(data, pointID, length, thickness, totalLength, its, loopStrokes, lastOrigin, lengthSum);
            totalSample = min(totalSample, sample);
        }
    }
    
    return totalSample * 255;
}

uint SampleLoopRepeatingSDF(StrokeData data, float2 pointID, float dimension) {
    float2 origin = GetOriginCoordinate(data.coords.xy, dimension);
    float thickness = GetInterpolatedFloatValue(data.thickness, dimension, 8.0);
    float length = GetInterpolatedFloatValue(data.length, dimension, 2.0);
    
    return LoopRepeatingSDFBehaviour(data, pointID, origin, thickness, length, dimension);
}

uint SampleLoopRepeatingSDFClamp(StrokeData data, float2 pointID, float dimension)
{
    float2 origin = GetOriginCoordinate(data.coords.xy, dimension);
    float thickness = GetInterpolatedFloatValue(data.thickness, dimension, 8.0);
    float length = GetInterpolatedFloatValue(data.length, dimension, 2.0);
    
    return LoopRepeatingSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint SampleLoopRepeatingSDFClamp(StrokeData data, float2 pointID, float2 dimensions)
{
    float largestDimension = max(dimensions.x, dimensions.y);
    float2 origin = GetOriginCoordinate(data.coords.xy, dimensions);
    float thickness = GetInterpolatedFloatValue(data.thickness, largestDimension, 8.0);
    float length = GetInterpolatedFloatValue(data.length, largestDimension, 2.0);
    
    return LoopRepeatingSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint SampleLoopRepeatingSDFClampParamScalar(StrokeData data, float2 pointID, float dimension, float thicknessScalar, float lengthScalar)
{
    float2 origin = GetOriginCoordinate(data.coords.xy, dimension);
    float thickness = GetInterpolatedFloatValue(data.thickness, dimension, 8.0 * thicknessScalar);
    float length = GetInterpolatedFloatValue(data.length, dimension, 2.0 * lengthScalar);
    
    return LoopRepeatingSDFClampBehaviour(data, pointID, origin, thickness, length);
}

uint SampleLoopRepeatingSDFClampParamScalar(StrokeData data, float2 pointID, float2 dimensions,  float thicknessScalar, float lengthScalar)
{
    float largestDimension = max(dimensions.x, dimensions.y);
    float2 origin = GetOriginCoordinate(data.coords.xy, dimensions);
    float thickness = GetInterpolatedFloatValue(data.thickness, largestDimension, 8.0 * thicknessScalar);
    float length = GetInterpolatedFloatValue(data.length, largestDimension, 2.0 * lengthScalar);
    
    return LoopRepeatingSDFClampBehaviour(data, pointID, origin, thickness, length);
}

#endif