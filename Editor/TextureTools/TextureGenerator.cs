using System;
using System.IO;
using SketchRenderer.Runtime.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SketchRenderer.Editor.TextureTools
{
    internal static class TextureGenerator
    {
        internal static event Action<RenderTexture> OnRecreateTargetTexture;
        
        internal static TextureImporterType TextureImporterType;
        
        
        internal static string OverwriteFileOutputPath;
        internal static string DefaultFileOutputPath
        {
            get
            {
                return SketchRendererData.DefaultPackageAssetDirectoryPath;
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

        internal static void PrepareGeneratorForRender()
        {
            ResetGeneratorOutputSettings();
        }
        
        #region Asset Preparation
        
        internal static void CreateOrClearTarget(ref RenderTexture targetRT, int dimension)
        {
            if (targetRT != null)
            {
                if(RenderTexture.active == targetRT)
                    RenderTexture.active = null;
                targetRT.Release();
            }

            targetRT = CreateRT(dimension);
            targetRT.hideFlags = HideFlags.HideAndDontSave;
            OnRecreateTargetTexture?.Invoke(targetRT);
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

        internal static void BlitToTargetTexture(RenderTexture targetRT, Material blitMat, int pass = 0)
        {
            Graphics.Blit(null, targetRT,  blitMat, pass);
        }
        
        internal static Texture2D SaveCurrentTargetTexture(RenderTexture targetRT, bool overwrite, string fileName = null)
        {
            string path = GetTextureOutputPath();
            
            if(!string.IsNullOrEmpty(fileName))
                OverwriteFileOutputName = fileName;
            
            string name = GetTextureOutputName();
        
            return TextureAssetManager.OutputToAssetTexture(targetRT, path, name, overwrite);
        }
        
        internal static Texture2D SaveCurrentTargetTexture(RenderTexture targetRT, TextureImporterType texType, bool overwrite, string fileName = null)
        {
            TextureImporterType = texType;

            string path = GetTextureOutputPath();
            
            if(!string.IsNullOrEmpty(fileName))
                OverwriteFileOutputName = fileName;
            
            string name = GetTextureOutputName();
        
            return TextureAssetManager.OutputToAssetTexture(targetRT, path, name, overwrite, TextureImporterType);
        }
        
        #endregion
    }
}