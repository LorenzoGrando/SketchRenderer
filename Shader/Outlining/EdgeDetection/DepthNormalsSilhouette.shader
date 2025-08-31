Shader "SketchRenderer/DepthNormalsSilhouette"
{
   SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "Horizontal Sobel"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
           #include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/NeighborhoodSample.hlsl"
           #include "Packages/com.lorenzogrando.sketchrenderer/Shader/Outlining/EdgeDetection/DepthNormalsInclude.hlsl"

           #pragma multi_compile_local SOURCE_DEPTH SOURCE_DEPTH_NORMALS
           #pragma multi_compile_local SOBEL_KERNEL_3X3 SOBEL_KERNEL_1X3
           
           #pragma vertex Vert
           #pragma fragment Frag
           
           float _OutlineOffset;
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 uv = input.texcoord;
               
               //DepthSamples
               //UV positions for samples
               float2 dUL, dUC, dUR, dCL, dCR, dDL, dDC, dDR;
               Get3X3NeighborhoodPositions(uv, _OutlineOffset, _CameraDepthTexture_TexelSize.xy, dUL, dUC, dUR, dCL, dCR, dDL, dDC, dDR);

               #if defined(SOBEL_KERNEL_3X3)
               float depthEdge = SobelDepthHorizontal3X3(ModifiedSobel3X3HorizontalKernel,dUL, dCL, dDL, dUR, dCR, dDR);
               #elif defined(SOBEL_KERNEL_1X3)
               float depthEdge = SobelDepth1X3(Sobel1X3Kernel, dCL, uv, dCR);
               #endif
               
               #if defined(SOURCE_DEPTH_NORMALS)
               //NormalsSamples
               //UV positions for samples
               float2 nUL, nUC, nUR, nCL, nCR, nDL, nDC, nDR;
               Get3X3NeighborhoodPositions(uv, _OutlineOffset, _CameraNormalsTexture_TexelSize.xy, nUL, nUC, nUR, nCL, nCR, nDL, nDC, nDR);

               #if defined(SOBEL_KERNEL_3X3)
               float normalEdge = SobelNormalHorizontal3x3(ModifiedSobel3X3HorizontalKernel, uv, nUL, nCL, nDL, nUR, nCR, nDR);
               #elif defined(SOBEL_KERNEL_1X3)
               float normalEdge = SobelNormal1X3(Sobel1X3Kernel, nCL, uv, nCR);
               #endif
               
               #endif

               //Store for use in vertical pass
               #if defined(SOURCE_DEPTH)
               return float4((depthEdge + 1) * 0.5, 0, 0, 1);
               #elif defined(SOURCE_DEPTH_NORMALS)
               return float4((depthEdge + 1) * 0.5, (normalEdge + 1) * 0.5, 0.0, 1.0);
               #endif
           }

           ENDHLSL
       }

       Pass
       {
           Name "Vertical Sobel"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
           #include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/NeighborhoodSample.hlsl"
           #include "Packages/com.lorenzogrando.sketchrenderer/Shader/Outlining/EdgeDetection/DepthNormalsInclude.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag

           #pragma multi_compile_local SOURCE_DEPTH SOURCE_DEPTH_NORMALS
           #pragma multi_compile_local SOBEL_KERNEL_3X3 SOBEL_KERNEL_1X3
           #pragma multi_compile_local OUTPUT_GREYSCALE OUTPUT_DIRECTION_DATA_ANGLE OUTPUT_DIRECTION_DATA_VECTOR

           int _OutlineOffset;
           float _OutlineThreshold;
           float _OutlineDistanceFalloff;
           float _OutlineShallowThresholdSensitivity;
           float _OutlineShallowThresholdStrength;
           float _OutlineNormalDistanceSensitivity;
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 uv = input.texcoord;

               //DepthSamples
               //UV positions for samples
               float2 dUL, dUC, dUR, dCL, dCR, dDL, dDC, dDR;
               Get3X3NeighborhoodPositions(uv, _OutlineOffset, _CameraDepthTexture_TexelSize.xy, dUL, dUC, dUR, dCL, dCR, dDL, dDC, dDR);

               #if defined(SOBEL_KERNEL_3X3)
               float depthEdge = SobelDepthVertical3X3(ModifiedSobel3X3VerticalKernel,dUL, dUC, dUR, dDL, dDC, dDR);
               #elif defined(SOBEL_KERNEL_1X3)
               float depthEdge = SobelDepth1X3(Sobel1X3Kernel, dUC, uv, dDC);
               #endif

               #if defined(SOURCE_DEPTH_NORMALS)
               //NormalsSamples
               //UV positions for samples
               float2 nUL, nUC, nUR, nCL, nCR, nDL, nDC, nDR;
               Get3X3NeighborhoodPositions(uv, _OutlineOffset, _CameraNormalsTexture_TexelSize.xy, nUL, nUC, nUR, nCL, nCR, nDL, nDC, nDR);

               #if defined(SOBEL_KERNEL_3X3)
               float3 normalEdge = SobelNormalVertical3x3(ModifiedSobel3X3VerticalKernel, uv, nUL, nUC, nUR, nDL, nDC, nDR);
               #elif defined(SOBEL_KERNEL_1X3)
               float normalEdge = SobelNormal1X3(Sobel1X3Kernel, dUC, uv, dDC);
               #endif
               #endif
               
               
               //Sample horizontal pass (passed as blit texture) and get filter results in RG (depth, normal)
               float4 horizontalValue = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, _BlitMipLevel);
               //Bring depth back to -1 to 1
               horizontalValue.r = (horizontalValue.r * 2.0) - 1.0;
               //Get gradient of each image, and threshold for silhouette)
               //If normals texture is available, modify threshold depending on viewing angle to avoid thick edges in very shallow angles
               //Specifically, if view vector is almost perpendicular to surface normal, make the threshold higher.
               #if defined(SOURCE_DEPTH)
               float depth = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
               float2 depthGradientVector = float2(horizontalValue.r, depthEdge);
               float thresholdScale = lerp(1.0, 5.0, _OutlineDistanceFalloff);
               float threshold = _OutlineThreshold * lerp(1.0, thresholdScale * (1 + _OutlineOffset), depth/60);
               float depthGradient = step(threshold, length(depthGradientVector));
               #elif defined (SOURCE_DEPTH_NORMALS)
               float3 surfaceNormal = SampleSceneNormals(uv);
               //Get view vector, see bgolus and keijiro: https://discussions.unity.com/t/help-with-view-space-normals/654031/12
               float2 perspectiveProj = float2(unity_CameraProjection._11, unity_CameraProjection._22);
               float3 viewDir = -normalize(float3((uv * 2 - 1)/perspectiveProj, -1));
               
               half isShallow = step(1 - _OutlineShallowThresholdSensitivity, 1 - dot(viewDir, surfaceNormal));
               float depth = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
               float thresholdScale = lerp(1.0, 50.0, _OutlineDistanceFalloff);
               float threshold = _OutlineThreshold * lerp(1.0, thresholdScale * (1 + _OutlineOffset), depth/60);
               threshold += isShallow * (_OutlineThreshold * 20.0 * _OutlineShallowThresholdStrength);
               float2 depthGradientVector = float2(horizontalValue.r, depthEdge);
               float depthGradient = step(threshold, length(depthGradientVector));
               #endif
               
               #if defined(SOURCE_DEPTH_NORMALS)
               horizontalValue.g = (horizontalValue.g * 2.0) - 1.0;
               float2 normalGradientVector = float2(horizontalValue.g, normalEdge.r)/2.0;
               float gradN = length(normalGradientVector);

               float eyeDepth = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
               float normalDistanceThresholdOffset = (1 - _OutlineThreshold) * (lerp(0, 1, eyeDepth/32) * ( 1 - _OutlineNormalDistanceSensitivity));
               
               float normalGradient = step(_OutlineThreshold + normalDistanceThresholdOffset, gradN);
               #endif

               //Set alpha as outline strenght, to easy blending in composite shader
               #if defined(OUTPUT_GREYSCALE)
                   #if defined(SOURCE_DEPTH)
                   return float4(max(0, depthGradient).rrrr);
                   #elif defined(SOURCE_DEPTH_NORMALS)
                   return float4(max(0, max(depthGradient, normalGradient)).rrrr);
                   #endif
               #elif defined(OUTPUT_DIRECTION_DATA_ANGLE)
                    #if defined(SOURCE_DEPTH)
                    float angle = atan2(depthGradientVector.y, depthGradientVector.x);
                    angle /= PI;
                    angle = (angle + 1) * 0.5;
                    float edge = max(0, depthGradient);
                    return float4(edge, angle, 0.0, edge);
                    #elif defined(SOURCE_DEPTH_NORMALS)
                    float edge = max(0, max(depthGradient, normalGradient));
                    float angleDepth = atan2(depthGradientVector.y, depthGradientVector.x);
                    float angleNormal = atan2(-normalGradientVector.y, normalGradientVector.x);
                    float useDepth = step(1, depthGradient);
                    float useNormal = 1 - useDepth;
                    float angle = angleDepth * useDepth + angleNormal * useNormal;
                    angle /= PI;
                    angle = (angle + 1) * 0.5;
                    return float4(edge, angle * edge, 0.0, edge);
                    #endif
               #elif defined(OUTPUT_DIRECTION_DATA_VECTOR)
                    #if defined(SOURCE_DEPTH)
                    float2 direction = depthGradientVector.xy;
                    float edge = max(0, depthGradient);
                    return float4(edge, direction, edge);
                    #elif defined(SOURCE_DEPTH_NORMALS)
                    float edge = max(0, max(depthGradient, normalGradient));
                    float2 directionDepth = depthGradientVector.xy;
                    float2 directionNormal = float2(-normalGradientVector.y, normalGradientVector.x);
                    float useDepth = step(0.05, depthGradient);
                    float useNormal = 1 - useDepth;
                    float2 direction = normalize(directionDepth * useDepth + directionNormal * useNormal);
                    return float4(edge, ((direction * 0.5) + 0.5) * edge, edge);
                    #endif
               #endif
           }

           ENDHLSL
       }
   }
}