using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class EdgeCompositorRenderPass : ScriptableRenderPass
    {
        public string PassName => "EdgeCompositor";
        
        private Material compositeMaterial;
        
        private static readonly int primaryTextureShaderID = Shader.PropertyToID("_PrimaryEdgeTex");
        private static readonly int secondaryTextureShaderID = Shader.PropertyToID("_SecondaryEdgeTex");
        
        // Scale bias is used to control how the blit operation is done. The x and y parameter controls the scale
        // and z and w controls the offset. TAKEN FROM URPSAMPLES
        static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);
        
        public virtual void Setup(Material mat)
        {
            compositeMaterial = mat;
            
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            requiresIntermediateTexture = false;
        }

        class PassData
        {
            public Material material;
            public TextureHandle primaryEdgeTexture;
            public TextureHandle secondaryEdgeTexture;
            public TextureHandle dst;
        }

        private static void ExecuteEdgeComposition(PassData passData, RasterGraphContext context)
        {
            passData.material.SetTexture(primaryTextureShaderID, passData.primaryEdgeTexture);
            passData.material.SetTexture(secondaryTextureShaderID, passData.secondaryEdgeTexture);
            Blitter.BlitTexture(context.cmd, passData.dst, scaleBias, passData.material, 0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName, out PassData passData))
            {
                var resourceData = frameData.Get<UniversalResourceData>();

                if (resourceData.isActiveTargetBackBuffer)
                    return;

                var sketchData = frameData.GetOrCreate<SketchFrameData>();

                var dstDesc = renderGraph.GetTextureDesc(sketchData.OutlinesTexture);
                dstDesc.name = "CompositeOutlineTexture";
                dstDesc.format = GraphicsFormat.R8G8B8A8_UNorm;
                dstDesc.clearBuffer = true;
                dstDesc.msaaSamples = MSAASamples.None;
                dstDesc.enableRandomWrite = true;

                TextureHandle dst = renderGraph.CreateTexture(dstDesc);
                
                builder.UseTexture(sketchData.OutlinesTexture);
                passData.primaryEdgeTexture = sketchData.OutlinesTexture;
                builder.UseTexture(sketchData.OutlinesSecondaryTexture);
                passData.secondaryEdgeTexture = sketchData.OutlinesSecondaryTexture;
                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);
                passData.dst = dst;
                passData.material = compositeMaterial;
                
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteEdgeComposition(data, context));
                
                sketchData.OutlinesTexture = dst;
            }
        }
    }
}