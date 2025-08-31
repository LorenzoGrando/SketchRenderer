using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using SketchRenderer.ShaderLibrary;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class SketchCompositionRenderPass : ScriptableRenderPass, ISketchRenderPass<SketchCompositionPassData>
    { 
        public string PassName
        {
            get { return "SketchCompositionRenderPass"; }
        }
        
        protected static readonly int materialShaderID = Shader.PropertyToID("_MaterialTex");
        protected static readonly int materialDirectionalShaderID = Shader.PropertyToID("_DirectionalTex");
        protected static readonly int outlinesShaderID = Shader.PropertyToID("_OutlineTex");
        protected static readonly int luminanceShaderID = Shader.PropertyToID("_LuminanceTex");
        protected static readonly int outlineColorShaderID = Shader.PropertyToID("_OutlineColor");
        protected static readonly int shadingColorShaderID = Shader.PropertyToID("_ShadingColor");
        protected static readonly int materialAccumulationShaderID = Shader.PropertyToID("_MaterialAccumulationStrength");
        protected static readonly int luminanceBasisDirectionShaderID = Shader.PropertyToID("_LuminanceBasisDirection");
        protected static readonly int blendStrengthShaderID = Shader.PropertyToID("_BlendStrength");
        
        public static readonly string DEBUG_MATERIAL_ALBEDO = "DEBUG_MATERIAL_ALBEDO";
        public static readonly string DEBUG_MATERIAL_DIRECTION = "DEBUG_MATERIAL_DIRECTION";
        public static readonly string DEBUG_OUTLINES = "DEBUG_OUTLINES";
        public static readonly string DEBUG_LUMINANCE = "DEBUG_LUMINANCE";
        
        public static readonly string HAS_MATERIAL_KEYWORD_ID = "HAS_MATERIAL";
        public static readonly string HAS_OUTLINE_KEYWORD_ID = "HAS_OUTLINES";
        public static readonly string HAS_LUMINANCE_KEYWORD_ID = "HAS_LUMINANCE";

        private LocalKeyword debugMaterialAlbedoKeyword;
        private LocalKeyword debugMaterialDirectionKeyword;
        private LocalKeyword debugOutlinesKeyword;
        private LocalKeyword debugLuminanceKeyword;
        
        private LocalKeyword hasMaterialKeyword;
        private LocalKeyword hasOutlinesKeyword;
        private LocalKeyword hasLuminanceKeyword;
        
        private LocalKeyword[] blendingKeywords;
        
        // Scale bias is used to control how the blit operation is done. The x and y parameter controls the scale
        // and z and w controls the offset. TAKEN FROM URPSAMPLES
        static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

        private Material mat;
        private SketchCompositionPassData passData;

        private class PassData
        {
            public TextureHandle materialTexture;
            public TextureHandle directionalTexture;
            public TextureHandle outlineTexture;
            public TextureHandle luminanceTexture;
            public Vector4 luminanceBasisDirection;
            public Material material;
            public TextureHandle dst;
        }

        public void Setup(SketchCompositionPassData passData, Material material)
        {
            this.mat = material;
            this.passData = passData;
            
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            requiresIntermediateTexture = true;
            
            ConfigureMaterial();
        }

        public void ConfigureMaterial()
        {
            debugMaterialAlbedoKeyword = new LocalKeyword(mat.shader, DEBUG_MATERIAL_ALBEDO);
            debugMaterialDirectionKeyword = new LocalKeyword(mat.shader, DEBUG_MATERIAL_DIRECTION);
            debugOutlinesKeyword = new LocalKeyword(mat.shader, DEBUG_OUTLINES);
            debugLuminanceKeyword = new LocalKeyword(mat.shader, DEBUG_LUMINANCE);
            
            hasMaterialKeyword = new LocalKeyword(mat.shader, HAS_MATERIAL_KEYWORD_ID);
            hasOutlinesKeyword = new LocalKeyword(mat.shader, HAS_OUTLINE_KEYWORD_ID);
            hasLuminanceKeyword = new LocalKeyword(mat.shader, HAS_LUMINANCE_KEYWORD_ID);
            
            string[] blending = Enum.GetNames(typeof(BlendingOperations));
            blendingKeywords = new LocalKeyword[blending.Length];
            string selected = passData.StrokeBlendMode.ToString();
            for (int i = 0; i < blending.Length; i++)
            {
                LocalKeyword blendKeyword = new LocalKeyword(mat.shader, blending[i]);
                blendingKeywords[i] = blendKeyword;
                if(blending[i] == selected)
                    mat.EnableKeyword(blendKeyword);
                else
                    mat.DisableKeyword(blendKeyword);
            }
            
            switch (passData.debugMode)
            {
                case SketchCompositionPassData.DebugMode.NONE:
                    mat.DisableKeyword(debugMaterialAlbedoKeyword);
                    mat.DisableKeyword(debugMaterialDirectionKeyword);
                    mat.DisableKeyword(debugOutlinesKeyword);
                    mat.DisableKeyword(debugLuminanceKeyword);
                    break;
                case SketchCompositionPassData.DebugMode.MATERIAL_ALBEDO:
                    mat.EnableKeyword(debugMaterialAlbedoKeyword);
                    mat.DisableKeyword(debugMaterialDirectionKeyword);
                    mat.DisableKeyword(debugOutlinesKeyword);
                    mat.DisableKeyword(debugLuminanceKeyword);
                    break;
                case SketchCompositionPassData.DebugMode.MATERIAL_DIRECTION:
                    mat.DisableKeyword(debugMaterialAlbedoKeyword);
                    mat.EnableKeyword(debugMaterialDirectionKeyword);
                    mat.DisableKeyword(debugOutlinesKeyword);
                    mat.DisableKeyword(debugLuminanceKeyword);
                    break;
                case SketchCompositionPassData.DebugMode.OUTLINES:
                    mat.DisableKeyword(debugMaterialAlbedoKeyword);
                    mat.DisableKeyword(debugMaterialDirectionKeyword);
                    mat.EnableKeyword(debugOutlinesKeyword);
                    mat.DisableKeyword(debugLuminanceKeyword);
                    break;
                case SketchCompositionPassData.DebugMode.LUMINANCE:
                    mat.DisableKeyword(debugMaterialAlbedoKeyword);
                    mat.DisableKeyword(debugMaterialDirectionKeyword);
                    mat.DisableKeyword(debugOutlinesKeyword);
                    mat.EnableKeyword(debugLuminanceKeyword);
                    break;
            }

            if(passData.RequiresColorTexture())
                ConfigureInput(ScriptableRenderPassInput.Color);
            mat.SetKeyword(hasMaterialKeyword, passData.FeaturesToCompose.Contains(SketchRendererFeatureType.MATERIAL));
            mat.SetKeyword(hasOutlinesKeyword, passData.FeaturesToCompose.Contains(SketchRendererFeatureType.OUTLINE_SMOOTH) || passData.FeaturesToCompose.Contains(SketchRendererFeatureType.OUTLINE_SKETCH));
            mat.SetKeyword(hasLuminanceKeyword, passData.FeaturesToCompose.Contains(SketchRendererFeatureType.LUMINANCE));
            
            mat.SetColor(outlineColorShaderID, passData.OutlineStrokeColor);
            mat.SetColor(shadingColorShaderID, passData.ShadingStrokeColor);
            mat.SetFloat(materialAccumulationShaderID, passData.MaterialAccumulationStrength);
            mat.SetFloat(blendStrengthShaderID, passData.BlendStrength);
        }

        public void Dispose()
        {
            
        }

        private static void ExecuteCompositionPass(PassData passData, RasterGraphContext context)
        {
            passData.material.SetTexture(materialShaderID, passData.materialTexture);
            passData.material.SetTexture(materialDirectionalShaderID, passData.directionalTexture);
            passData.material.SetTexture(outlinesShaderID, passData.outlineTexture);
            passData.material.SetTexture(luminanceShaderID, passData.luminanceTexture);
            passData.material.SetVector(luminanceBasisDirectionShaderID, passData.luminanceBasisDirection);
            Blitter.BlitTexture(context.cmd, passData.dst, scaleBias, passData.material, 0);
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            var sketchData = frameData.Get<SketchResourceData>();
            if(sketchData == null)
                return;
            
            var dstDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            dstDesc.name = "FinalSketchColor";
            dstDesc.clearBuffer = true;
            dstDesc.msaaSamples = MSAASamples.None;
            TextureHandle dst = renderGraph.CreateTexture(dstDesc);
            
            bool usingColorTexture = passData.RequiresColorTexture();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName, out var passData))
            {
                passData.material = mat;
                if (sketchData.MaterialTexture.IsValid())
                {
                    builder.UseTexture(sketchData.MaterialTexture);
                    passData.materialTexture = sketchData.MaterialTexture;
                }
                else if (usingColorTexture)
                {
                    builder.UseTexture(resourceData.activeColorTexture);
                    passData.materialTexture = resourceData.activeColorTexture;
                }

                if (sketchData.DirectionalTexture.IsValid())
                {
                    builder.UseTexture(sketchData.DirectionalTexture);
                    passData.directionalTexture = sketchData.DirectionalTexture;
                }
                if (sketchData.OutlinesTexture.IsValid())
                {
                    builder.UseTexture(sketchData.OutlinesTexture);
                    passData.outlineTexture = sketchData.OutlinesTexture;
                }
                if (sketchData.LuminanceTexture.IsValid())
                {
                    builder.UseTexture(sketchData.LuminanceTexture);
                    passData.luminanceTexture = sketchData.LuminanceTexture;
                }
                
                passData.luminanceBasisDirection = sketchData.LuminanceBasisDirection;

                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);
                passData.dst = dst;
                
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteCompositionPass(data, context));
            }
            
            resourceData.cameraColor = dst;
        }
    }
}