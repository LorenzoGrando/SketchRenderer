using System;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.TextureTools.Strokes;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;
using TextureTools.Material;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Editor.TextureTools
{
    public static class MaterialGenerator
    {
        internal static event Action OnTextureUpdated;
        internal static event Action<TextureToolGenerationStatusArgs> OnMaterialGenerationStep; 
        
        internal static string DefaultFileOutputName
        {
            get
            {
                switch (TextureOutputType)
                {
                    case TextureImporterType.Default:
                        return "MaterialAlbedoTexture";
                    case TextureImporterType.NormalMap:
                        return "MaterialDirectionalTexture";
                    default:
                        return "MaterialTexture";
                }
            }
        }
        internal static string DefaultFileOutputPath => string.IsNullOrEmpty(OverrideOutputPath) ? TextureGenerator.DefaultFileOutputPath : OverrideOutputPath;
        internal static string OverrideOutputPath;
        
        private static Shader MaterialGeneratorShader;
        private static Material MaterialGeneratorMaterial;
        private static MaterialDataAsset defaultMaterialDataAsset;
        private static MaterialDataAsset materialDataAsset;
        internal static MaterialDataAsset MaterialDataAsset
        {
            get => materialDataAsset;
            set
            {
                materialDataAsset = value;
                hasNonDefaultMaterialDataAsset = materialDataAsset != null && materialDataAsset == defaultMaterialDataAsset;
            }
        }
        private static bool hasNonDefaultMaterialDataAsset;
        internal static TextureResolution TargetResolution = TextureResolution.SIZE_512;
        private static TextureImporterType textureOutputType = TextureImporterType.Default;
        internal static TextureImporterType TextureOutputType
        {
            get { return textureOutputType; } private set { textureOutputType = value; }
        }
        
        //Shader Data
        private const int ALBEDO_PASS_ID = 0;
        private const int DIRECTIONAL_PASS_ID = 1;
        
        //Shader Properties
        //Granularity
        private static readonly int GRANULARITY_SCALE_ID = Shader.PropertyToID("_GranularityScale");
        private static readonly int GRANULARITY_OCTAVES_ID = Shader.PropertyToID("_GranularityOctaves");
        private static readonly int GRANULARITY_LACUNARITY_ID = Shader.PropertyToID("_GranularityLacunarity");
        private static readonly int GRANULARITY_PERSISTENCE_ID = Shader.PropertyToID("_GranularityPersistence");
        private static readonly int GRANULARITY_VALUE_RANGES_ID = Shader.PropertyToID("_GranularityValueRange");
        private static readonly int GRANULARITY_TINT_ID = Shader.PropertyToID("_GranularityTint");
        
        //Laid Lines
        private static readonly int LAID_LINE_FREQUENCY_ID = Shader.PropertyToID("_LaidLineFrequency");
        private static readonly int LAID_LINE_THICKNESS_ID = Shader.PropertyToID("_LaidLineThickness");
        private static readonly int LAID_LINE_STRENGTH_ID = Shader.PropertyToID("_LaidLineStrength");
        private static readonly int LAID_LINE_GRANULARITY_DISPLACEMENT_ID = Shader.PropertyToID("_LaidLineDisplacement");
        private static readonly int LAID_LINE_GRANULARITY_MASK_ID = Shader.PropertyToID("_LaidLineMask");
        private static readonly int LAID_LINE_TINT_ID = Shader.PropertyToID("_LaidLineTint");
        
        //Crumples
        private static readonly int CRUMPLES_SCALE_ID = Shader.PropertyToID("_CrumplesScale");
        private static readonly int CRUMPLES_JITTER_ID = Shader.PropertyToID("_CrumplesJitter");
        private static readonly int CRUMPLES_STRENGTH_ID = Shader.PropertyToID("_CrumplesStrength");
        private static readonly int CRUMPLES_OCTAVES_ID = Shader.PropertyToID("_CrumplesOctaves");
        private static readonly int CRUMPLES_LACUNARITY_ID = Shader.PropertyToID("_CrumplesLacunarity");
        private static readonly int CRUMPLES_PERSISTENCE_ID = Shader.PropertyToID("_CrumplesPersistence");
        private static readonly int CRUMPLES_TINT_STRENGTH_ID = Shader.PropertyToID("_CrumplesTintStrength");
        private static readonly int CRUMPLES_SHARPNESS_ID = Shader.PropertyToID("_CrumplesTintSharpness");
        private static readonly int CRUMPLES_TINT_ID = Shader.PropertyToID("_CrumplesTint");
        
        //Notebook Lines
        private static readonly int NOTEBOOK_FREQUENCY_ID = Shader.PropertyToID("_NotebookLineFrequency");
        private static readonly int NOTEBOOK_OFFSET_ID = Shader.PropertyToID("_NotebookLinePhase");
        private static readonly int NOTEBOOK_THICKNESS_ID = Shader.PropertyToID("_NotebookLineSize");
        private static readonly int NOTEBOOK_GRANULARITY_SENSITIVITY_ID = Shader.PropertyToID("_NotebookLineGranularitySensitivity");
        private static readonly int NOTEBOOK_HORIZONTAL_TINT_ID = Shader.PropertyToID("_NotebookLineHorizontalTint");
        private static readonly int NOTEBOOK_VERTICAL_TINT_ID = Shader.PropertyToID("_NotebookLineVerticalTint");
        
        //Shader Keywords
        private static readonly string USE_GRANULARITY_KEYWORD = "USE_GRANULARITY";
        private static readonly string USE_LAID_LINES_KEYWORD = "USE_LAID_LINES";
        private static readonly string USE_CRUMPLES_KEYWORD = "USE_CRUMPLES";
        private static readonly string USE_NOTEBOOK_LINES_KEYWORD = "USE_NOTEBOOK_LINES";

        private static LocalKeyword GranularityKeyword;
        private static LocalKeyword LaidLineKeyword;
        private static LocalKeyword CrumpleKeyword;
        private static LocalKeyword NotebookLineKeyword;
        
        internal static bool Generating { get; private set; }

        internal static Texture LastGeneratedAlbedoTexture {get; private set;}
        internal static Texture LastGeneratedDirectionalTexture { get; private set; }
        
        internal static void Init(SketchResourceAsset resources)
        {
            if (MaterialGeneratorShader == null)
                MaterialGeneratorShader = resources.Shaders.MaterialGenerator;
            if(MaterialGeneratorMaterial == null && MaterialGeneratorShader != null)
                MaterialGeneratorMaterial = new Material(MaterialGeneratorShader);

            defaultMaterialDataAsset = resources.Scriptables.MaterialData;
            if (MaterialDataAsset == null)
            {
                MaterialDataAsset = resources.Scriptables.MaterialData;
            }

            ConfigureGeneratorData();
            UpdateMaterialAlbedoTexture();
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
            
            PrepareData();
        }
        
        internal static bool IsGeneratorValid()
        {
            if(MaterialGeneratorShader == null || MaterialDataAsset == null || MaterialGeneratorMaterial == null)
                return false;

            return true;
        }
        
        internal static TextureToolGenerationStatusArgs CreateStatusArgs(string subProcess, float relativeProgress)
        {
            TextureToolGenerationStatusArgs args = new TextureToolGenerationStatusArgs();
            args.toolProcess = "Generating Material Textures";
            args.toolSubprocess = subProcess;
            args.toolTotalProgress = relativeProgress;
            return args;
        }

        private static void PrepareData()
        {
            ManageMaterialDataKeywords();
            UpdateShaderMaterialData();
        }

        private static void ManageMaterialDataKeywords()
        {
            GranularityKeyword = new LocalKeyword(MaterialGeneratorShader, USE_GRANULARITY_KEYWORD);
            MaterialGeneratorMaterial.SetKeyword(GranularityKeyword, MaterialDataAsset.UseGranularity);

            LaidLineKeyword = new LocalKeyword(MaterialGeneratorShader, USE_LAID_LINES_KEYWORD);
            MaterialGeneratorMaterial.SetKeyword(LaidLineKeyword, MaterialDataAsset.UseLaidLines);
            
            CrumpleKeyword = new LocalKeyword(MaterialGeneratorShader, USE_CRUMPLES_KEYWORD);
            MaterialGeneratorMaterial.SetKeyword(CrumpleKeyword, MaterialDataAsset.UseCrumples);
            
            NotebookLineKeyword = new LocalKeyword(MaterialGeneratorShader, USE_NOTEBOOK_LINES_KEYWORD);
            MaterialGeneratorMaterial.SetKeyword(NotebookLineKeyword, MaterialDataAsset.UseNotebookLines);
        }

        private static void UpdateShaderMaterialData()
        {
            if (MaterialDataAsset.UseGranularity)
            {
                MaterialGeneratorMaterial.SetVector(GRANULARITY_SCALE_ID, new Vector4(MaterialDataAsset.Granularity.Scale.x, MaterialDataAsset.Granularity.Scale.y, 0, 0));
                MaterialGeneratorMaterial.SetInt(GRANULARITY_OCTAVES_ID, MaterialDataAsset.Granularity.DetailLevel);
                MaterialGeneratorMaterial.SetFloat(GRANULARITY_LACUNARITY_ID, MaterialDataAsset.Granularity.DetailFrequency);
                MaterialGeneratorMaterial.SetFloat(GRANULARITY_PERSISTENCE_ID, MaterialDataAsset.Granularity.DetailPersistence);
                MaterialGeneratorMaterial.SetVector(GRANULARITY_VALUE_RANGES_ID, new Vector4(MaterialDataAsset.Granularity.MinimumGranularity, MaterialDataAsset.Granularity.MaximumGranularity, 0, 0));
                MaterialGeneratorMaterial.SetVector(GRANULARITY_TINT_ID, MaterialDataAsset.Granularity.GranularityTint);
            }

            if (MaterialDataAsset.UseLaidLines)
            {
                MaterialGeneratorMaterial.SetFloat(LAID_LINE_FREQUENCY_ID, MaterialDataAsset.LaidLines.LineFrequency);
                MaterialGeneratorMaterial.SetFloat(LAID_LINE_THICKNESS_ID, MaterialDataAsset.LaidLines.LineThickness);
                MaterialGeneratorMaterial.SetFloat(LAID_LINE_STRENGTH_ID, MaterialDataAsset.LaidLines.LineStrength);
                MaterialGeneratorMaterial.SetFloat(LAID_LINE_GRANULARITY_DISPLACEMENT_ID, MaterialDataAsset.LaidLines.LineGranularityDisplacement);
                MaterialGeneratorMaterial.SetFloat(LAID_LINE_GRANULARITY_MASK_ID, MaterialDataAsset.LaidLines.LineGranularityMasking);
                MaterialGeneratorMaterial.SetVector(LAID_LINE_TINT_ID, MaterialDataAsset.LaidLines.LineTint);
            }

            if (MaterialDataAsset.UseCrumples)
            {
                MaterialGeneratorMaterial.SetVector(CRUMPLES_SCALE_ID, new Vector4(MaterialDataAsset.Crumples.CrumpleScale.x, MaterialDataAsset.Crumples.CrumpleScale.y, 0, 0));
                MaterialGeneratorMaterial.SetFloat(CRUMPLES_JITTER_ID, MaterialDataAsset.Crumples.CrumpleJitter);
                MaterialGeneratorMaterial.SetFloat(CRUMPLES_STRENGTH_ID, MaterialDataAsset.Crumples.CrumpleStrength);
                MaterialGeneratorMaterial.SetInt(CRUMPLES_OCTAVES_ID, MaterialDataAsset.Crumples.CrumpleDetailLevel);
                MaterialGeneratorMaterial.SetFloat(CRUMPLES_LACUNARITY_ID, MaterialDataAsset.Crumples.CrumpleDetailFrequency);
                MaterialGeneratorMaterial.SetFloat(CRUMPLES_PERSISTENCE_ID, MaterialDataAsset.Crumples.CrumpleDetailPersistence);
                MaterialGeneratorMaterial.SetFloat(CRUMPLES_TINT_STRENGTH_ID, MaterialDataAsset.Crumples.CrumpleTintStrength);
                MaterialGeneratorMaterial.SetFloat(CRUMPLES_SHARPNESS_ID, MaterialDataAsset.Crumples.CrumpleTintSharpness);
                MaterialGeneratorMaterial.SetVector(CRUMPLES_TINT_ID, MaterialDataAsset.Crumples.CrumpleTint);
            }

            if (MaterialDataAsset.UseNotebookLines)
            {
                MaterialGeneratorMaterial.SetVector(NOTEBOOK_FREQUENCY_ID, new Vector4(MaterialDataAsset.NotebookLines.HorizontalLineFrequency, MaterialDataAsset.NotebookLines.VerticalLineFrequency, 0, 0));
                MaterialGeneratorMaterial.SetVector(NOTEBOOK_OFFSET_ID, new Vector4(MaterialDataAsset.NotebookLines.HorizontalLineOffset, MaterialDataAsset.NotebookLines.VerticalLineOffset, 0, 0));
                MaterialGeneratorMaterial.SetVector(NOTEBOOK_THICKNESS_ID, new Vector4(MaterialDataAsset.NotebookLines.HorizontalLineThickness, MaterialDataAsset.NotebookLines.VerticalLineThickness, 0, 0));
                MaterialGeneratorMaterial.SetFloat(NOTEBOOK_GRANULARITY_SENSITIVITY_ID, MaterialDataAsset.NotebookLines.NotebookLineGranularitySensitivity);
                MaterialGeneratorMaterial.SetColor(NOTEBOOK_HORIZONTAL_TINT_ID, MaterialDataAsset.NotebookLines.HorizontalLineTint);
                MaterialGeneratorMaterial.SetColor(NOTEBOOK_VERTICAL_TINT_ID, MaterialDataAsset.NotebookLines.VerticalLineTint);
            }
        }
        
        #region Create Texture

        private static bool ValidateTextureGeneration()
        {
            if (!IsGeneratorValid())
            {
                TextureGenerator.PrepareGeneratorForRender();
                return false;
            }
            
            TextureGenerator.PrepareGeneratorForRender();
            ConfigureGeneratorData();
            
            return true;
        }

        internal static void UpdateMaterialAlbedoTexture()
        {
            if (ValidateTextureGeneration())
            {
                TextureOutputType = TextureImporterType.Default;
                BlitAlbedoTexture();
            }
        }
        
        internal static void UpdateMaterialDirectionalTexture()
        {
            if (ValidateTextureGeneration())
            {
                TextureOutputType = TextureImporterType.NormalMap;
                BlitDirectionalTexture();
            }
        }

        private static void BlitAlbedoTexture()
        {
            if(MaterialGeneratorMaterial == null)
                return;
            
            TextureGenerator.BlitToTargetTexture(MaterialGeneratorMaterial, ALBEDO_PASS_ID);
        }

        private static void BlitDirectionalTexture()
        {
            if(MaterialGeneratorMaterial == null)
                return;
            
            TextureGenerator.BlitToTargetTexture(MaterialGeneratorMaterial, DIRECTIONAL_PASS_ID);
        }

        internal static void GenerateAlbedoTexture()
        {
            if(!IsGeneratorValid())
                return;
            OnMaterialGenerationStep?.Invoke(CreateStatusArgs("Creating Albedo Texture", 0.25f));
            UpdateMaterialAlbedoTexture();
            ConfigureGeneratorData();
            LastGeneratedAlbedoTexture = TextureGenerator.SaveCurrentTargetTexture(TextureOutputType, overwrite: true);
        }

        internal static void GenerateDirectionalTexture()
        {
            if(!IsGeneratorValid())
                return;
            
            OnMaterialGenerationStep?.Invoke(CreateStatusArgs("Creating Normal Texture", 0.25f));
            UpdateMaterialDirectionalTexture();
            ConfigureGeneratorData();
            LastGeneratedDirectionalTexture = TextureGenerator.SaveCurrentTargetTexture(TextureOutputType, overwrite: true);
        }

        internal static void GenerateMaterialTextures()
        {
            Generating = true;
            GenerateAlbedoTexture();
            GenerateDirectionalTexture();
            Generating = false;
            OnTextureUpdated?.Invoke();
        }
        
        #endregion
    }
}