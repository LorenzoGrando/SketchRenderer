Shader "SketchRenderer/SketchComposition"
{
    SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "Final Sketch Composition"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
           #include "Packages/com.lorenzogrando.sketchrenderer/ShaderLibrary/BlendingOperations/BlendingOperations.hlsl"
           
           #pragma vertex Vert
           #pragma fragment Frag

           #pragma multi_compile_local_fragment _ DEBUG_MATERIAL_ALBEDO DEBUG_MATERIAL_DIRECTION DEBUG_OUTLINES DEBUG_LUMINANCE
           #pragma multi_compile_local_fragment BLEND_MULTIPLY BLEND_SCREEN BLEND_ADD BLEND_SUBTRACT
           #pragma multi_compile_local_fragment _ HAS_MATERIAL
           #pragma multi_compile_local_fragment _ HAS_LUMINANCE
           #pragma multi_compile_local_fragment _ HAS_OUTLINES

           Texture2D _MaterialTex;
           Texture2D _DirectionalTex;
           Texture2D _OutlineTex;
           Texture2D _LuminanceTex;

           float4 _OutlineColor;
           float4 _ShadingColor;

           float _MaterialAccumulationStrength;
           float4 _LuminanceBasisDirection;

           float _BlendStrength;
           
           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               float2 uv = input.texcoord;
               
               float4 material = 0;
               float4 direction = 0;
               float4 outline = 0;
               float4 luminance = 1.0;

                //Handled in render pass. If has material, will be materialprojection; Otherwise its the color buffer.
               material = SAMPLE_TEXTURE2D_X_LOD(_MaterialTex, sampler_LinearClamp, uv, _BlitMipLevel);
               #if defined(HAS_MATERIAL)
               direction = (SAMPLE_TEXTURE2D_X_LOD(_DirectionalTex, sampler_LinearClamp, uv, _BlitMipLevel) - 0.5) * 2.0;
               #endif

               #if defined(HAS_OUTLINES)
               outline = SAMPLE_TEXTURE2D_X_LOD(_OutlineTex, sampler_LinearClamp, uv, _BlitMipLevel);
               #endif

               #if defined(HAS_LUMINANCE)
               luminance = SAMPLE_TEXTURE2D_X_LOD(_LuminanceTex, sampler_LinearClamp, uv, _BlitMipLevel);
               #endif

               #if defined (DEBUG_MATERIAL_ALBEDO)
               return float4(material.rgb, 1);
               #elif defined (DEBUG_MATERIAL_DIRECTION)
               return float4(direction.rgb * _MaterialAccumulationStrength, 1);
               #elif defined(DEBUG_OUTLINES)
               float4 debugOutline = 1 - outline;
               return float4(debugOutline.rrr, outline.a);
               #elif defined(DEBUG_LUMINANCE)
               return float4(luminance.rgb, 1);
               #endif
               float4 white = float4(1,1,1,1);

               float isOutlineStroke = step(0.1, outline.a);
               #if defined(HAS_OUTLINES)
               float4 outlineDirection = (float4(outline.gb, 0, 0) - 0.5) * 2.0;
               float outlineAccumulation = 0;
               #if defined(HAS_MATERIAL)
               outlineAccumulation = 1.0 - dot(outlineDirection.rg, direction.rg);
               #endif
               outline = float4(outline.rgb, saturate((outline.a * _OutlineColor.a) + lerp(0, _MaterialAccumulationStrength, outlineAccumulation * isOutlineStroke)));
               float maxOut = max(outline.r, max(outline.g, outline.g));
               float4 outlineShade = float4(maxOut * _OutlineColor.rgb, outline.a);
               outline = lerp(white, outlineShade, isOutlineStroke * outline.a);
               
               #endif
                
               float isLuminanceStroke = (1 - luminance.a);
               #if defined(HAS_LUMINANCE)
               float luminanceAccumulation = 0;
               #if defined(HAS_MATERIAL)
               luminanceAccumulation = 1.0 - dot(_LuminanceBasisDirection.rg, direction.rg);
               #endif
               luminance = float4(1.0 - luminance.rgb, saturate(((1.0 - luminance.a) * _ShadingColor.a) + lerp(0, _MaterialAccumulationStrength, luminanceAccumulation * isLuminanceStroke)));
               float4 lumShade = float4(luminance.rgb * _ShadingColor.rgb, luminance.a);
               luminance = lerp(white, lumShade, isLuminanceStroke * luminance.a);
               #endif
        
               float isAnyStroke = saturate(isOutlineStroke + isLuminanceStroke);
               //TODO: Make this an option
               //Outlines always on top
               //float3 blend = (outline * outline.a * isOutlineStroke) + luminance * (1.0 - outline.a * isOutlineStroke);
               
               float4 blendOutline = lerp(white, outline, outline.a);
               float4 blend = BLEND_0P_MULTIPLY(luminance, blendOutline);
               //return blend;
               float3 materialBlend = BLENDING_OPERATION(material.rgba, blend).rgb;
               //First, apply an attenuation to the blend effect
               materialBlend = lerp(material, materialBlend, _BlendStrength);
               //Then blend only if a stroke
               materialBlend = lerp(material, materialBlend, isAnyStroke);
               
               return float4(materialBlend.rgb, 1);
           }

           ENDHLSL
       }
   }
}