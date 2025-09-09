using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SketchRenderer.Editor.Utils;
using SketchRenderer.Runtime.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using SketchRenderer.ShaderLibrary;
using SketchRenderer.Runtime.TextureTools.Strokes;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;

namespace SketchRenderer.Editor.TextureTools
{
    internal static class TonalArtMapGenerator
    {
        internal static event Action OnTextureUpdated;
        internal static event Action<TextureToolGenerationStatusArgs> OnTonalArtMapGenerationStep; 
        internal static event Action OnTonalArtMapGenerated;
        
        internal static string DefaultFileOutputName => "TonalArtMapTexture";
        internal static string DefaultFileOutputPath
        {
            get
            {
                if(!string.IsNullOrEmpty(OverrideOutputPath))
                    return OverrideOutputPath;
                
                if (TonalArtMapAsset != null && hasNonDefaultTonalArtMapAsset)
                {
                    return Path.Combine(TextureAssetManager.GetAssetPath(TonalArtMapAsset).Split('.')[0],
                        "ToneTextures");
                }

                return Path.Combine(TextureGenerator.DefaultFileOutputPath, DefaultFileOutputName);
            }
        }
        
        internal static ComputeShader TAMGeneratorShader;
        [Range(1, 100)]
        internal static int IterationsPerStroke = 15;
        [Range(0, 1)] 
        internal static float TargetFillRate = 1f;
        internal static TextureResolution TargetResolution = TextureResolution.SIZE_512;

        internal static StrokeAsset strokeDataAsset;
        internal static StrokeAsset[] defaultStrokeDataAssets;
        internal static StrokeAsset StrokeDataAsset
        {
            get => strokeDataAsset;
            set
            {
                strokeDataAsset = value;
                bool hasNonDefault = strokeDataAsset != null;
                if (hasNonDefault)
                {
                    for (int i = 0; i < defaultStrokeDataAssets.Length; i++)
                    {
                        if (strokeDataAsset == defaultStrokeDataAssets[i])
                            return;
                    }

                    hasNonDefaultStrokeAsset = false;
                }
            }
        }
        private static bool hasNonDefaultStrokeAsset = false;
    
        private static TonalArtMapAsset tonalArtMapAsset;
        private static TonalArtMapAsset defaultTonalArtMapAsset;
        internal static TonalArtMapAsset TonalArtMapAsset
        {
            get => tonalArtMapAsset;
            set
            {
                tonalArtMapAsset = value;
                hasNonDefaultTonalArtMapAsset = tonalArtMapAsset != null && tonalArtMapAsset != defaultTonalArtMapAsset;
            }
        }
        private static bool hasNonDefaultTonalArtMapAsset = false;


        internal static string OverrideOutputPath;
        internal static bool PackTAMTextures = true;
        
        //Compute Data
        private static readonly string GENERATE_STROKE_BUFFER_KERNEL = "GenerateRandomStrokeBuffer";
        private static readonly string APPLY_STROKE_KERNEL = "ApplyStrokeIterated";
        private static readonly string TONE_FILL_RATE_KERNEL = "FindAverageTextureFillRate";
        private static readonly string BLIT_STROKE_KERNEL = "BlitFinalSelectedStroke";
        private static readonly string PACK_STROKES_KERNEL = "PackStrokeTextures";
        private static int csGenerateStrokeBufferKernelID;
        private static int csApplyStrokeKernelID;
        private static int csFillRateKernelID;
        private static int csBlitStrokeKernelID;
        private static int csPackStrokesKernelID;
        private static Vector3Int csGenerateStrokeBufferKernelThreads;
        private static Vector3Int csApplyStrokeKernelThreads;
        private static Vector3Int csFillRateKernelThreads;
        private static Vector3Int csBlitStrokeKernelThreads;
        private static Vector3Int csPackStrokesKernelThreads;
        private static ComputeBuffer strokeDataBuffer;
        private static ComputeBuffer variationDataBuffer;
        private static ComputeBuffer[] strokeIterationTextureBuffers;
        private static ComputeBuffer strokeTextureTonesBuffer;
        private static ComputeBuffer strokeReducedSource;
        private static ComputeBuffer fillRateBuffer;
        
