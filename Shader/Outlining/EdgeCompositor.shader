Shader "SketchRenderer/EdgeCompositor"
{
    SubShader
    {
         Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
         ZWrite Off Cull Off
         Pass
         {
           Name "Edge Composition"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
           
           #pragma vertex Vert
           #pragma fragment Frag

           Texture2D _PrimaryEdgeTex;
           Texture2D _SecondaryEdgeTex;
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 uv = input.texcoord;

               float4 primaryColor = SAMPLE_TEXTURE2D_X(_PrimaryEdgeTex, sampler_PointClamp, uv);
               float4 secondaryColor = SAMPLE_TEXTURE2D_X(_SecondaryEdgeTex, sampler_PointClamp, uv);

               float isPrimary = step (0.5 , primaryColor.a);
               
               if(isPrimary)
                   return float4(primaryColor * isPrimary);
               else
                   return float4(secondaryColor * (1.0 - isPrimary));
           }

           ENDHLSL
         }
    }
}