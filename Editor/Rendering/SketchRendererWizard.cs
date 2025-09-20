using SketchRenderer.Runtime.Data;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.Rendering
{
    public class SketchRendererWizard
    {
        [MenuItem(SketchRendererData.PackageMenuItemPath + "Settings...", false, priority:(int)SketchRendererData.MenuPriority.Major)]
        private static void OpenPackageSettings()
        {
            SettingsService.OpenProjectSettings(SketchRendererData.PackageProjectSettingsPath);
        }
        
        [MenuItem(SketchRendererData.PackageMenuItemPath + "Reset to Default Renderer Context", true)]
        private static bool InitializeDefaultInRendererValidation()
        {
            return !Application.isPlaying;
        }

        [MenuItem(SketchRendererData.PackageMenuItemPath + "Reset to Default Renderer Context", false, priority:(int)SketchRendererData.MenuPriority.Minor)]
        private static void InitializeDefaultInRenderer()
        {
            SketchRendererManager.UpdateRendererToDefaultContext();
        }
        
        [MenuItem(SketchRendererData.PackageMenuItemPath + "Initialize Sketch Renderer with Active Renderer Context", true)]
        private static bool InitializeCurrentInRendererValidation()
        {
            return !Application.isPlaying && SketchRendererManager.CurrentRendererContext != null;
        }

        [MenuItem(SketchRendererData.PackageMenuItemPath + "Initialize Sketch Renderer with Active Renderer Context", false, priority:(int)SketchRendererData.MenuPriority.Minor)]
        private static void InitializeCurrentInRenderer()
        {
            SketchRendererManager.UpdateRendererToCurrentContext();
        }
    }
}