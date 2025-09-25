using System;
using SketchRenderer.Editor.Utils;
using SketchRenderer.Runtime.Data;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.Rendering
{
    internal static class SketchRendererContextWizard
    {
        internal static SketchRendererContext CreateSketchRendererContext()
        {
            return CreateSketchRendererContext(SketchRendererData.DefaultPackageAssetDirectoryPath);
        }
        
        internal static SketchRendererContext CreateSketchRendererContext(string path)
        {
            return SketchAssetCreationWrapper.CreateScriptableInstance<SketchRendererContext>(path, forceFocus:false);
        }
    }
}