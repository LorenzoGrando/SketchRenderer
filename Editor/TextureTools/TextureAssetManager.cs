using System;
using System.IO;
using SketchRenderer.Editor.Utils;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SketchRenderer.Editor.TextureTools
{
    public static class TextureAssetManager
    {
        private const string IMAGE_FORMAT_IDENTIFIER = ".png";

        public static int GetTextureResolution(TextureResolution resolution)
        {
            switch (resolution)
            {
                case TextureResolution.SIZE_256:
                    return 256;
                case TextureResolution.SIZE_512:
                    return 512;
                case TextureResolution.SIZE_1024:
                    return 1024;
                default:
                    return 520;
            }
        }
        
        public static string GetAssetPath(Object asset)
        {
            if(AssetDatabase.Contains(asset))
            {
                return AssetDatabase.GetAssetPath(asset);
            }
            
            return null;
        }

        private static string GetCompleteTextureAssetPath(string fileNamePath)
        {
            return fileNamePath + IMAGE_FORMAT_IDENTIFIER;
        }

        public static Texture2D OutputToAssetTexture(RenderTexture tex, string folderPath, string fileName, bool overwrite)
        {
            if (tex == null)
                throw new ArgumentNullException(nameof(tex));
            
            RenderTexture.active = tex;
            Texture2D outputTexture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
            outputTexture.name = fileName;
            outputTexture.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
            outputTexture.Apply(false, false);
            outputTexture.hideFlags = HideFlags.HideAndDontSave;
            
            if (!SketchAssetCreationWrapper.TryValidateOrCreateAssetPath(folderPath))
                throw new UnityException("Failed to create texture at specified folder");
            
            string targetPath = Path.Combine(folderPath, fileName);
            string assetPath = GetCompleteTextureAssetPath(targetPath);

            if (AssetDatabase.AssetPathExists(assetPath))
            {
                if (overwrite)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
                else
                {
                    int copyCount = 1;
                    while (AssetDatabase.AssetPathExists(GetCompleteTextureAssetPath(targetPath + $"_{copyCount}")))
                        copyCount++;
                    targetPath += $"_{copyCount}";
                }
            }

            targetPath = GetCompleteTextureAssetPath(targetPath);
            byte[] bytes = outputTexture.EncodeToPNG();
            File.WriteAllBytes(targetPath, bytes);
            
            RenderTexture.active = null;
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Object.DestroyImmediate(outputTexture);
            //Return a reference to the created asset
            return AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
        }
        
        public static Texture2D OutputToAssetTexture(RenderTexture tex, string folderPath, string fileName, bool overwrite, TextureImporterType textureType = TextureImporterType.Default)
        {
            if (tex == null)
                throw new ArgumentNullException(nameof(tex));
            

            if (!SketchAssetCreationWrapper.TryValidateOrCreateAssetPath(folderPath))
                throw new UnityException("Failed to create texture at specified folder");
            
            RenderTexture.active = tex;
            Texture2D outputTexture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
            outputTexture.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
            outputTexture.Apply(false, false);
            outputTexture.hideFlags = HideFlags.HideAndDontSave;
            
            string targetPath = Path.Combine(folderPath, fileName);
            string assetPath = GetCompleteTextureAssetPath(targetPath);
            if (AssetDatabase.AssetPathExists(assetPath))
            {
                if (overwrite)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
                else
                {
                    int copyCount = 1;
                    while (AssetDatabase.AssetPathExists(GetCompleteTextureAssetPath(targetPath + $"_{copyCount}")))
                        copyCount++;
                    targetPath += $"_{copyCount}";
                }
            }

            targetPath = GetCompleteTextureAssetPath(targetPath);
            byte[] bytes = outputTexture.EncodeToPNG();
            File.WriteAllBytes(targetPath, bytes);
            
            RenderTexture.active = null;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Object.DestroyImmediate(outputTexture);
            
            TextureImporter importer = TextureImporter.GetAtPath(targetPath) as TextureImporter;
            importer.textureType = textureType;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            //Return a reference to the created asset
            return AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
        }

        public static void ClearTexture(Texture2D texture)
        {
            if(texture == null)
                throw new ArgumentNullException(nameof(texture));
            
            if (AssetDatabase.Contains(texture))
            {
                AssetDatabase.DeleteAsset(GetAssetPath(texture));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        
        public static void ClearTexture(Texture2D texture, string rootPath)
        {
            if(texture == null)
                throw new ArgumentNullException(nameof(texture));
            
            ClearTexture(texture);
        }
    }
}