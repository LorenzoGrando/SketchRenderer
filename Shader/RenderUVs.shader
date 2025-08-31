Shader "SketchRenderer/RenderUVs"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            Name "Render Object UVs"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 uvs : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uvs : TEXCOORD0;
            };
                        
            Varyings vert(Attributes i)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                Varyings o;
                
                VertexPositionInputs vertexPositions = GetVertexPositionInputs(i.positionOS.xyz);
                o.positionCS = vertexPositions.positionCS;

                o.uvs = i.uvs;
                
                return o;
            }
                        
            float4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                return float4(i.uvs, 0.0, 0.0);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "Render Skybox UVs"
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_local_fragment _ ROTATE_SKYBOX

            float4x4 _SkyboxRotationMatrix;
            
            float4 Frag(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float2 perspectiveProj = float2(unity_CameraProjection._11, unity_CameraProjection._22);
                float3 viewDir = -normalize(float3((uv * 2 - 1)/perspectiveProj, -1));
                float3 worldDir = normalize(mul((float3x3)UNITY_MATRIX_I_V, viewDir));
                //sphere uvs for skybox: https://gamedev.stackexchange.com/questions/114412/how-to-get-uv-coordinates-for-sphere-cylindrical-projection
                float sU = (atan2(worldDir.z, worldDir.x)/(2*PI)) + 0.5;
                float sV = 0.5 - asin(worldDir.y)/PI;
                uv = float2(sU, sV);
                #if defined(ROTATE_SKYBOX)
                uv = mul((float2x2)_SkyboxRotationMatrix, uv);
                #endif
                
                return float4(uv, 0.0, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}