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
            return CreateByType(type, path);
        }

        private static StrokeAsset CreateByType(StrokeSDFType sdfType, string path)
        {
            switch (sdfType)
            {
                case StrokeSDFType.SIMPLE:
                    StrokeAsset asset = SketchAssetCreationWrapper.CreateScriptableInstance<StrokeAsset>(path);
                    asset.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.Strokes.DefaultSimpleStroke);
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssetIfDirty(asset);
                    return asset;
                case StrokeSDFType.HATCHING:
                    HatchingStrokeAsset hatchingAsset = SketchAssetCreationWrapper.CreateScriptableInstance<HatchingStrokeAsset>(path);
                    hatchingAsset.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.Strokes.DefaultSimpleStroke);
                    EditorUtility.SetDirty(hatchingAsset);
                    AssetDatabase.SaveAssetIfDirty(hatchingAsset);
                    return hatchingAsset;
                case StrokeSDFType.ZIGZAG:
                    ZigzagStrokeAsset zigzagAsset = SketchAssetCreationWrapper.CreateScriptableInstance<ZigzagStrokeAsset>(path);
                    zigzagAsset.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.Strokes.DefaultSimpleStroke);
                    EditorUtility.SetDirty(zigzagAsset);
                    AssetDatabase.SaveAssetIfDirty(zigzagAsset);
                    return zigzagAsset;
                case StrokeSDFType.FEATHERING:
                    FeatheringStrokeAsset featheringAsset = SketchAssetCreationWrapper.CreateScriptableInstance<FeatheringStrokeAsset>(path);
                    featheringAsset.CopyFrom(SketchRendererManager.ResourceAsset.Scriptables.Strokes.DefaultSimpleStroke);
                    EditorUtility.SetDirty(featheringAsset);
                    AssetDatabase.SaveAssetIfDirty(featheringAsset);
                    return featheringAsset;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sdfType), sdfType, null);
            }
        }
    }
}