        private const int MAX_THREADS_PER_DISPATCH = 65535;
        
        //Shader Properties
        private static readonly int RENDER_TEXTURE_ID = Shader.PropertyToID("_OriginalSource");
        private static readonly int REDUCED_SOURCE_ID = Shader.PropertyToID("_ReducedSource");
        private static readonly int STROKE_DATA_ID = Shader.PropertyToID("_StrokeData");
        private static readonly int VARIATION_DATA_ID = Shader.PropertyToID("_VariationData");    
        private static readonly int ITERATION_STEP_TEXTURE_ID = Shader.PropertyToID("_IterationOutputs");
        private static readonly int TONE_RESULTS_ID = Shader.PropertyToID("_ToneResults");
        private static readonly int ITERATIONS_ID = Shader.PropertyToID("_Iteration");
        private static readonly int DIMENSION_ID = Shader.PropertyToID("_Dimension");
        private static readonly int SEED_ID = Shader.PropertyToID("_Seed");
        private static readonly int FILL_RATE_ID = Shader.PropertyToID("_Tone_GlobalCache");
        private static readonly int FILL_RATE_BUFFER_SIZE_ID = Shader.PropertyToID("_BufferSize");
        private static readonly int FILL_RATE_SPLIT_BUFFER_SIZE_ID = Shader.PropertyToID("_SplitBufferSize");
        private static readonly int FILL_RATE_BUFFER_OFFSET_ID = Shader.PropertyToID("_BufferOffset");
        private static readonly int BLIT_RESULT_ID = Shader.PropertyToID("_BlitResult");
        private static readonly int PACK_R_TEXTURE = Shader.PropertyToID("_PackTextR");
        private static readonly int PACK_G_TEXTURE = Shader.PropertyToID("_PackTextG");
        private static readonly int PACK_B_TEXTURE = Shader.PropertyToID("_PackTextB");
        
        //Keywords
        private static readonly string FIRST_ITERATION_KEYWORD = "IS_FIRST_ITERATION";
        private static readonly string LAST_REDUCTION_KEYWORD = "IS_LAST_REDUCTION";
        private static readonly string PACK_TEXTURES_2_KEYWORD = "PACK_TEXTURES_2";
        private static readonly string PACK_TEXTURES_3_KEYWORD = "PACK_TEXTURES_3";
        
        private static LocalKeyword firstIterationLocalKeyword;
        private static LocalKeyword lastReductionLocalKeyword;
        private static LocalKeyword[] falloffLocalKeywords;
        private static LocalKeyword[] strokeTypeLocalKeywords;
        private static LocalKeyword packTextures2LocalKeyword;
        private static LocalKeyword packTextures3LocalKeyword;

        private static bool generating;
        internal static bool Generating
        {
            get => generating;
            private set
            {
                if(generating && !value)
                    OnTextureUpdated?.Invoke();
                generating = value;
            }
        }

        private static Coroutine currentRoutine;

        internal static bool CanRequest { get { return !Generating; } }

        internal static void Init(SketchResourceAsset resources)
        {
            if (TAMGeneratorShader == null)
                TAMGeneratorShader = resources.ComputeShaders.TonalArtMapGenerator;

            defaultTonalArtMapAsset = resources.Scriptables.TonalArtMap;
            if (TonalArtMapAsset == null)
            {
                TonalArtMapAsset = resources.Scriptables.TonalArtMap;
            }

            defaultStrokeDataAssets = new StrokeAsset[]
            {
                resources.Scriptables.Strokes.DefaultSimpleStroke, resources.Scriptables.Strokes.DefaultHatchingStroke,
                resources.Scriptables.Strokes.DefaultZigzagStroke, resources.Scriptables.Strokes.DefaultFeatheringStroke
            };
            if (StrokeDataAsset == null)
            {
                StrokeDataAsset = resources.Scriptables.Strokes.DefaultSimpleStroke;
            }

            ConfigureGeneratorData();
            DisplaySDF();
        }

        internal static void Finish()
        {
            ReleaseBuffers();
            StaticCoroutine.StopCoroutine(currentRoutine);
        }

