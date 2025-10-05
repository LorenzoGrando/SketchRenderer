using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{ 
    public class DepthNormalsSilhouetteRenderPass : EdgeDetectionRenderPass
    {
        public override string PassName => "DepthNormalsSilhouette";
        
        private LocalKeyword edgeSobel3x3Keyword;
        private LocalKeyword edgeSobel1x3Keyword;
        
        private LocalKeyword sourceDepthKeyword;
        private LocalKeyword sourceDepthNormalsKeyword;

        public override void ConfigureMaterial()
        {
            base.ConfigureMaterial();
            
            edgeSobel3x3Keyword = new LocalKeyword(edgeDetectionMaterial.shader, EdgeDetectionGlobalData.SOBEL_3X3_KEYWORD);
            edgeSobel1x3Keyword = new LocalKeyword(edgeDetectionMaterial.shader, EdgeDetectionGlobalData.SOBEL_1X3_KEYWORD);
            
            sourceDepthKeyword = new LocalKeyword(edgeDetectionMaterial.shader, EdgeDetectionGlobalData.DEPTH_KEYWORD);
            sourceDepthNormalsKeyword = new LocalKeyword(edgeDetectionMaterial.shader, EdgeDetectionGlobalData.DEPTH_NORMALS_KEYWORD);

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

            switch (passData.Source)
            {
                //case EdgeDetectionGlobalData.EdgeDetectionSource.COLOR:
                    //break;
                case EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH:
                    ConfigureInput(ScriptableRenderPassInput.Depth);
                    edgeDetectionMaterial.EnableKeyword(sourceDepthKeyword);
                    edgeDetectionMaterial.DisableKeyword(sourceDepthNormalsKeyword);
                    break;
                case EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH_NORMALS:
                    ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
                    edgeDetectionMaterial.DisableKeyword(sourceDepthKeyword);
                    edgeDetectionMaterial.EnableKeyword(sourceDepthNormalsKeyword);
                    break;
            }
        }
        
        

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            var sketchData = frameData.GetOrCreate<SketchFrameData>();

            var dstDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            dstDesc.name = IsSecondary ? OUTLINE_SECONDARY_TEXTURE_NAME : OUTLINE_TEXTURE_NAME;
            dstDesc.format = GraphicsFormat.R16G16B16A16_SFloat;
            dstDesc.clearBuffer = true;
            dstDesc.msaaSamples = MSAASamples.None;
            dstDesc.enableRandomWrite = true;

            TextureHandle dst = renderGraph.CreateTexture(dstDesc);
            TextureHandle ping = renderGraph.CreateTexture(dstDesc);
            
            //Pass 0 = Sobel Horizontal Pass
            using (var builder = renderGraph.AddRasterRenderPass(PassName + "_Horizontal", out BlitPassData blitPassData))
            {
                if (passData.Method == EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_1X3 ||
                    passData.Method == EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_3X3)
                {
                    blitPassData.mat = edgeDetectionMaterial;
                    builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
                    blitPassData.src = resourceData.activeColorTexture;
                    blitPassData.passID = 0;
                    
                    builder.SetRenderAttachment(ping, 0, AccessFlags.ReadWrite);
                    builder.SetRenderFunc((BlitPassData passData, RasterGraphContext context) => ExecuteSobelEdgePass(passData, context));
                }
            }
            
            //Pass 1 = Sobel Vertical Pass
            using (var builder = renderGraph.AddRasterRenderPass(PassName + "_Vertical", out BlitPassData blitPassData))
            {
                if (passData.Method == EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_1X3 ||
                    passData.Method == EdgeDetectionGlobalData.EdgeDetectionMethod.SOBEL_3X3)
                {
                    blitPassData.mat = edgeDetectionMaterial;
                    builder.UseTexture(ping, AccessFlags.Read);
                    blitPassData.src = ping;
                    blitPassData.passID = 1;
                    
                    builder.SetRenderAttachment(dst, 0, AccessFlags.ReadWrite);
                    builder.SetRenderFunc((BlitPassData passData, RasterGraphContext context) => ExecuteSobelEdgePass(passData, context));
                }
            }

            if (!IsSecondary)
                sketchData.OutlinesTexture = dst;
            else
                sketchData.OutlinesSecondaryTexture = dst;
        }
    }
}