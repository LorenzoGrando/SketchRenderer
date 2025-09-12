using System;
using SketchRenderer.Editor.Rendering;
using SketchRenderer.Editor.Utils;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using TextureTools.Material;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.TextureTools
{
    internal static class MaterialGeneratorWizard
    {
        internal static MaterialDataAsset CreateMaterialDataAsset()
        {
            return CreateMaterialDataAsset(SketchRendererData.DefaultPackageAssetDirectoryPath);
        }
        
        internal static MaterialDataAsset CreateMaterialDataAsset(string path)
        {
            try
            {
                string validatedPath = SketchAssetCreationWrapper.ConvertToAssetsPath(path);
                SketchAssetCreationWrapper.TryValidateOrCreateAssetPath(validatedPath);

                MaterialDataAsset materialDataAsset = ScriptableObject.CreateInstance<MaterialDataAsset>();
                materialDataAsset.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.MaterialData);
                string assetName = nameof(MaterialDataAsset) + ".asset";
                int copiesCount = 0;
                while (AssetDatabase.AssetPathExists(validatedPath + "/" +  assetName))
                {
                    copiesCount++;
                    assetName = $"{nameof(MaterialDataAsset)}_{copiesCount}.asset";
                }
                string assetPath = validatedPath + "/" +  assetName;
                AssetDatabase.CreateAsset(materialDataAsset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return AssetDatabase.LoadAssetAtPath<MaterialDataAsset>(assetPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
        
        internal static void SetAsActiveAlbedo(Texture albedoTexture)
        {
            if(albedoTexture == null)
                throw new ArgumentNullException(nameof(albedoTexture));
            
            if (SketchRendererManager.CurrentRendererContext != null)
            {
                SketchRendererManager.CurrentRendererContext.MaterialFeatureData.AlbedoTexture = albedoTexture as Texture2D;
                EditorUtility.SetDirty(SketchRendererManager.CurrentRendererContext);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                SketchRendererManager.UpdateFeatureByCurrentContext(SketchRendererFeatureType.MATERIAL);
            }
        }
        
        internal static void SetAsActiveDirectional(Texture directionalTexture)
        {
            if(directionalTexture == null)
                throw new ArgumentNullException(nameof(directionalTexture));
            
            if (SketchRendererManager.CurrentRendererContext != null)
            {
                SketchRendererManager.CurrentRendererContext.MaterialFeatureData.NormalTexture = directionalTexture as Texture2D;
                EditorUtility.SetDirty(SketchRendererManager.CurrentRendererContext);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                SketchRendererManager.UpdateFeatureByCurrentContext(SketchRendererFeatureType.MATERIAL);
            }
        }
    }
}