        internal static void ConfigureGeneratorData()
        {
            if(!IsGeneratorValid())
                return;

            if (TextureGenerator.Resolution != TargetResolution)
            {
                TextureGenerator.Resolution = TargetResolution;
                TextureGenerator.PrepareGeneratorForRender();
            }

            TextureGenerator.OverwriteGeneratorOutputSettings(DefaultFileOutputName, DefaultFileOutputPath);
            
            ConfigureBuffers();
            PrepareComputeData();
        }

        internal static bool IsGeneratorValid()
        {
            if(TAMGeneratorShader == null || StrokeDataAsset == null || TonalArtMapAsset == null)
                return false;

            return true;
        }

        internal static TextureToolGenerationStatusArgs CreateStatusArgs(string subProcess, float relativeProgress)
        {
            TextureToolGenerationStatusArgs args = new TextureToolGenerationStatusArgs();
            args.toolProcess = "Generating Tonal Art Maps";
            args.toolSubprocess = subProcess;
            args.toolTotalProgress = relativeProgress;
            return args;
        }
        
        #region Compute

        private static void ManageStrokeDataKeywords()
        {
            firstIterationLocalKeyword = new LocalKeyword(TAMGeneratorShader, FIRST_ITERATION_KEYWORD);
            lastReductionLocalKeyword = new LocalKeyword(TAMGeneratorShader, LAST_REDUCTION_KEYWORD);

            string[] falloffs = Enum.GetNames(typeof(FalloffFunction));
            falloffLocalKeywords = new LocalKeyword[falloffs.Length];
            string selected = StrokeDataAsset.SelectedFalloffFunction.ToString();
            for (int i = 0; i < falloffs.Length; i++)
            {
                falloffLocalKeywords[i] = new LocalKeyword(TAMGeneratorShader, falloffs[i]);
                if (falloffs[i] == selected)
                    TAMGeneratorShader.EnableKeyword(falloffLocalKeywords[i]);
                else
                    TAMGeneratorShader.DisableKeyword(falloffLocalKeywords[i]);
            }
            
            string[] sdfTypes = Enum.GetNames(typeof(StrokeSDFType));
            strokeTypeLocalKeywords = new LocalKeyword[sdfTypes.Length];
            string selectedType = StrokeDataAsset.PatternType.ToString();
            for (int t = 0; t < sdfTypes.Length; t++)
            {
                strokeTypeLocalKeywords[t] = new LocalKeyword(TAMGeneratorShader, sdfTypes[t]);
                if (sdfTypes[t] == selectedType)
                {
                    TAMGeneratorShader.EnableKeyword(strokeTypeLocalKeywords[t]);
                }
                else
                    TAMGeneratorShader.DisableKeyword(strokeTypeLocalKeywords[t]);
            }

            packTextures2LocalKeyword = new LocalKeyword(TAMGeneratorShader, PACK_TEXTURES_2_KEYWORD);
            packTextures3LocalKeyword = new LocalKeyword(TAMGeneratorShader, PACK_TEXTURES_3_KEYWORD);
        }

