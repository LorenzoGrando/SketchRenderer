using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using SketchRenderer.Runtime.Extensions;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class MaterialSurfaceRenderPass : ScriptableRenderPass, ISketchRenderPass<MaterialSurfacePassData>
    {
        public string PassName {
            get { return " MaterialSurfaceRenderPass"; }
        }

        private Material materialMat;
        private MaterialSurfacePassData passData;

        public readonly int ALBEDO_PASS_ID = 0;
        public readonly int DIRECTIONAL_PASS_ID = 1;
        
        private readonly int ALBEDO_TEXTURE_ID = Shader.PropertyToID("_MaterialAlbedoTex");
        private readonly int ALBEDO_TEXTURE_TEXEL_ID = Shader.PropertyToID("_MaterialAlbedoTex_TexelSize");
        private readonly int NORMAL_TEXTURE_ID = Shader.PropertyToID("_MaterialDirectionalTex");
        private readonly int NORMAL_TEXTURE_TEXEL_ID = Shader.PropertyToID("_MaterialDirectionalTex_TexelSize");
        private readonly int TEXTURE_SCALE_ID = Shader.PropertyToID("_TextureScales");
        private readonly int COLOR_BLEND_ID = Shader.PropertyToID("_BlendStrength");
        
        private LocalKeyword UVsScreenSpaceKeyword;
        private LocalKeyword UVsObjectSpaceKeyword;
        private LocalKeyword UVsObjectSpaceConstantKeyword;
        private LocalKeyword UVsObjectSpaceReversedConstantKeyword;
        
        // Scale bias is used to control how the blit operation is done. The x and y parameter controls the scale
        // and z and w controls the offset.
        static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);
        
        public void Setup(MaterialSurfacePassData passData, Material mat)
        {
            materialMat = mat;
            this.passData = passData;
            
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            requiresIntermediateTexture = true;
            
            ConfigureInput(ScriptableRenderPassInput.Color);
            
            ConfigureMaterial();
        }

        public void ConfigureMaterial()
        {
            UVsScreenSpaceKeyword = new LocalKeyword(materialMat.shader, TextureProjectionGlobalData.UVS_SCREEN_SPACE_KEYWORD);
            UVsObjectSpaceKeyword = new LocalKeyword(materialMat.shader, TextureProjectionGlobalData.UVS_OBJECT_SPACE_KEYWORD);
            UVsObjectSpaceConstantKeyword = new LocalKeyword(materialMat.shader, TextureProjectionGlobalData.UVS_OBJECT_SPACE_CONSTANT_KEYWORD);
            UVsObjectSpaceReversedConstantKeyword = new LocalKeyword(materialMat.shader, TextureProjectionGlobalData.UVS_OBJECT_SPACE_REVERSED_CONSTANT_KEYWORD);

            switch (passData.ProjectionMethod)
            {
                case TextureProjectionGlobalData.TextureProjectionMethod.SCREEN_SPACE:
                    materialMat.SetKeyword(UVsScreenSpaceKeyword, true);
                    materialMat.SetKeyword(UVsObjectSpaceKeyword, false);
                    materialMat.SetKeyword(UVsObjectSpaceConstantKeyword, false);
                    materialMat.SetKeyword(UVsObjectSpaceReversedConstantKeyword, false);
                    break;
                case TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE:
                    materialMat.SetKeyword(UVsScreenSpaceKeyword, false);
                    materialMat.SetKeyword(UVsObjectSpaceKeyword, true);
                    materialMat.SetKeyword(UVsObjectSpaceConstantKeyword, false);
                    materialMat.SetKeyword(UVsObjectSpaceReversedConstantKeyword, false);
                    break;
                case TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_CONSTANT_SCALE:
                    materialMat.SetKeyword(UVsScreenSpaceKeyword, false);
                    materialMat.SetKeyword(UVsObjectSpaceKeyword, false);
                    materialMat.SetKeyword(UVsObjectSpaceConstantKeyword, true);
                    materialMat.SetKeyword(UVsObjectSpaceReversedConstantKeyword, false);
                    break;
                case TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_REVERSED_CONSTANT_SCALE:
                    materialMat.SetKeyword(UVsScreenSpaceKeyword, false);
                    materialMat.SetKeyword(UVsObjectSpaceKeyword, false);
                    materialMat.SetKeyword(UVsObjectSpaceConstantKeyword, false);
                    materialMat.SetKeyword(UVsObjectSpaceReversedConstantKeyword, true);
                    break;
            }
            
            materialMat.SetFloat(TextureProjectionGlobalData.CONSTANT_SCALE_FALLOFF_SHADER_ID, passData.ConstantScaleFalloffFactor);
            
            materialMat.SetTexture(ALBEDO_TEXTURE_ID, passData.AlbedoTexture);
            materialMat.SetTexture(NORMAL_TEXTURE_ID, passData.NormalTexture);
            materialMat.SetVector(ALBEDO_TEXTURE_TEXEL_ID, passData.AlbedoTexture.GetTexelSize());
            materialMat.SetVector(NORMAL_TEXTURE_TEXEL_ID, passData.NormalTexture.GetTexelSize());
            
            materialMat.SetVector(TEXTURE_SCALE_ID, new Vector4(passData.Scale.x, passData.Scale.y, 0, 0));
            materialMat.SetFloat(COLOR_BLEND_ID, passData.BaseColorBlendFactor);
        }

        public void Dispose()
        {
            
        }

        private class PassData
        {
            public TextureHandle src;
            public TextureHandle dst;
            public int passID;
            public Material mat;
        }

        private static void ExecuteMaterial(PassData data, RasterGraphContext context)
        {
            Blitter.BlitTexture(context.cmd, data.src, scaleBias, data.mat, data.passID);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName, out var passData))
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                if(resourceData.isActiveTargetBackBuffer)
                    return;

                if (this.passData.ProjectionMethod 
                    is TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE
                    or TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_CONSTANT_SCALE
                    or TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_REVERSED_CONSTANT_SCALE)
                {
                    //Preemptive check to avoid exception from breaking blitter
                    try
                    {
                        builder.UseGlobalTexture(SketchGlobalFrameData.ScreenUVTexture.GetUVTextureID, AccessFlags.Read);
                    }
                    catch (System.Exception ex)
                    {
                        return;
                    }
                }

                var sketchData = frameData.GetOrCreate<SketchFrameData>();
                
                passData.mat = materialMat;

                //Projects Material Texture
                var dstDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
                dstDesc.name = "MaterialTexture";
                dstDesc.format = GraphicsFormat.R8G8B8A8_UNorm;
                dstDesc.clearBuffer = true;
                dstDesc.msaaSamples = MSAASamples.None;

                TextureHandle dst = renderGraph.CreateTexture(dstDesc);
                
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
                passData.src = resourceData.activeColorTexture;
                builder.SetRenderAttachment(dst, 0, AccessFlags.ReadWrite);
                passData.dst = dst;
                
                passData.passID = ALBEDO_PASS_ID;
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteMaterial(data, context));
                sketchData.MaterialTexture = dst;
            }

            using (var directionalBuilder = renderGraph.AddRasterRenderPass<PassData>(PassName + "_Directional", out var directionalPassData))
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                if(resourceData.isActiveTargetBackBuffer)
                    return;

                if (this.passData.ProjectionMethod 
                    is TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE
                    or TextureProjectionGlobalData.TextureProjectionMethod.OBJECT_SPACE_CONSTANT_SCALE)
                {
                    //Preemptive check to avoid exception from breaking blitter
                    try
                    {
                        directionalBuilder.UseGlobalTexture(SketchGlobalFrameData.ScreenUVTexture.GetUVTextureID, AccessFlags.Read);
                    }
                    catch (System.Exception ex)
                    {
                        return;
                    }
                }

                var sketchData = frameData.GetOrCreate<SketchFrameData>();
                
                directionalPassData.mat = materialMat;
                
                //Project Directional Texture
                var dstDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
                dstDesc.name = "MaterialDirectionalTexture";
                dstDesc.format = GraphicsFormat.R8G8B8A8_UNorm;
                dstDesc.clearBuffer = true;
                dstDesc.msaaSamples = MSAASamples.None;
                
                TextureHandle directionalDst = renderGraph.CreateTexture(dstDesc);
                
                directionalBuilder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
                directionalPassData.src = resourceData.activeColorTexture;
                directionalBuilder.SetRenderAttachment(directionalDst, 0, AccessFlags.ReadWrite);
                directionalPassData.dst = directionalDst;

                directionalPassData.passID = DIRECTIONAL_PASS_ID;
                directionalBuilder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteMaterial(data, context));
                sketchData.DirectionalTexture = directionalDst;
            }
        }
    }
}