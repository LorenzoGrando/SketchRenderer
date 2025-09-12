using System;
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
                    return ScriptableObject.CreateInstance<StrokeAsset>();
                case StrokeSDFType.HATCHING:
                    return ScriptableObject.CreateInstance<HatchingStrokeAsset>();
                case StrokeSDFType.ZIGZAG:
                    return ScriptableObject.CreateInstance<ZigzagStrokeAsset>();
                case StrokeSDFType.FEATHERING:
                    return ScriptableObject.CreateInstance<FeatheringStrokeAsset>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(sdfType), sdfType, null);
            }
        }
    }
}