        private static void PrepareComputeData()
        {
            ManageStrokeDataKeywords();

            if (TAMGeneratorShader.HasKernel(GENERATE_STROKE_BUFFER_KERNEL))
            {
                csGenerateStrokeBufferKernelID = TAMGeneratorShader.FindKernel(GENERATE_STROKE_BUFFER_KERNEL);
                TAMGeneratorShader.GetKernelThreadGroupSizes(csGenerateStrokeBufferKernelID, out uint groupsX, out uint groupsY, out uint groupsZ);
                csGenerateStrokeBufferKernelThreads = new Vector3Int(
                    Mathf.CeilToInt((float)IterationsPerStroke / groupsX), 
                    1, 
                    1);
                
                TAMGeneratorShader.SetInt(DIMENSION_ID, TextureGenerator.Dimension);
                TAMGeneratorShader.SetBuffer(csGenerateStrokeBufferKernelID, STROKE_DATA_ID, strokeDataBuffer);      
                TAMGeneratorShader.SetBuffer(csGenerateStrokeBufferKernelID, VARIATION_DATA_ID, variationDataBuffer);      
            }
            
            if (TAMGeneratorShader.HasKernel(APPLY_STROKE_KERNEL))
            {
                csApplyStrokeKernelID = TAMGeneratorShader.FindKernel(APPLY_STROKE_KERNEL);
                TAMGeneratorShader.GetKernelThreadGroupSizes(csApplyStrokeKernelID, out uint groupsX, out uint groupsY, out uint groupsZ);
                csApplyStrokeKernelThreads = new Vector3Int(
                    Mathf.CeilToInt((float)TextureGenerator.Dimension / groupsX), 
                    Mathf.CeilToInt((float)TextureGenerator.Dimension / groupsY), 
                    1);
                
                TAMGeneratorShader.SetTexture(csApplyStrokeKernelID, RENDER_TEXTURE_ID, TextureGenerator.TargetRT);
                TAMGeneratorShader.SetBuffer(csApplyStrokeKernelID, REDUCED_SOURCE_ID, strokeReducedSource);
                TAMGeneratorShader.SetBuffer(csApplyStrokeKernelID, STROKE_DATA_ID, strokeDataBuffer);
                TAMGeneratorShader.SetInt(DIMENSION_ID, TextureGenerator.Dimension);
            }

            if (TAMGeneratorShader.HasKernel(TONE_FILL_RATE_KERNEL))
            {
                csFillRateKernelID = TAMGeneratorShader.FindKernel(TONE_FILL_RATE_KERNEL);
                TAMGeneratorShader.GetKernelThreadGroupSizes(csFillRateKernelID, out uint groupsX, out uint groupsY, out uint groupsZ);
                csFillRateKernelThreads = new Vector3Int(
                    Mathf.CeilToInt(groupsX), 
                    Mathf.CeilToInt(groupsY), 
                    Mathf.CeilToInt(groupsZ));
                TAMGeneratorShader.SetBuffer(csFillRateKernelID, TONE_RESULTS_ID, strokeTextureTonesBuffer);
            }

            if (TAMGeneratorShader.HasKernel(BLIT_STROKE_KERNEL))
            {
                csBlitStrokeKernelID = TAMGeneratorShader.FindKernel(BLIT_STROKE_KERNEL);
                TAMGeneratorShader.GetKernelThreadGroupSizes(csBlitStrokeKernelID, out uint groupsX, out uint groupsY, out uint groupsZ);
                csBlitStrokeKernelThreads = new Vector3Int(
                    Mathf.CeilToInt((float)TextureGenerator.Dimension / groupsX), 
                    Mathf.CeilToInt((float)TextureGenerator.Dimension / groupsY), 
                    1);
                
                TAMGeneratorShader.SetTexture(csBlitStrokeKernelID, RENDER_TEXTURE_ID, TextureGenerator.TargetRT);
            }

            if (TAMGeneratorShader.HasKernel(PACK_STROKES_KERNEL))
            {
                csPackStrokesKernelID = TAMGeneratorShader.FindKernel(PACK_STROKES_KERNEL);
                TAMGeneratorShader.GetKernelThreadGroupSizes(csPackStrokesKernelID, out uint groupsX, out uint groupsY, out uint groupsZ);
                csPackStrokesKernelThreads = new Vector3Int(
                    Mathf.CeilToInt((float)TextureGenerator.Dimension / groupsX),
                    Mathf.CeilToInt((float)TextureGenerator.Dimension / groupsY),
                    1);
                
                TAMGeneratorShader.SetTexture(csPackStrokesKernelID, RENDER_TEXTURE_ID, TextureGenerator.TargetRT);
            }
        }

        private static void ConfigureBuffers()
        {
            ReleaseBuffers();
            
            ConfigureStrokesBuffer(0f);
            
            strokeIterationTextureBuffers = new ComputeBuffer[IterationsPerStroke];
            for (int i = 0; i < IterationsPerStroke; i++)
            {
                strokeIterationTextureBuffers[i] = new ComputeBuffer(TextureGenerator.Dimension * TextureGenerator.Dimension, sizeof(uint));
            }
            
            strokeTextureTonesBuffer = new ComputeBuffer(IterationsPerStroke, sizeof(uint));
            strokeReducedSource = new ComputeBuffer(TextureGenerator.Dimension*TextureGenerator.Dimension, sizeof(uint));
            fillRateBuffer = new ComputeBuffer(TextureGenerator.Dimension*TextureGenerator.Dimension, sizeof(uint));
        }
        
