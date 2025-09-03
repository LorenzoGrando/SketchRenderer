using SketchRenderer.Runtime.Data;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.Rendering
{
    public class SketchRendererWizard
    {
        [MenuItem(SketchRendererData.PackageMenuItemPath + "Initialize Sketch Renderer with default settings", true)]
        private static bool InitializeDefaultInRendererValidation()
        {
            return !Application.isPlaying;
        }

        [MenuItem(SketchRendererData.PackageMenuItemPath + "Initialize Sketch Renderer with default settings")]
        private static void InitializeDefaultInRenderer()
        {
            SketchRendererManager.UpdateRendererToCurrentContext();
        }
    }
}