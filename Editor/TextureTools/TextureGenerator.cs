using System.IO;
using SketchRenderer.Runtime.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SketchRenderer.Editor.TextureTools
{
    internal static class TextureGenerator
    {
        private static RenderTexture targetRT;

        internal static RenderTexture TargetRT
        {
            get
            {
                if(targetRT == null)
                    CreateOrClearTarget();
                return targetRT;
            }
        }
        
        
        internal static TextureResolution Resolution;
        internal static int Dimension
        {
            get; private set;
        }
        internal static TextureImporterType TextureImporterType;
        
        
        internal static string OverwriteFileOutputPath;
        internal static string DefaultFileOutputPath
        {
            get
            {
                return Path.Combine("Assets", SketchRendererData.PackageDisplayName);
            }
        }
        
        internal static string OverwriteFileOutputName;
        internal static string DefaultFileOutputName
        {
            get
            {
                return "SketchTexture";
            }
        }
        
        private static string GetTextureOutputPath()
        {
            if(!string.IsNullOrEmpty(OverwriteFileOutputPath))
                return OverwriteFileOutputPath;
            
            return DefaultFileOutputPath;
        }

        private static string GetTextureOutputName()
        {
            if(!string.IsNullOrEmpty(OverwriteFileOutputName))
                return OverwriteFileOutputName;
            
            return DefaultFileOutputName;
        }

        internal static void OverwriteGeneratorOutputSettings(string fileName, string path)
        {
            OverwriteFileOutputName = fileName;
            OverwriteFileOutputPath = path;
        }

        internal static void ResetGeneratorOutputSettings()
        {
            OverwriteFileOutputName = string.Empty;
            OverwriteFileOutputPath = string.Empty;
        }
        
        internal static void OverwriteGeneratorDimension(int dimension)
        {
            Dimension = dimension;
            CreateOrClearTarget();
        }

        internal static void PrepareGeneratorForRender()
        {
            ResetGeneratorOutputSettings();
            Dimension = TextureAssetManager.GetTextureResolution(Resolution);
            CreateOrClearTarget();
        }
        
        #region Asset Preparation
        
        internal static void CreateOrClearTarget()
        {
            if (targetRT != null)
            {
                if(RenderTexture.active == targetRT)
                    RenderTexture.active = null;
                targetRT.Release();
                targetRT = null;
            }
            
            if(Dimension <= 0)
                Dimension = TextureAssetManager.GetTextureResolution(Resolution);

            targetRT = CreateRT(Dimension);
            targetRT.hideFlags = HideFlags.HideAndDontSave;
        }
        
        internal static RenderTexture CreateRT(int dimension)
        {
            RenderTexture rt = new RenderTexture(dimension, dimension, GraphicsFormat.R8G8B8A8_SRGB, GraphicsFormat.None);
            rt.enableRandomWrite = true;
            rt.hideFlags = HideFlags.HideAndDontSave;
            Graphics.Blit(Texture2D.whiteTexture, rt);
            return rt;
        }
        
        internal static RenderTexture CopyToRT(RenderTexture copy)
        {
            RenderTexture rt = new RenderTexture(copy);
            rt.enableRandomWrite = true;
            rt.hideFlags = HideFlags.HideAndDontSave;
        
            return rt;
        }
    
        internal static RenderTexture CopyTextureToRT(Texture2D copy)
        {
            RenderTexture rt = new RenderTexture(copy.width, copy.height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None);
            rt.enableRandomWrite = true;
            rt.hideFlags = HideFlags.HideAndDontSave;
            rt.Create();
            RenderTexture.active = rt;
            Graphics.Blit(copy, rt);
            RenderTexture.active = null;
            return rt;
        }
        
        #endregion
        
        #region Asset Management
        
        internal static Texture2D SaveCurrentTargetTexture(bool overwrite, string fileName = null)
        {
            string path = GetTextureOutputPath();
            
            if(!string.IsNullOrEmpty(fileName))
                OverwriteFileOutputName = fileName;
            
            string name = GetTextureOutputName();
        
            return TextureAssetManager.OutputToAssetTexture(TargetRT, path, name, overwrite);
        }
        
        internal static Texture2D SaveCurrentTargetTexture(TextureImporterType texType, bool overwrite, string fileName = null)
        {
            TextureImporterType = texType;

            string path = GetTextureOutputPath();
            
            if(!string.IsNullOrEmpty(fileName))
                OverwriteFileOutputName = fileName;
            
            string name = GetTextureOutputName();
        
            return TextureAssetManager.OutputToAssetTexture(TargetRT, path, name, overwrite, TextureImporterType);
        }
        
        #endregion
    }
}