        private static void ConfigureStrokesBuffer(float fillRate)
        {
            StrokeData[] strokeDatas = new StrokeData[IterationsPerStroke];
            StrokeVariationData[] variationDatas = new StrokeVariationData[IterationsPerStroke];
            StrokeData expectedStrokeData = StrokeDataAsset.UpdatedDataByFillRate(fillRate);
            for (int i = 0; i < IterationsPerStroke; i++)
            {
                //TAMStrokeData iterationData = StrokeDataAsset.Randomize(fillRate);
                strokeDatas[i] = expectedStrokeData;
                variationDatas[i] = StrokeDataAsset.VariationData;
            }
            if(strokeDataBuffer == null)
                strokeDataBuffer = new ComputeBuffer(IterationsPerStroke, StrokeDataAsset.StrokeData.GetStrideLength());
            if (variationDataBuffer == null)
                variationDataBuffer = new ComputeBuffer(IterationsPerStroke, StrokeDataAsset.VariationData.GetStrideLength());

            strokeDataBuffer.SetData(strokeDatas);
            variationDataBuffer.SetData(variationDatas);
        }

        private static void ReleaseBuffers()
        {
            if (strokeDataBuffer != null)
            {
                strokeDataBuffer.Release();
                strokeDataBuffer = null;
            }

            if (variationDataBuffer != null)
            {
                variationDataBuffer.Release();
                variationDataBuffer = null;
            }

            if (strokeIterationTextureBuffers != null)
            {
                for (int i = 0; i < strokeIterationTextureBuffers.Length; i++)
                {
                    if(strokeIterationTextureBuffers[i] == null)
                        continue;
                    strokeIterationTextureBuffers[i].Release();
                    strokeIterationTextureBuffers[i] = null;
                }
                strokeIterationTextureBuffers = null;
            }

            if (strokeTextureTonesBuffer != null)
            {
                strokeTextureTonesBuffer.Release();
                strokeTextureTonesBuffer = null;
            }
            
            if (strokeReducedSource != null)
            {
                strokeReducedSource.Release();
                strokeReducedSource = null;
            }

            if (fillRateBuffer != null)
            {
                fillRateBuffer.Release();
                fillRateBuffer = null;
            }
        }
        
        internal static float ApplyStrokeKernel(int iteration)
        {
            //Randomize stroke positions
            TAMGeneratorShader.SetInt(SEED_ID, (int)Time.realtimeSinceStartup + Mathf.RoundToInt(UnityEngine.Random.value * iteration * 997));
            TAMGeneratorShader.Dispatch(csGenerateStrokeBufferKernelID, csGenerateStrokeBufferKernelThreads.x, csGenerateStrokeBufferKernelThreads.y, csGenerateStrokeBufferKernelThreads.z);
            return ExecuteIteratedStrokeKernel();
        }

