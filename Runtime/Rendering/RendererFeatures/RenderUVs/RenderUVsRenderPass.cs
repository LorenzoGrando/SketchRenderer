using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class RenderUVsRenderPass : ScriptableRenderPass, ISketchRenderPass<RenderUVsPassData>
    {
        public string PassName => "RenderUVsPass";

        private RenderUVsPassData passData;
        private Material uvsMaterial;

        private readonly int ROTATION_MATRIX_ID = Shader.PropertyToID("_SkyboxRotationMatrix");
        private readonly string ROTATE_SKYBOX_ID = "ROTATE_SKYBOX";
        
        private LocalKeyword RotateSkyboxKeyword;
        
        // Scale bias is used to control how the blit operation is done. The x and y parameter controls the scale
        // and z and w controls the offset.
        static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

        public void Setup(RenderUVsPassData passData, Material mat)
        {
            this.passData = passData;
            uvsMaterial = mat;
            
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            requiresIntermediateTexture = true;
            
            ConfigureMaterial();
        }

        public void ConfigureMaterial()
        {
            RotateSkyboxKeyword = new LocalKeyword(uvsMaterial.shader, ROTATE_SKYBOX_ID);

            if (passData.ShouldRotate)
            {
                uvsMaterial.SetKeyword(RotateSkyboxKeyword, true);
                uvsMaterial.SetMatrix(ROTATION_MATRIX_ID, passData.SkyboxRotationMatrix);
            }
            else
                uvsMaterial.SetKeyword(RotateSkyboxKeyword, false);
        }

        public void Dispose()
        {
        }

        private class PassData
        {
            public Material material;
            public TextureHandle src;
            public RendererListHandle rendererList;
        }

        private static void RenderUVs(PassData passData, RasterGraphContext context)
        {
            context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.black, 1,0);
            //This just fills the texture with screenspace uvs, used by skybox.
            Blitter.BlitTexture(context.cmd, passData.src, scaleBias, passData.material, 1);
            context.cmd.DrawRendererList(passData.rendererList);
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName, out var passData))
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                if (resourceData.isActiveTargetBackBuffer)
                    return;
                
                var renderingData = frameData.Get<UniversalRenderingData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();
                var sortFlags = cameraData.defaultOpaqueSortFlags;
                var renderQueueRange = RenderQueueRange.opaque;
                var filterSettings = new FilteringSettings(renderQueueRange, ~0);

                TextureDesc desc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
                desc.name = SketchGlobalFrameData.ScreenUVTexture.TextureName;
                desc.format = GraphicsFormat.R32G32_SFloat;
                desc.msaaSamples = MSAASamples.None;
                TextureHandle dst = renderGraph.CreateTexture(desc);
                
                if (dst.IsValid())
                {
                    ShaderTagId shaderOverrides = new ShaderTagId("UniversalForward");
                    DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(shaderOverrides, renderingData, cameraData, lightData, sortFlags);
                    drawingSettings.overrideMaterial = uvsMaterial;
                    var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filterSettings);
                    
                    passData.rendererList = renderGraph.CreateRendererList(rendererListParams);
                    passData.material = uvsMaterial;
                    passData.src = dst;
                    builder.UseRendererList(passData.rendererList);
                    builder.SetRenderAttachment(dst, 0, AccessFlags.Write);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => RenderUVs(data, context));
                    builder.SetRenderFunc<PassData>(RenderUVs);
                    builder.SetGlobalTextureAfterPass(dst, SketchGlobalFrameData.ScreenUVTexture.GetUVTextureID);
                }
            }
        }
    }
}