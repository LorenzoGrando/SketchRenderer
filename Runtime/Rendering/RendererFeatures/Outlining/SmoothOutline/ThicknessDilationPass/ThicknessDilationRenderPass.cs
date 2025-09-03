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
            
            RenderGraphUtils.BlitMaterialParameters thickenParams = new RenderGraphUtils.BlitMaterialParameters(sketchData.OutlinesTexture, dst, dilationMaterial, 0);
            renderGraph.AddBlitPass(thickenParams, PassName);
            sketchData.OutlinesTexture = dst;
        }
    }
}