        internal static void DisplaySDF()
        {
            if (!IsGeneratorValid())
            {
                TextureGenerator.PrepareGeneratorForRender();
                return;
            }

            if (Generating)
            {
                return;
            }
            
            int prevIterations = IterationsPerStroke;
            IterationsPerStroke = 1;
            TextureGenerator.PrepareGeneratorForRender();
            ConfigureGeneratorData();
            strokeDataBuffer.SetData(new [] {StrokeDataAsset.PreviewDisplay()});
            ExecuteIteratedStrokeKernel();
            IterationsPerStroke = prevIterations;
            
            
            ReleaseBuffers();
            OnTextureUpdated?.Invoke();
        }
        private static float ExecuteIteratedStrokeKernel()
        {
            for (int i = 0; i < IterationsPerStroke; i++)
            {
                if(i == 0)
                    TAMGeneratorShader.EnableKeyword(firstIterationLocalKeyword);
                else
                    TAMGeneratorShader.DisableKeyword(firstIterationLocalKeyword);
                //DISPATCH INDIVIDUAL STROKE APPLICATION ITERATIONS
                TAMGeneratorShader.SetInt(ITERATIONS_ID, i);
                TAMGeneratorShader.SetBuffer(csApplyStrokeKernelID, ITERATION_STEP_TEXTURE_ID, strokeIterationTextureBuffers[i]);
                TAMGeneratorShader.Dispatch(csApplyStrokeKernelID, csApplyStrokeKernelThreads.x, csApplyStrokeKernelThreads.y, csApplyStrokeKernelThreads.z);
            }
            
            for (int j = 0; j < IterationsPerStroke; j++)
            {
                TAMGeneratorShader.SetBuffer(csFillRateKernelID, ITERATION_STEP_TEXTURE_ID, strokeIterationTextureBuffers[j]);
                TAMGeneratorShader.SetBuffer(csFillRateKernelID, FILL_RATE_ID, fillRateBuffer);
                TAMGeneratorShader.SetInt(ITERATIONS_ID, j);
                int expectedBufferSize = TextureGenerator.Dimension * TextureGenerator.Dimension;
                
                for (int bufferSize = expectedBufferSize; bufferSize > 1; bufferSize = Mathf.CeilToInt((float)bufferSize/(float)csFillRateKernelThreads.x))
                {
                    if (bufferSize == TextureGenerator.Dimension * TextureGenerator.Dimension)
                        TAMGeneratorShader.EnableKeyword(firstIterationLocalKeyword);
                    else
                        TAMGeneratorShader.DisableKeyword(firstIterationLocalKeyword);
                    
                    int reductionGroupSize = Mathf.CeilToInt((float)(bufferSize) / (float)csFillRateKernelThreads.x);
                    
                    if (reductionGroupSize > 1)
                        TAMGeneratorShader.DisableKeyword(lastReductionLocalKeyword);
                    else
                        TAMGeneratorShader.EnableKeyword(lastReductionLocalKeyword);
                    int amountToSplitBuffer = 1;
                    if (reductionGroupSize > MAX_THREADS_PER_DISPATCH)
                    {
                        amountToSplitBuffer = Mathf.CeilToInt((float)reductionGroupSize / (float)MAX_THREADS_PER_DISPATCH);
                    }
                    
                    TAMGeneratorShader.SetInt(FILL_RATE_BUFFER_SIZE_ID,bufferSize);

                    int amountDispatched = 0;
                    int groupsDispatched = 0;
                    for (int s = 0; s < amountToSplitBuffer; s++)
                    {
                        bool isUnderflow = amountDispatched + MAX_THREADS_PER_DISPATCH > bufferSize;
                        int splitBufferSize = isUnderflow ? bufferSize - amountDispatched : MAX_THREADS_PER_DISPATCH * csFillRateKernelThreads.x;
                        
                        TAMGeneratorShader.SetInt(FILL_RATE_SPLIT_BUFFER_SIZE_ID,amountDispatched);
                        TAMGeneratorShader.SetInt(FILL_RATE_BUFFER_OFFSET_ID, groupsDispatched);
                        
                        int reductionGroups = Mathf.CeilToInt((float)splitBufferSize / (float)csFillRateKernelThreads.x);
                        TAMGeneratorShader.Dispatch(csFillRateKernelID, reductionGroups, csFillRateKernelThreads.y,
                            csFillRateKernelThreads.z);
                        
                        amountDispatched += splitBufferSize;
                        groupsDispatched += reductionGroups;
                    }
                    if(bufferSize == 1)
                        break;
                }
            }
            
            //Here we unfortunately call back from GPU memory to decide which value to choose in an O(N) operations loop.
            //At the worst case, this is a call back on sizeof(uint) * MaxPossibleValueOf(_IterationsPerStroke) of data
            //TODO: Surely theres a better way to do this?
            uint[] fillRates = new uint[IterationsPerStroke];
            strokeTextureTonesBuffer.GetData(fillRates);

            //maxFillrate will be equal to 1 - the found tone
            int maxToneIndex = 0;
            float maxFillRateFound = -1;
            for (int i = 0; i < fillRates.Length; i++)
            {
                float fillRate = 1f - ((float)fillRates[i]/(float)(TextureGenerator.Dimension*TextureGenerator.Dimension*255));
                if (fillRate > maxFillRateFound)
                {
                    maxFillRateFound = fillRate;
                    maxToneIndex = i;
                }
            }
            
            TAMGeneratorShader.SetBuffer(csBlitStrokeKernelID, BLIT_RESULT_ID, strokeIterationTextureBuffers[maxToneIndex]);
            TAMGeneratorShader.Dispatch(csBlitStrokeKernelID, csBlitStrokeKernelThreads.x, csBlitStrokeKernelThreads.y, csBlitStrokeKernelThreads.z);
            return maxFillRateFound;
        }

