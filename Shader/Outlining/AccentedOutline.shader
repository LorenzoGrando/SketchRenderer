Shader "SketchRenderer/AccentedOutline"
{
    SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "Accented Outlines"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
           #include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/NoiseFunctions.hlsl"
           
           #pragma vertex Vert
           #pragma fragment Frag

           #pragma multi_compile_local_fragment _ DISTORT_OUTLINES BAKED_DISTORT_OUTLINES
           #pragma multi_compile_local_fragment _ MASK_OUTLINES
           #pragma multi_compile_local_fragment _ MULTIPLE_DISTORTIONS

           float _DistortionRate;
           float _DistortionStrength;
           float _DistortionOffset;
           float _DistortionFlatSeed;

           int _AdditionalLines;
           float _LineStrengthJitter;
           float _LineTintFalloff;
           
           TEXTURE2D(_OutlineMaskTex);
           SAMPLER(sampler_OutlineMaskTex);

           TEXTURE2D(_BakedUVDistortionTex);
           TEXTURE2D(_BakedUVDistortionTex2);

           CBUFFER_START(UnityPerMaterial)
                float4 _OutlineMaskTex_ST;
           CBUFFER_END
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 uv1 = input.texcoord;
               float2 uv2 = input.texcoord;
               float4 col = (float4)0;
               float4 strengths = floor(float4(1.0, step(0.9, _AdditionalLines), step(1.9, _AdditionalLines), step(2.9, _AdditionalLines)));
               _LineStrengthJitter /= 100.0;

               #if defined MULTIPLE_DISTORTIONS
               float2 uv3 = input.texcoord;
               float2 uv4 = input.texcoord;
               #endif

               #if defined MASK_OUTLINES
               float2 maskUvs = TRANSFORM_TEX(uv1, _OutlineMaskTex);
               float mask = SAMPLE_TEXTURE2D_X_LOD(_OutlineMaskTex, sampler_OutlineMaskTex, maskUvs, _BlitMipLevel).r;
               #endif

               #if defined DISTORT_OUTLINES
               float displacementX1 = (1 - Voronoi(uv1, 1.0, _DistortionRate)) * 2 - 1;
               float displacementY1 = (1 - Voronoi(uv1, -1.0, _DistortionRate)) * 2 - 1;
               uv1 = saturate(uv1 + float2(displacementX1, displacementY1) * _DistortionStrength);

               float displacementX2 = (1 - Voronoi(uv2, 1.0 + _DistortionFlatSeed, _DistortionRate)) * 2 - 1;
               float displacementY2 = (1 - Voronoi(uv2, -1.0 - _DistortionFlatSeed, _DistortionRate)) * 2 - 1;
               uv2 = saturate(uv2 + float2(displacementX2, displacementY2) * (_DistortionStrength + _LineStrengthJitter));

                    #if defined MULTIPLE_DISTORTIONS
               float displacementX3 = (1 - Voronoi(uv3 + _DistortionOffset, 1.0, _DistortionRate)) * 2 - 1;
               float displacementY3 = (1 - Voronoi(uv3 + _DistortionOffset, -1.0, _DistortionRate)) * 2 - 1;
               uv3 = saturate(uv3 + float2(displacementX3, displacementY3) * (_DistortionStrength + _LineStrengthJitter * 2));

               float displacementX4 = (1 - Voronoi(uv4 + _DistortionOffset, 1.0 + _DistortionFlatSeed, _DistortionRate)) * 2 - 1;
               float displacementY4 = (1 - Voronoi(uv4 + _DistortionOffset, -1.0 - _DistortionFlatSeed, _DistortionRate)) * 2 - 1;
               uv4 = saturate(uv4 + float2(displacementX4, displacementY4) * (_DistortionStrength + _LineStrengthJitter * 3));
                    #endif
               #elif defined BAKED_DISTORT_OUTLINES
               float4 angleStrengths = SAMPLE_TEXTURE2D_X_LOD(_BakedUVDistortionTex, sampler_PointClamp, input.texcoord, _BlitMipLevel);
               float2 angleStrength1 = angleStrengths.xy;
               float2 angleStrength2 = angleStrengths.zw;
               
               angleStrength1.x = angleStrength1.x * 2.0 - 1.0;
               angleStrength1.x *= PI;
               float2 distUVs = float2(cos(angleStrength1.x), sin(angleStrength1.x)) * angleStrength1.y;
               uv1 = saturate(uv1 + distUVs * _DistortionStrength);

               angleStrength2.x = angleStrength2.x * 2.0 - 1.0;
               angleStrength2.x *= PI;
               float2 distUVs2 = float2(cos(angleStrength2.x), sin(angleStrength2.x)) * angleStrength2.y;
               uv2 = saturate(uv2 + distUVs2 * (_DistortionStrength + _LineStrengthJitter));
                    #if defined MULTIPLE_DISTORTIONS
               float4 angleStrengths2 = SAMPLE_TEXTURE2D_X_LOD(_BakedUVDistortionTex, sampler_PointClamp, input.texcoord, _BlitMipLevel);
               float2 angleStrength3 = angleStrengths2.xy;
               float2 angleStrength4 = angleStrengths2.zw;
               
               angleStrength3.x = angleStrength3.x * 2.0 - 1.0;
               angleStrength3.x *= PI;
               float2 distUVs3 = float2(cos(angleStrength3.x), sin(angleStrength3.x)) * angleStrength3.y;
               uv3 = saturate(uv3 + distUVs3 * (_DistortionStrength + _LineStrengthJitter * 2.0));

               angleStrength4.x = angleStrength4.x * 2.0 - 1.0;
               angleStrength4.x *= PI;
               float2 distUVs4 = float2(cos(angleStrength4.x), sin(angleStrength4.x)) * angleStrength4.y;
               uv4 = saturate(uv4 + distUVs4 * (_DistortionStrength + _LineStrengthJitter * 3.0));
                    #endif
               #endif
               
               col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv1, _BlitMipLevel) * strengths.x ;
               col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv2, _BlitMipLevel) * strengths.y * _LineTintFalloff;

               #if defined MULTIPLE_DISTORTIONS
               col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv3, _BlitMipLevel) * strengths.z * pow(_LineTintFalloff, 2.0);
               col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv4, _BlitMipLevel) * strengths.w * pow(_LineTintFalloff, 3.0);
               #endif

               #if defined MASK_OUTLINES
               col = lerp((float4)0, col, 1.0 - mask);
               #endif
               
               return float4(col.rgb, col.r);
           }

           ENDHLSL
       }

       Pass
       {
           Name "Bake Distorted UVs"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
           #include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/NoiseFunctions.hlsl"
           
           #pragma vertex Vert
           #pragma fragment Frag

           float _DistortionRate;
           float _DistortionStrength;
           float _DistortionOffset;
           float _DistortionFlatSeed;
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 uv = input.texcoord;

               float displacementX = (1 - Voronoi(uv + _DistortionOffset, 1.0, _DistortionRate)) * 2 - 1;
               float displacementY = (1 - Voronoi(uv + _DistortionOffset, -1.0, _DistortionRate)) * 2 - 1;
               float2 displacementUVs = float2(displacementX, displacementY);
               float angle = atan2(displacementY, displacementX);
               angle /= PI;
               angle = (angle + 1) * 0.5;
               float magn = saturate(length(displacementUVs));
               
               
               float displacementX2 = (1 - Voronoi(uv + _DistortionOffset, 1.0 + _DistortionFlatSeed, _DistortionRate)) * 2 - 1;
               float displacementY2 = (1 - Voronoi(uv + _DistortionOffset, -1.0 - _DistortionFlatSeed, _DistortionRate)) * 2 - 1;
               float2 displacementUVs2 = float2(displacementX2, displacementY2);
               float angle2 = atan2(displacementY2, displacementX2);
               angle2 /= PI;
               angle2 = (angle2 + 1) * 0.5;
               float magn2 = saturate(length(displacementUVs2));
               
               return float4(angle, magn, angle2, magn2);
           }

           ENDHLSL
       }
   }
}