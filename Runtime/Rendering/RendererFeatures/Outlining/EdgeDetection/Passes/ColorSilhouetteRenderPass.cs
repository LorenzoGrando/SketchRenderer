using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class ColorSilhouetteRenderPass : EdgeDetectionRenderPass
    {
        public override string PassName => "ColorBufferSilhouette";
        
        private LocalKeyword edgeSobel3x3Keyword;
        private LocalKeyword edgeSobel1x3Keyword;

        public override void ConfigureMaterial()
        {
            base.ConfigureMaterial();
            
            edgeSobel3x3Keyword = new LocalKeyword(edgeDetectionMaterial.shader, EdgeDetectionGlobalData.SOBEL_3X3_KEYWORD);
            edgeSobel1x3Keyword = new LocalKeyword(edgeDetectionMaterial.shader, EdgeDetectionGlobalData.SOBEL_1X3_KEYWORD);

            switch (passData.Method)
            {
                case EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_3X3:
                    edgeDetectionMaterial.EnableKeyword(edgeSobel3x3Keyword);
                    edgeDetectionMaterial.DisableKeyword(edgeSobel1x3Keyword);
                    break;
                case EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_1X3:
                    edgeDetectionMaterial.DisableKeyword(edgeSobel3x3Keyword);
                    edgeDetectionMaterial.EnableKeyword(edgeSobel1x3Keyword);
                    break;
            }

            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            
            if (resourceData.isActiveTargetBackBuffer)
                return;

            var sketchData = frameData.GetOrCreate<SketchResourceData>();
            
            var dstDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            dstDesc.name = IsSecondary ? OUTLINE_SECONDARY_TEXTURE_NAME : OUTLINE_TEXTURE_NAME;
            dstDesc.format = GraphicsFormat.R8G8B8A8_UNorm;
            dstDesc.clearBuffer = true;
            dstDesc.msaaSamples = MSAASamples.None;
            dstDesc.enableRandomWrite = true;

            TextureHandle dst = renderGraph.CreateTexture(dstDesc);
            TextureHandle ping = renderGraph.CreateTexture(dstDesc);

            if (passData.Method == EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_1X3 || passData.Method == EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_3X3)
            {
                //Pass 0 = Sobel Horizontal Pass
                RenderGraphUtils.BlitMaterialParameters horParams = new(dst, ping, edgeDetectionMaterial, 0);
                renderGraph.AddBlitPass(horParams, passName: PassName + "_SobelHorizontal");
                //Pass 1 = Sobel Vertical Pass
                RenderGraphUtils.BlitMaterialParameters verParams = new(ping, dst, edgeDetectionMaterial, 1);
                renderGraph.AddBlitPass(verParams, passName: PassName + "_SobelVertical");
            }
            
            if(!IsSecondary)
                sketchData.OutlinesTexture = dst;
            else
                sketchData.OutlinesSecondaryTexture = dst;
        }
    }
}