        internal static void CombineStrokeTextures(Texture2D texture1, Texture2D texture2 = null, Texture2D texture3 = null)
        {
            TAMGeneratorShader.SetKeyword(packTextures2LocalKeyword, texture2 != null && texture3 == null);
            TAMGeneratorShader.SetKeyword(packTextures3LocalKeyword, texture2 != null && texture3 != null);
            
            RenderTexture tmp1 = TextureGenerator.CopyTextureToRT(texture1);
            TAMGeneratorShader.SetTexture(csPackStrokesKernelID, PACK_R_TEXTURE, tmp1);
            RenderTexture tmp2;
            if (texture2 != null)
                tmp2 = TextureGenerator.CopyTextureToRT(texture2);
            else
                tmp2 = TextureGenerator.CopyTextureToRT(Texture2D.whiteTexture);
            TAMGeneratorShader.SetTexture(csPackStrokesKernelID, PACK_G_TEXTURE, tmp2);
            RenderTexture tmp3 = null;
            if (texture3 != null)
                tmp3 = TextureGenerator.CopyTextureToRT(texture3);
            else
                tmp3 = TextureGenerator.CopyTextureToRT(Texture2D.whiteTexture);
            TAMGeneratorShader.SetTexture(csPackStrokesKernelID, PACK_B_TEXTURE, tmp3);

            TAMGeneratorShader.Dispatch(csPackStrokesKernelID, csPackStrokesKernelThreads.x, csPackStrokesKernelThreads.y, csPackStrokesKernelThreads.z);
            tmp1.Release();
            if(tmp2 != null)
                tmp2.Release();
            if(tmp3 != null)
                tmp3.Release();
        }
        
        #endregion
        
        #region TAM Asset
        
        internal static void GenerateTAMToneTextures()
        {
            if(!IsGeneratorValid())
                return;
            
            if(Generating)
                return;
            
            //Force Clear
            TextureGenerator.PrepareGeneratorForRender();
            ConfigureGeneratorData();
            ClearAndReleaseTAMTones();
            OnTonalArtMapGenerationStep?.Invoke(CreateStatusArgs("Preparing Data...", 0f));
            currentRoutine = StaticCoroutine.StartCoroutine(GenerateTAMTonesRoutine());
            Generating = true;
        }

        private static void ClearAndReleaseTAMTones()
        {
            for (int i = 0; i < TonalArtMapAsset.Tones.Length; i++)
            {
                if(TonalArtMapAsset.Tones[i] != null)
                    TextureAssetManager.ClearTexture(TonalArtMapAsset.Tones[i]);
            }
            TonalArtMapAsset.ResetTones();
        }
        
        internal static void ApplyStrokesUntilFillRateAchieved()
        {
            StaticCoroutine.StartCoroutine(ApplyStrokesUntilFillRateRoutine(TargetFillRate));
        }

        private static IEnumerator ApplyStrokesUntilFillRateRoutine(float targetFillRate, float achievedFillRate = 0)
        {
            int maxStrokesPerFrame = 10;
            int strokesApplied = 0;
            while (achievedFillRate < targetFillRate)
            {
                ConfigureStrokesBuffer(achievedFillRate);
                if (Application.isPlaying)
                {
                    strokesApplied = 0;
                    while (strokesApplied < maxStrokesPerFrame)
                    {
                        achievedFillRate = ApplyStrokeKernel(strokesApplied);
                        if (achievedFillRate > targetFillRate)
                            break;
                        strokesApplied++;
                    }
                    yield return null;
                }
                else
                {
                    achievedFillRate = ApplyStrokeKernel(strokesApplied);
                    strokesApplied++;
                }
            }
        }

