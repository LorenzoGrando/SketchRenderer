using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class ThicknessDilationRenderPass : ScriptableRenderPass, ISketchRenderPass<ThicknessDilationPassData>
    {
        public string PassName => "ThickenOutlinesPass";
        
        private Material dilationMaterial;
        private ThicknessDilationPassData passData;
        
        private static readonly int outlineSizeShaderID = Shader.PropertyToID("_OutlineSize");
        private static readonly int outlineStrengthShaderID = Shader.PropertyToID("_OutlineStrength");
        
        // Scale bias is used to control how the blit operation is done. The x and y parameter controls the scale
        // and z and w controls the offset.
        static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

        public void Setup(ThicknessDilationPassData passData, Material mat)
        {
            this.passData = passData;
            dilationMaterial = mat;
            
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            requiresIntermediateTexture = false;
            
            ConfigureMaterial();
        }

        public void ConfigureMaterial()
        {
            dilationMaterial.SetInteger(outlineSizeShaderID, passData.ThicknessRange);
            dilationMaterial.SetFloat(outlineStrengthShaderID, passData.ThicknessStrength);
        }

        public void Dispose() {}
        
        private class BlitPassData
        {
            public Material mat;
            public TextureHandle src;
            public int passID;
        }

        private static void ExecuteBlitPass(BlitPassData passData, RasterGraphContext context)
        {
            Blitter.BlitTexture(context.cmd, passData.src, scaleBias, passData.mat, passData.passID);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            var sketchData = frameData.Get<SketchFrameData>();
            if(sketchData == null)
                return;
            
            var dstDesc = renderGraph.GetTextureDesc(sketchData.OutlinesTexture);
            dstDesc.name = "ThickenedOutlines";
            dstDesc.clearBuffer = true;
            dstDesc.msaaSamples = MSAASamples.None;
                
            TextureHandle dst = renderGraph.CreateTexture(dstDesc);

            using (var builder = renderGraph.AddRasterRenderPass(PassName, out BlitPassData blitPassData))
            {
                builder.UseTexture(sketchData.OutlinesTexture);
                blitPassData.mat = dilationMaterial;
                blitPassData.passID = 0;
                blitPassData.src = sketchData.OutlinesTexture;
                
                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);
                builder.SetRenderFunc((BlitPassData passData, RasterGraphContext context) => ExecuteBlitPass(passData, context));
            }
            sketchData.OutlinesTexture = dst;
        }
    }
}