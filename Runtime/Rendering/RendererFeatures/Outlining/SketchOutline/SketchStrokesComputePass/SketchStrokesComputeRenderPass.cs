using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using SketchRenderer.Runtime.TextureTools.Strokes;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class SketchStrokesComputeRenderPass : ScriptableRenderPass
    {
        public string PassName => "SketchyStrokesCompute";
        
        private SketchStrokesPassData passData;
        private ComputeShader sketchComputeShader;
        private ComputeBuffer gradientBuffer;
        private ComputeBuffer strokeDataBuffer;
        private ComputeBuffer strokeVariationDataBuffer;
        
        //Compute Data
        private readonly string COMPUTE_STROKE_KERNEL = "ComputeAverageStroke";
        private readonly string APPLY_STROKES_KERNEL = "ApplyStrokes";
        private int computeStrokeKernelID;
        private int computeApplyStrokeKernelID;
        
        private static readonly int SOURCE_TEXTURE_ID = Shader.PropertyToID("_OriginalSource");
        private static readonly int DIMENSION_WIDTH_ID = Shader.PropertyToID("_TextureWidth");
        private static readonly int DIMENSION_HEIGHT_ID = Shader.PropertyToID("_TextureHeight");
        private static readonly int GROUPS_X_ID = Shader.PropertyToID("_GroupsX");
        private static readonly int GROUPS_Y_ID = Shader.PropertyToID("_GroupsY");
        private static readonly int DOWNSCALE_FACTOR_ID = Shader.PropertyToID("_DownscaleFactor");
        private static readonly int COMPUTE_GRADIENT_VECTORS_ID = Shader.PropertyToID("_GradientVectors");
        private static readonly int THRESHOLD_ID = Shader.PropertyToID("_ThresholdForStroke");
        private static readonly int SMOOTHING_THRESHOLD_ID = Shader.PropertyToID("_SmoothingThreshold");
        private static readonly int STROKE_SCALE_ID = Shader.PropertyToID("_StrokeSampleScale");
        private static readonly int STROKE_DATA_ID = Shader.PropertyToID("_OutlineStrokeData");
        private static readonly int STROKE_VARIATION_DATA_ID = Shader.PropertyToID("_OutlineStrokeVariationData");
        
        private static readonly string PERPENDICULAR_DIRECTION_KEYWORD_ID = "USE_PERPENDICULAR_DIRECTION";
        private static readonly string SMOOTHING_KEYWORD_ID = "FRAME_SMOOTHING";
        
        private Vector3Int strokesKernelThreads;
        private Vector3Int applyKernelThreads;

        private LocalKeyword PerpendicularDirectionKeyword;
        private LocalKeyword SmoothingThresholdKeyword;
        private LocalKeyword[] strokeTypeLocalKeywords;

        private readonly int GRADIENT_VECTOR_STRIDE_LENGTH = sizeof(float) * 4;
        
        private Vector2Int GetDownscaleTargetDimension(Vector2 currentResolution, int factor)
        {
            //if(currentResolution.x > 1920f || currentResolution.y > 1080f)
                //currentResolution = new Vector2(1920f, 1080f);
            return new Vector2Int(Mathf.CeilToInt(currentResolution.x / factor), Mathf.CeilToInt(currentResolution.y / factor));
        }

        public void Setup(SketchStrokesPassData passData, Material _, ComputeShader computeShader)
        {
            this.passData = passData;
            sketchComputeShader = computeShader;
            
            requiresIntermediateTexture = false;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            ConfigureComputeShader();
        }

        private void ConfigureComputeShader()
        {
            PerpendicularDirectionKeyword = new LocalKeyword(sketchComputeShader, PERPENDICULAR_DIRECTION_KEYWORD_ID);
            SmoothingThresholdKeyword = new LocalKeyword(sketchComputeShader, SMOOTHING_KEYWORD_ID);
            sketchComputeShader.SetKeyword(PerpendicularDirectionKeyword, passData.UsePerpendicularDirection);
            sketchComputeShader.SetKeyword(SmoothingThresholdKeyword, passData.FrameSmoothingFactor > 0);
            
            string[] sdfTypes = Enum.GetNames(typeof(StrokeSDFType));
            strokeTypeLocalKeywords = new LocalKeyword[sdfTypes.Length];
            string selectedType = passData.OutlineStrokeData.PatternType.ToString();
            for (int t = 0; t < sdfTypes.Length; t++)
            {
                strokeTypeLocalKeywords[t] = new LocalKeyword(sketchComputeShader, sdfTypes[t]);
                if (sdfTypes[t] == selectedType)
                    sketchComputeShader.EnableKeyword(strokeTypeLocalKeywords[t]);
                else
                    sketchComputeShader.DisableKeyword(strokeTypeLocalKeywords[t]);
            }
            
            computeStrokeKernelID = sketchComputeShader.FindKernel(Get1DAreaReliantKernelID(COMPUTE_STROKE_KERNEL, passData.SampleArea));
            sketchComputeShader.GetKernelThreadGroupSizes(computeStrokeKernelID, out uint x, out uint y, out uint z);
            strokesKernelThreads = new Vector3Int((int)x, (int)y, (int)z);

            computeApplyStrokeKernelID = sketchComputeShader.FindKernel(Get1DAreaReliantKernelID(APPLY_STROKES_KERNEL, passData.SampleArea));
            sketchComputeShader.GetKernelThreadGroupSizes(computeApplyStrokeKernelID, out uint x1, out uint y1, out uint z1);
            applyKernelThreads = new Vector3Int((int)x1, (int)y1, (int)z1);
        }

        private string Get1DAreaReliantKernelID(string kernelName, ComputeData.KernelSize2D kernelSize)
        {
            Vector2Int sizes = ComputeData.GetKernelSizeFromEnum(kernelSize);
            return $"{kernelName}{sizes.x}";
        }

        public void Dispose()
        {
            if (gradientBuffer != null)
            {
                gradientBuffer.Release();
                gradientBuffer = null;
            }

            if (strokeDataBuffer != null)
            {
                strokeDataBuffer.Release();
                strokeDataBuffer = null;
            }

            if (strokeVariationDataBuffer != null)
            {
                strokeVariationDataBuffer.Release();
                strokeVariationDataBuffer = null;
            }
        }

        class ComputePassData
        {
            public ComputeShader computeShader;
            public int kernelID;
            public Vector3Int threadGroupSize;
            public Vector2Int dimensions;
            public TextureHandle outlineTex;
            public float threshold;
            public float frameSmoothingFactor;
            public ComputeBuffer outputBuffer;
        }

        class DownscalePassData
        {
            public TextureHandle source;
            public TextureHandle dest;
        }

        class StrokesPassData
        {
            public ComputeShader computeShader;
            public int kernelID;
            public Vector3Int threadGroupSize;
            public Vector2Int dimensions;
            public TextureHandle outlineTex;
            public ComputeBuffer inputBuffer;
            public ComputeBuffer strokeDataBuffer;
            public ComputeBuffer strokeVariationDataBuffer;
            public int downscaleFactor;
            public int strokeSampleScale;
        }

        static void ExecuteDownscale(DownscalePassData passData, UnsafeGraphContext context)
        {
            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
            cmd.SetRenderTarget(passData.dest);
            Blitter.BlitTexture(cmd, passData.source, new Vector4(1f, 1f, 0f, 0f), 0f, false);
        }
        static void ExecuteFindStrokesCompute(ComputePassData passData, UnsafeGraphContext context)
        {
            context.cmd.SetRenderTarget(passData.outlineTex);
            context.cmd.SetComputeTextureParam(passData.computeShader, passData.kernelID, SOURCE_TEXTURE_ID, passData.outlineTex);
            context.cmd.SetComputeIntParam(passData.computeShader, DIMENSION_WIDTH_ID, passData.dimensions.x);
            context.cmd.SetComputeIntParam(passData.computeShader, DIMENSION_HEIGHT_ID, passData.dimensions.y);
            context.cmd.SetComputeIntParam(passData.computeShader, GROUPS_X_ID, passData.threadGroupSize.x);
            context.cmd.SetComputeIntParam(passData.computeShader, GROUPS_Y_ID, passData.threadGroupSize.y);
            context.cmd.SetComputeBufferParam(passData.computeShader, passData.kernelID, COMPUTE_GRADIENT_VECTORS_ID, passData.outputBuffer);
            context.cmd.SetComputeFloatParam(passData.computeShader, THRESHOLD_ID, passData.threshold);
            context.cmd.SetComputeFloatParam(passData.computeShader, SMOOTHING_THRESHOLD_ID, passData.frameSmoothingFactor);
            context.cmd.DispatchCompute(passData.computeShader, passData.kernelID, passData.threadGroupSize.x, passData.threadGroupSize.y, passData.threadGroupSize.z);
        }

        static void ExecuteApplyStrokesCompute(StrokesPassData passData, UnsafeGraphContext context)
        {
            context.cmd.SetRenderTarget(passData.outlineTex);
            context.cmd.SetComputeIntParam(passData.computeShader, GROUPS_X_ID, passData.threadGroupSize.x);
            context.cmd.SetComputeIntParam(passData.computeShader, GROUPS_Y_ID, passData.threadGroupSize.y);
            context.cmd.SetComputeIntParam(passData.computeShader, DOWNSCALE_FACTOR_ID, passData.downscaleFactor);
            context.cmd.SetComputeIntParam(passData.computeShader, STROKE_SCALE_ID, passData.strokeSampleScale);
            context.cmd.SetComputeIntParam(passData.computeShader, DIMENSION_WIDTH_ID, passData.dimensions.x);
            context.cmd.SetComputeIntParam(passData.computeShader, DIMENSION_HEIGHT_ID, passData.dimensions.y);
            context.cmd.SetComputeTextureParam(passData.computeShader, passData.kernelID, SOURCE_TEXTURE_ID, passData.outlineTex);
            context.cmd.SetComputeBufferParam(passData.computeShader, passData.kernelID, COMPUTE_GRADIENT_VECTORS_ID, passData.inputBuffer);
            context.cmd.SetComputeBufferParam(passData.computeShader, passData.kernelID, STROKE_DATA_ID, passData.strokeDataBuffer);
            context.cmd.SetComputeBufferParam(passData.computeShader, passData.kernelID, STROKE_VARIATION_DATA_ID, passData.strokeVariationDataBuffer);
            context.cmd.DispatchCompute(passData.computeShader, passData.kernelID, passData.threadGroupSize.x, passData.threadGroupSize.y, passData.threadGroupSize.z);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            var sketchData = frameData.GetOrCreate<SketchResourceData>();
            if(!sketchData.OutlinesTexture.IsValid())
                return;
            
            bool isDoingDownscale = false;
            var desc = renderGraph.GetTextureDesc(sketchData.OutlinesTexture);
            TextureHandle downscaleTex = TextureHandle.nullHandle;
            TextureHandle originalOutlines = sketchData.OutlinesTexture;
            if (passData.DoDownscale && (GetDownscaleTargetDimension(new Vector2(desc.width, desc.height), passData.DownscaleFactor) is Vector2Int downscaleRes && (desc.width > downscaleRes.x || desc.height > downscaleRes.y)))
            {
                //If we are downscaling, for some reason this has always been culled even when the texture is assigned back
                //so prevent it from ever culling
                using (var downBuilder = renderGraph.AddUnsafePass(PassName + "_Downscale", out DownscalePassData passData))
                {
                    downBuilder.AllowPassCulling(false);
                    downBuilder.UseTexture(originalOutlines);
                    //create a temporary downscaled version to blit to
                    var downDesc = desc;
                    downDesc.width = downscaleRes.x;
                    downDesc.height = downscaleRes.y;
                    downscaleTex = renderGraph.CreateTexture(downDesc);
                    downBuilder.UseTexture(downscaleTex);

                    passData.source = sketchData.OutlinesTexture;
                    passData.dest = downscaleTex;
                    
                    downBuilder.SetRenderFunc((DownscalePassData passData, UnsafeGraphContext context) => ExecuteDownscale(passData, context));

                    isDoingDownscale = true;
                    sketchData.OutlinesTexture = downscaleTex;
                }
            }

            using (var builder = renderGraph.AddUnsafePass(PassName, out ComputePassData computePassData))
            {
                //since this dosent assign back to the texture, if it exists, make sure the pass cant be culled
                builder.AllowPassCulling(false);
          
                builder.UseTexture(sketchData.OutlinesTexture);
                computePassData.outlineTex = sketchData.OutlinesTexture;
                
                var computeDesc = renderGraph.GetTextureDesc(sketchData.OutlinesTexture);
                Vector2Int dimensions = new Vector2Int(computeDesc.width, computeDesc.height);
                Vector3Int groups = new Vector3Int(
                    Mathf.CeilToInt((float)dimensions.x / (float)strokesKernelThreads.x),
                    Mathf.CeilToInt((float)dimensions.y / (float)strokesKernelThreads.y),
                    1
                );
                computePassData.threadGroupSize = groups;
                
                if (gradientBuffer == null || gradientBuffer.count != (groups.x * groups.y))
                {
                    if(gradientBuffer != null)
                        gradientBuffer.Release();
                    
                    gradientBuffer = new ComputeBuffer(groups.x * groups.y, GRADIENT_VECTOR_STRIDE_LENGTH);
                }
                
                computePassData.dimensions = new Vector2Int(dimensions.x, dimensions.y);
                computePassData.threshold = passData.StrokeThreshold;
                computePassData.frameSmoothingFactor = passData.FrameSmoothingFactor;
                computePassData.outputBuffer = gradientBuffer;
                
                computePassData.computeShader = sketchComputeShader;
                computePassData.kernelID = computeStrokeKernelID;
                
                builder.SetRenderFunc((ComputePassData computePassData, UnsafeGraphContext context) => ExecuteFindStrokesCompute(computePassData, context));
            }
            
            if (isDoingDownscale)
            {
                using(var upBuilder = renderGraph.AddUnsafePass(PassName + "_Upscale", out DownscalePassData passData)) {
                    upBuilder.AllowPassCulling(false);
                    upBuilder.UseTexture(sketchData.OutlinesTexture);
                    upBuilder.UseTexture(originalOutlines);
                    passData.source = sketchData.OutlinesTexture;
                    passData.dest = originalOutlines;
                    upBuilder.SetRenderFunc((DownscalePassData passData, UnsafeGraphContext context) => ExecuteDownscale(passData, context));
                    sketchData.OutlinesTexture = originalOutlines;
                }
            }

            using (var applyBuilder = renderGraph.AddUnsafePass(PassName + "_ApplyStrokes", out StrokesPassData computePassData))
            {
                applyBuilder.AllowPassCulling(false);
                applyBuilder.UseTexture(sketchData.OutlinesTexture);
                computePassData.outlineTex = sketchData.OutlinesTexture;
                computePassData.inputBuffer = gradientBuffer;
                computePassData.downscaleFactor = passData.DownscaleFactor;
                computePassData.strokeSampleScale = passData.StrokeSampleScale;

                computePassData.computeShader = sketchComputeShader;
                computePassData.kernelID = computeApplyStrokeKernelID;
                
                var computeDesc = renderGraph.GetTextureDesc(sketchData.OutlinesTexture);
                Vector2Int dimensions = new Vector2Int(computeDesc.width, computeDesc.height);
                Vector3Int groups = new Vector3Int(
                    Mathf.CeilToInt((float)dimensions.x / (float)applyKernelThreads.x),
                    Mathf.CeilToInt((float)dimensions.y / (float)applyKernelThreads.y),
                    1
                );
                computePassData.threadGroupSize = groups;
                computePassData.dimensions = new Vector2Int(dimensions.x, dimensions.y);
                    
                if (strokeDataBuffer == null)
                {
                    strokeDataBuffer = new ComputeBuffer(1, passData.OutlineStrokeData.StrokeData.GetStrideLength());
                }
                strokeDataBuffer.SetData(new [] {passData.OutlineStrokeData.GetSampleReadyData()});
                computePassData.strokeDataBuffer = strokeDataBuffer;
                
                if (strokeVariationDataBuffer == null)
                {
                    strokeVariationDataBuffer = new ComputeBuffer(1, passData.OutlineStrokeData.VariationData.GetStrideLength());
                }
                strokeVariationDataBuffer.SetData(new [] {passData.OutlineStrokeData.VariationData});
                computePassData.strokeVariationDataBuffer = strokeVariationDataBuffer;
                
                applyBuilder.SetRenderFunc((StrokesPassData data, UnsafeGraphContext context) => ExecuteApplyStrokesCompute(data, context));
            }
        }
    }
}