        private static IEnumerator GenerateTAMTonesRoutine()
        {
            float currentFillRate = 0;
            float expectedFillRateThreshold = TonalArtMapAsset.GetHomogenousFillRateThreshold();
            for (int i = 0; i < TonalArtMapAsset.ExpectedTones; i++)
            {
                float target = currentFillRate + expectedFillRateThreshold;
                if(i == 0)
                    target = TonalArtMapAsset.ForceFirstToneFullWhite ? 0f : target;
                else if (i == TonalArtMapAsset.ExpectedTones - 1)
                    target = TonalArtMapAsset.ForceFinalToneFullBlack ? 1f : Mathf.Lerp(currentFillRate, 1f, 0.5f);
                OnTonalArtMapGenerationStep?.Invoke(CreateStatusArgs($"Creating Tonal Value Texture {i}...", (float)(i * 2f)/(float)(TonalArtMapAsset.ExpectedTones * 2f)));
                yield return StaticCoroutine.StartCoroutine(ApplyStrokesUntilFillRateRoutine(target, currentFillRate));
                OnTonalArtMapGenerationStep?.Invoke(CreateStatusArgs($"Creating Tonal Value Texture {i}...", (float)(i * 2f + 1)/(float)(TonalArtMapAsset.ExpectedTones * 2f)));
                Texture2D output = DispatchSaveTexture(true, $"Tone_{i}");
                if (output == null)
                {
                    yield break;
                }
                TonalArtMapAsset.Tones[i] = output;
                currentFillRate += expectedFillRateThreshold;
                if(Application.isPlaying)
                    yield return null;
            }
            if(PackTAMTextures)
                PackAllTAMTextures();

            TonalArtMapAsset.TAMBasisDirection = StrokeDataAsset.StrokeData.Direction;
            ReleaseBuffers();
            
            EditorUtility.SetDirty(TonalArtMapAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            currentRoutine = null;
            Generating = false;
            OnTonalArtMapGenerated?.Invoke();
        }

        private static void PackAllTAMTextures()
        {
            if(TonalArtMapAsset == null || TonalArtMapAsset.Tones.Length == 0)
                return;
            
            TextureGenerator.PrepareGeneratorForRender();
            TextureGenerator.OverwriteGeneratorDimension(TonalArtMapAsset.Tones[0].width);
            PrepareComputeData();
            
            List<Texture2D> packedTAMs = new List<Texture2D>();
            for (int i = 0; i < TonalArtMapAsset.ExpectedTones; i += 3)
            {
                OnTonalArtMapGenerationStep?.Invoke(CreateStatusArgs($"Packing Tones{i}-{i+2}...", (float)i/((float)TonalArtMapAsset.ExpectedTones/3f)));
                bool isReduced = i + 1 >= TonalArtMapAsset.Tones.Length;
                bool isFullReduced = i + 2 >= TonalArtMapAsset.Tones.Length;
                if (!isReduced && !isFullReduced)
                    CombineStrokeTextures(TonalArtMapAsset.Tones[i], TonalArtMapAsset.Tones[i + 1], TonalArtMapAsset.Tones[i + 2]);
                else if (!isReduced && isFullReduced)
                {
                    CombineStrokeTextures(TonalArtMapAsset.Tones[i], TonalArtMapAsset.Tones[i + 1]);
                }
                else
                {
                    CombineStrokeTextures(TonalArtMapAsset.Tones[i]);
                }
                
                Texture2D packedTAM = DispatchSaveTexture(true, $"PackedTAM_{i}_{(isFullReduced ? i + 1 : i + 2)}");
                packedTAMs.Add(packedTAM);
            }
            ClearAndReleaseTAMTones();
            TonalArtMapAsset.SetPackedTams(packedTAMs.ToArray());
        }

        private static Texture2D DispatchSaveTexture(bool overwriteExisting, string fileNameOverwrite)
        {
            TextureGenerator.OverwriteGeneratorOutputSettings(fileNameOverwrite, DefaultFileOutputPath);
            try
            {
                return TextureGenerator.SaveCurrentTargetTexture(overwriteExisting, fileNameOverwrite);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if(Generating)
                    Generating = false;
                return null;
            }
        }
        
        #endregion
    }
}