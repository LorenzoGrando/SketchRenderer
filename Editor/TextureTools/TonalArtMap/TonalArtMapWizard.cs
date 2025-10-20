using System;
using System.IO;
using SketchRenderer.Editor.Rendering;
using SketchRenderer.Editor.Utils;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.TextureTools
{
    internal static class TonalArtMapWizard
    {
        internal static bool IsCurrentTonalArtMap(TonalArtMapAsset asset)
        {
            if(asset == null)
                return false;
            
            if(SketchRendererManager.CurrentRendererContext == null)
                return false;
            
            return SketchRendererManager.CurrentRendererContext.LuminanceFeatureData.ActiveTonalMap == asset;
        }
        
        internal static TonalArtMapAsset CreateTonalArtMapAndSetActive()
        {
            return CreateTonalArtMapAndSetActive(SketchRendererData.DefaultPackageAssetDirectoryPath);
        }
        
        internal static TonalArtMapAsset CreateTonalArtMapAndSetActive(string path)
        {
            TonalArtMapAsset asset = CreateTonalArtMap(path);
            SetAsCurrentTonalArtMap(asset);
            return asset;
        }

        internal static TonalArtMapAsset CreateTonalArtMap(string path)
        {
            return SketchAssetCreationWrapper.CreateScriptableInstance<TonalArtMapAsset>(path);
        }

        internal static void SetAsCurrentTonalArtMap(TonalArtMapAsset asset)
        {
            if(asset == null)
                throw new ArgumentNullException(nameof(asset));
            
            if (SketchRendererManager.CurrentRendererContext != null)
            {
                SketchRendererManager.CurrentRendererContext.LuminanceFeatureData.ActiveTonalMap = asset;
                EditorUtility.SetDirty(SketchRendererManager.CurrentRendererContext);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                SketchRendererManager.CurrentRendererContext.Redraw();
                
                SketchRendererManager.UpdateFeatureByCurrentContext(SketchRendererFeatureType.LUMINANCE);
            }
            else
                Debug.LogWarning("Couldn't set as active since there is no current assigned SketchRendererContext in the Sketch settings.");
        }
    }
}