using System;
using SketchRenderer.Editor.Rendering;
using SketchRenderer.Editor.Utils;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.TextureTools.Strokes;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.TextureTools.Strokes
{
    internal static class StrokeAssetWizard
    {
        internal static StrokeAsset CreateStrokeAsset(StrokeSDFType type)
        {
            return CreateStrokeAsset(type, SketchRendererData.DefaultPackageAssetDirectoryPath);
        }
        
        internal static StrokeAsset CreateStrokeAsset(StrokeSDFType type, string path)
        {
            try
            {
                string validatedPath = SketchAssetCreationWrapper.ConvertToAssetsPath(path);
                SketchAssetCreationWrapper.TryValidateOrCreateAssetPath(validatedPath);

                StrokeAsset strokeAsset = CreateByType(type);
                string assetName = nameof(StrokeAsset) + ".asset";
                int copiesCount = 0;
                while (AssetDatabase.AssetPathExists(validatedPath + "/" +  assetName))
                {
                    copiesCount++;
                    assetName = $"{nameof(StrokeAsset)}_{copiesCount}.asset";
                }
                string assetPath = validatedPath + "/" +  assetName;
                AssetDatabase.CreateAsset(strokeAsset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return AssetDatabase.LoadAssetAtPath<StrokeAsset>(assetPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private static StrokeAsset CreateByType(StrokeSDFType sdfType)
        {
            switch (sdfType)
            {
                case StrokeSDFType.SIMPLE:
                    StrokeAsset asset = ScriptableObject.CreateInstance<StrokeAsset>();
                    asset.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.Strokes.DefaultSimpleStroke);
                    return asset;
                case StrokeSDFType.HATCHING:
                    HatchingStrokeAsset hatchingAsset = ScriptableObject.CreateInstance<HatchingStrokeAsset>();
                    hatchingAsset.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.Strokes.DefaultHatchingStroke);
                    return hatchingAsset;
                case StrokeSDFType.ZIGZAG:
                    ZigzagStrokeAsset zigzagAsset = ScriptableObject.CreateInstance<ZigzagStrokeAsset>();
                    zigzagAsset.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.Strokes.DefaultZigzagStroke);
                    return zigzagAsset;
                case StrokeSDFType.FEATHERING:
                    FeatheringStrokeAsset featheringAsset = ScriptableObject.CreateInstance<FeatheringStrokeAsset>();
                    featheringAsset.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.Strokes.DefaultFeatheringStroke);
                    return featheringAsset;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sdfType), sdfType, null);
            }
        }
    }
}