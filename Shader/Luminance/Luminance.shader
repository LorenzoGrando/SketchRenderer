Shader "SketchRenderer/Luminance"
{
    SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "Image Luminance"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
           #include "Packages/com.lorenzogrando.sketchrenderer/Shader/Luminance/TamSampleInclude.hlsl"
           #include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/LuminanceSample.hlsl"
        
           
           #pragma vertex Vert
           #pragma fragment Frag

           #pragma multi_compile_local_fragment TAM_SINGLE TAM_DOUBLE TAM_TRIPLE
           #pragma multi_compile_local_fragment _ QUANTIZE
           #pragma multi_compile_local_fragment UVS_SCREEN_SPACE UVS_OBJECT_SPACE UVS_OBJECT_SPACE_CONSTANT UVS_OBJECT_SPACE_REVERSED_CONSTANT

           int _NumTones;
           float _LuminanceOffset;

           TEXTURE2D(_CameraUVsTexture);
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 screenSpaceUV = input.texcoord;
               #if defined UVS_OBJECT_SPACE || defined UVS_OBJECT_SPACE_CONSTANT || defined UVS_OBJECT_SPACE_REVERSED_CONSTANT
               float3 objectUVsMip = SAMPLE_TEXTURE2D_X_LOD(_CameraUVsTexture, sampler_PointClamp, screenSpaceUV, _BlitMipLevel).xyz;
               objectUVsMip.z = GetMipLevel(objectUVsMip.z, max(_Tam0_2_TexelSize.z, _Tam0_2_TexelSize.w));
               #endif

               //get pixel luminance: https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
               float4 col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, screenSpaceUV, _BlitMipLevel);
               float lum = pow(SamplePerceivedLuminance(col.rgb), _LuminanceOffset);
               #if defined(QUANTIZE)
               lum = floor(lum * _NumTones)/_NumTones;
               #endif

               #if defined UVS_OBJECT_SPACE || defined UVS_OBJECT_SPACE_CONSTANT || defined UVS_OBJECT_SPACE_REVERSED_CONSTANT
               float stroke = SampleTAM(lum, _NumTones, objectUVsMip.xy, objectUVsMip.z);
               #else
               float stroke = SampleTAM(lum, _NumTones, screenSpaceUV, _BlitMipLevel);
               #endif
               
               return float4(stroke.rrrr);
           }

           ENDHLSL
       }
   }
}