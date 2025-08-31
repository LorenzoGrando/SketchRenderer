Shader "SketchRenderer/ThicknessDilation"
{
    SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "Thicken Outlines"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
           
           #pragma vertex Vert
           #pragma fragment Frag
           
           int _OutlineSize;
           float _OutlineStrength;
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 uv = input.texcoord;
               float4 col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, _BlitMipLevel);
               float3 maxOutline = col.rgb;
               float expectedStrength = lerp(0.0, _OutlineSize * _BlitTexture_TexelSize.x, _OutlineStrength);
               for (int x = -_OutlineSize; x <= _OutlineSize; x++)
               {
                   for (int y = -_OutlineSize; y <= _OutlineSize; y++)
                   {
                       float2 offset = float2(x, y) * _BlitTexture_TexelSize.xy;
                       float d = length(uv - (uv + offset));
                       if(d <= expectedStrength)
                       {
                           float4 fragOutline = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, saturate(uv + offset), _BlitMipLevel);
                           maxOutline = max(fragOutline.rgb, maxOutline);
                       }
                   }
               }
               return float4(maxOutline.rgbr);
           }

           ENDHLSL
       }
   }
}