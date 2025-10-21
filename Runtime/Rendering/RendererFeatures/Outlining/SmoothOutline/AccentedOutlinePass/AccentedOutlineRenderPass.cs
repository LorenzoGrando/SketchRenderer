using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class AccentedOutlineRenderPass : ScriptableRenderPass, ISketchRenderPass<AccentedOutlinePassData>
    {
        public string PassName => "AccentedOutlinesPass";
        
        private Material accentedMaterial;
        private AccentedOutlinePassData passData;
        
        private RTHandle bakedDistortionTexture;
        private RTHandle bakedDistortionTexture2;

        protected static readonly int bakedDistortionTexShaderID = Shader.PropertyToID("_BakedUVDistortionTex");
        protected static readonly int bakedDistortionTex2ShaderID = Shader.PropertyToID("_BakedUVDistortionTex2");
        protected static readonly int distortionRateShaderID = Shader.PropertyToID("_DistortionRate");
        protected static readonly int distortionStrengthShaderID = Shader.PropertyToID("_DistortionStrength");
        
        protected static readonly int additionalLinesShaderID = Shader.PropertyToID("_AdditionalLines");
        protected static readonly int additionalLinesOffsetShaderID = Shader.PropertyToID("_DistortionOffset");
        protected static readonly int additionalLinesSeedShaderID = Shader.PropertyToID("_DistortionFlatSeed");
        protected static readonly int additionalLinesTintShaderID = Shader.PropertyToID("_LineTintFalloff");
        protected static readonly int additionalLinesStrengthShaderID = Shader.PropertyToID("_LineStrengthJitter");
        
        protected static readonly int outlineMaskShaderID = Shader.PropertyToID("_OutlineMaskTex");
        
        protected static readonly string DISTORT_OUTLINE_KEYWORD = "DISTORT_OUTLINES";
        protected static readonly string BAKE_DISTORT_OUTLINE_KEYWORD = "BAKED_DISTORT_OUTLINES";
        protected static readonly string MULTIPLE_DISTORT_OUTLINE_KEYWORD = "MULTIPLE_DISTORTIONS";
        protected static readonly string MASK_OUTLINE_KEYWORD = "MASK_OUTLINES";

        private const float OFFSET_IN_MULTIPLE_TEXTURE = 100f;
        private const float SEED_IN_MULTIPLE_LINES = 20f;
        
        private LocalKeyword DistortionKeyword;
        private LocalKeyword BakeDistortionKeyword;
        private LocalKeyword MultipleDistortionKeyword;
        private LocalKeyword MaskKeyword;
        
        // Scale bias is used to control how the blit operation is done. The x and y parameter controls the scale
        // and z and w controls the offset.
        static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

        public void Setup(AccentedOutlinePassData passData, Material mat)
        {
            this.passData = passData;
            accentedMaterial = mat;
            
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            requiresIntermediateTexture = false;
            
            ConfigureMaterial();
        }

        public void ConfigureMaterial()
        {
            DistortionKeyword = new LocalKeyword(accentedMaterial.shader, DISTORT_OUTLINE_KEYWORD);
            BakeDistortionKeyword = new LocalKeyword(accentedMaterial.shader, BAKE_DISTORT_OUTLINE_KEYWORD);
            MultipleDistortionKeyword = new LocalKeyword(accentedMaterial.shader, MULTIPLE_DISTORT_OUTLINE_KEYWORD);
            MaskKeyword = new LocalKeyword(accentedMaterial.shader, MASK_OUTLINE_KEYWORD);
            
            accentedMaterial.SetFloat(distortionRateShaderID, passData.Rate);
            //here we interpret 0 as disabled, and 1 as a clamped value (since high values destroy the effect)
            accentedMaterial.SetFloat(distortionStrengthShaderID, Mathf.Lerp(0f, 0.01f, passData.Strength));
            accentedMaterial.SetTexture(outlineMaskShaderID, passData.PencilOutlineMask);
            accentedMaterial.SetTextureScale(outlineMaskShaderID, passData.MaskScale);
            
            accentedMaterial.SetKeyword(BakeDistortionKeyword, passData.BakeDistortionDuringRuntime);
            accentedMaterial.SetKeyword(DistortionKeyword, passData.Strength > 0 && !passData.BakeDistortionDuringRuntime);
            
            accentedMaterial.SetTexture(bakedDistortionTexShaderID, bakedDistortionTexture);
            accentedMaterial.SetTexture(bakedDistortionTex2ShaderID, bakedDistortionTexture2);

            accentedMaterial.SetKeyword(MaskKeyword, passData.PencilOutlineMask != null && passData.MaskScale != Vector2.zero);
            
            accentedMaterial.SetKeyword(MultipleDistortionKeyword, passData.RequireMultipleTextures);
            accentedMaterial.SetInt(additionalLinesShaderID, passData.AdditionalLines);
            accentedMaterial.SetFloat(additionalLinesOffsetShaderID, OFFSET_IN_MULTIPLE_TEXTURE);
            accentedMaterial.SetFloat(additionalLinesSeedShaderID, SEED_IN_MULTIPLE_LINES);
            accentedMaterial.SetFloat(additionalLinesTintShaderID, passData.AdditionalLineTintPersistence);
            accentedMaterial.SetFloat(additionalLinesStrengthShaderID, passData.AdditionalLineDistortionJitter);
        }
        
        private Vector2Int GetTargetBakedResolution(TextureDesc desc, Vector2 scaleFactor) => new Vector2Int(Mathf.RoundToInt(desc.width * scaleFactor.x), Mathf.RoundToInt(desc.height * scaleFactor.y));

        private bool ReValidateBakedTextures(TextureDesc desc)
        {
            Vector2Int resolution = GetTargetBakedResolution(desc, Vector2.one * passData.BakedTextureScaleFactor);
                
            return bakedDistortionTexture == null || (bakedDistortionTexture != null && (bakedDistortionTexture.rt.width != resolution.x || bakedDistortionTexture.rt.height != resolution.y));
        }

        public void ConfigureBakedTextures(TextureDesc desc, bool forceBake = false)
        {
            if (this.passData.BakeDistortionDuringRuntime)
            {
                Vector2Int resolution = GetTargetBakedResolution(desc, Vector2.one * passData.BakedTextureScaleFactor);
                

                if (bakedDistortionTexture == null || passData.ForceRebake || forceBake)
                {
                    if (bakedDistortionTexture != null)
                        bakedDistortionTexture.Release();
                    bakedDistortionTexture = RTHandles.Alloc(resolution.x, resolution.y, GraphicsFormat.R8G8B8A8_UNorm, enableRandomWrite: true, name: "_BakedUVDistortionTex");
                    accentedMaterial.SetTexture(bakedDistortionTexShaderID, bakedDistortionTexture);
                }
                //Only rebuild this if it is being used
                if (this.passData.RequireMultipleTextures && (bakedDistortionTexture2 == null || passData.ForceRebake || forceBake))
                {
                    if(bakedDistortionTexture2 != null)
                        bakedDistortionTexture2.Release();
                    bakedDistortionTexture2 = RTHandles.Alloc(resolution.x, resolution.y, GraphicsFormat.R8G8B8A8_UNorm, enableRandomWrite: true, name: "_BakedDistortionTex2");
                    accentedMaterial.SetTexture(bakedDistortionTex2ShaderID, bakedDistortionTexture2);
                }
            }
            
            if (!this.passData.BakeDistortionDuringRuntime && (bakedDistortionTexture != null || bakedDistortionTexture2 != null))
            {
                if (bakedDistortionTexture != null)
                {
                    bakedDistortionTexture.Release();
                    bakedDistortionTexture = null;
                }

                if (bakedDistortionTexture2 != null)
                {
                    bakedDistortionTexture2.Release();
                    bakedDistortionTexture2 = null;
                }
            }
        }
        
        public void Dispose()
        {
            if (bakedDistortionTexture != null)
            {
                bakedDistortionTexture.Release();
                bakedDistortionTexture = null;
            }

            if (bakedDistortionTexture2 != null)
            {
                bakedDistortionTexture2.Release();
                bakedDistortionTexture2 = null;
            }
        }
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
            if(!passData.ActiveInVolume())
                return;
            
            var resourceData = frameData.Get<UniversalResourceData>();
            
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            var sketchData = frameData.Get<SketchFrameData>();
            if(sketchData == null)
                return;
            
            TextureDesc dstDesc = renderGraph.GetTextureDesc(sketchData.OutlinesTexture);
            dstDesc.name = "AccentedOutlines";
            dstDesc.clearBuffer = true;
            dstDesc.msaaSamples = MSAASamples.None;

            if (passData.ForceRebake || ReValidateBakedTextures(dstDesc))
            {
                sketchData.PrebakedDistortedUVs = false;
                sketchData.PrebakedDistortedMultipleUVs = false;
                passData.ForceRebake = false;
            }

            bool shouldRebakeIfSingle = !passData.RequireMultipleTextures && !sketchData.PrebakedDistortedUVs;
            bool shouldRebakeIfMultiple = passData.RequireMultipleTextures && !sketchData.PrebakedDistortedMultipleUVs;
            
            if(passData.BakeDistortionDuringRuntime && (shouldRebakeIfSingle || shouldRebakeIfMultiple))
                ConfigureBakedTextures(dstDesc, true);
            
            if (passData.BakeDistortionDuringRuntime && (shouldRebakeIfSingle || shouldRebakeIfMultiple))
            {
                using (var builder = renderGraph.AddRasterRenderPass(PassName + "_BakeUVDistortion", out BlitPassData blitPassData))
                {
                    ImportResourceParams importParams = new ImportResourceParams();
                    TextureHandle distortedDst = renderGraph.ImportTexture(bakedDistortionTexture, importParams);
                    builder.UseTexture(sketchData.OutlinesTexture, AccessFlags.Read);
                    blitPassData.mat = accentedMaterial;
                    blitPassData.src = sketchData.OutlinesTexture;
                    blitPassData.passID = 1;
                    builder.SetRenderAttachment(distortedDst, 0);
                    builder.SetRenderFunc((BlitPassData passData, RasterGraphContext context) => ExecuteBlitPass(passData, context));
                }

                if (shouldRebakeIfMultiple)
                {
                    using (var builder = renderGraph.AddRasterRenderPass(PassName + "_BakeUVDistortion2", out BlitPassData blitPassData))
                    {
                        ImportResourceParams importParams = new ImportResourceParams();
                        TextureHandle distortedDst2 = renderGraph.ImportTexture(bakedDistortionTexture2, importParams);
                        builder.UseTexture(sketchData.OutlinesTexture, AccessFlags.Read);
                        blitPassData.mat = accentedMaterial;
                        blitPassData.src = sketchData.OutlinesTexture;
                        blitPassData.passID = 1;
                        builder.SetRenderAttachment(distortedDst2, 0);
                        builder.SetRenderFunc((BlitPassData passData, RasterGraphContext context) => ExecuteBlitPass(passData, context));
                    }
                }
                
                sketchData.PrebakedDistortedUVs = shouldRebakeIfSingle;
                sketchData.PrebakedDistortedMultipleUVs = shouldRebakeIfMultiple;
            }
            else if (!passData.BakeDistortionDuringRuntime && (sketchData.PrebakedDistortedUVs || sketchData.PrebakedDistortedMultipleUVs))
            {
                sketchData.PrebakedDistortedUVs = false;
                sketchData.PrebakedDistortedMultipleUVs = false;
            }


            TextureHandle dst = renderGraph.CreateTexture(dstDesc);
            using (var builder = renderGraph.AddRasterRenderPass(PassName, out BlitPassData blitPassData))
            {
                builder.UseTexture(sketchData.OutlinesTexture, AccessFlags.Read);
                blitPassData.mat = accentedMaterial;
                blitPassData.src = sketchData.OutlinesTexture;
                blitPassData.passID = 0;
                
                //This shader has a single pass that handles all behaviours (distortion, outline brightness and texture masking) by compile keywords
                builder.SetRenderAttachment(dst, 0);
                builder.SetRenderFunc((BlitPassData passData, RasterGraphContext context) => ExecuteBlitPass(passData, context));
            }

            sketchData.OutlinesTexture = dst;
        }
    }
}