Shader "SketchRenderer/ColorSilhouette"
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
           #include "Packages/com.lorenzogrando.sketchrenderer/Shader/Outlining/EdgeDetection/ColorInclude.hlsl"

           #pragma multi_compile_local SOURCE_DEPTH SOURCE_DEPTH_NORMALS
           #pragma multi_compile_local SOBEL_KERNEL_3X3 SOBEL_KERNEL_1X3
           
           #pragma vertex Vert
           #pragma fragment Frag
           
           float _OutlineOffset;
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 uv = input.texcoord;
               
               //ColorSamples
               //UV positions for samples
               float2 dUL, dUC, dUR, dCL, dCR, dDL, dDC, dDR;
               Get3X3NeighborhoodPositions(uv, _OutlineOffset, _CameraOpaqueTexture_TexelSize.xy, dUL, dUC, dUR, dCL, dCR, dDL, dDC, dDR);
               
               #if defined(SOBEL_KERNEL_3X3)
               float colorEdge = SobelColorHorizontal3x3(BaseSobel3X3HorizontalKernel, uv, dUL, dCL, dDL, dUR, dCR, dDR);
               #elif defined(SOBEL_KERNEL_1X3)
               float colorEdge = SobelColor1X3(Sobel1X3Kernel, dCL, uv, dCR);
               #endif

               
               //Store for use in vertical pass
               return float4(colorEdge, 0, 0, 1);
               return float4((colorEdge + 1) * 0.5, 0, 0, 1);
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
           #include "Packages/com.lorenzogrando.sketchrenderer/Shader/Outlining/EdgeDetection/ColorInclude.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag
           
           #pragma multi_compile_local SOBEL_KERNEL_3X3 SOBEL_KERNEL_1X3
           #pragma multi_compile_local OUTPUT_GREYSCALE OUTPUT_DIRECTION_DATA_ANGLE OUTPUT_DIRECTION_DATA_VECTOR

           int _OutlineOffset;
           float _OutlineThreshold;
           float _DirectionStrengthMod;
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 uv = input.texcoord;

               //ColorSamples
               //UV positions for samples
               float2 dUL, dUC, dUR, dCL, dCR, dDL, dDC, dDR;
               Get3X3NeighborhoodPositions(uv, _OutlineOffset, _CameraOpaqueTexture_TexelSize.xy, dUL, dUC, dUR, dCL, dCR, dDL, dDC, dDR);
               
               #if defined(SOBEL_KERNEL_3X3)
               float colorEdge = SobelColorVertical3x3(BaseSobel3X3VerticalKernel, uv, dUL, dUC, dUR, dDL, dDC, dDR);
               #elif defined(SOBEL_KERNEL_1X3)
               float colorEdge = SobelColor1X3(Sobel1X3Kernel, dUC, uv, dDC);
               #endif
               
               //Sample horizontal pass (passed as blit texture) and get filter results in R
               float4 horizontalValue = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, _BlitMipLevel);
               //Bring color back to -1 to 1
               //horizontalValue.r = (horizontalValue.r * 2.0) - 1.0;
                //Get gradient of each image, and threshold for silhouette)
               
               float2 colorGradientVector = float2(horizontalValue.r, colorEdge);
               float colorGradient = step(_OutlineThreshold, length(colorGradientVector));

               //Set alpha as outline strenght, to easy blending in composite shader
               #if defined(OUTPUT_GREYSCALE)
                   return float4(max(0, colorGradient).rrrr);
               #elif defined(OUTPUT_DIRECTION_DATA_ANGLE)
                    float angle = atan2(colorGradientVector.y, colorGradientVector.x);
                    angle /= PI;
                    angle = (angle + 1) * 0.5;
                    float edge = max(0, colorGradient);
                    //Is Edge, the angle of outline flow (only if edge present), the strength of the direction, and a repeated edge presence as the alpha strength
                    return float4(edge * _DirectionStrengthMod, angle * edge, 0.0, edge);
               #elif defined(OUTPUT_DIRECTION_DATA_VECTOR)
                    float2 direction = colorGradientVector.xy;
                    float edge = max(0, colorGradient);
                    //Is Edge, the direciton of outline flow (only if edge present) modified by the strength of the direction, and a repeated edge presence as the alpha strength
                    return float4(edge  * _DirectionStrengthMod, direction * edge, edge);
               #endif
           }

           ENDHLSL
       }
   }
}