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
            MaterialDataAsset materialData = SketchAssetCreationWrapper.CreateScriptableInstance<MaterialDataAsset>(path);
            materialData.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.MaterialData);
            return materialData;
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