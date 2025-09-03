using SketchRenderer.Editor.Rendering;
using SketchRenderer.Editor.TextureTools;
using SketchRenderer.Runtime.Data;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.TextureTools
{
    [InitializeOnLoad]
    internal static class TextureToolWizard
    {
        static TextureToolWizard()
        {
            ValidateTonalArtMapWindow();
        }
        
        [MenuItem(SketchRendererData.PackageMenuItemPath + SketchRendererData.PackageMenuTextureToolSubPath + "Tonal Art Map Generator", false)]
        internal static void CreateTonalArtMapWindow()
        {
            if (TonalArtMapGeneratorWindow.window != null)
            {
                TonalArtMapGeneratorWindow.window.Focus(); 
                return;
            }
            
            TonalArtMapGeneratorWindow tamWindow = EditorWindow.GetWindow<TonalArtMapGeneratorWindow>();
            tamWindow.minSize = tamWindow.ExpectedMinWindowSize;
            tamWindow.maxSize = tamWindow.ExpectedMaxWindowSize;
            tamWindow.InitializeTool(SketchRendererManager.ResourceAsset);
            tamWindow.Show();
            TonalArtMapGeneratorWindow.window = tamWindow;
        }

        internal static void DestroyTonalArtMapWindow()
        {
            TonalArtMapGeneratorWindow.window.FinalizeTool();
            TonalArtMapGeneratorWindow.window.Close();
            TonalArtMapGeneratorWindow.window = (TonalArtMapGeneratorWindow) null;
        }

        internal static void ValidateTonalArtMapWindow()
        {
            if (EditorWindow.HasOpenInstances<TonalArtMapGeneratorWindow>())
            {
                if (TonalArtMapGeneratorWindow.window == null)
                {
                    //Do this instead of EditorWindow.GetWindow so we find the window regardless of docked state
                    TonalArtMapGeneratorWindow.window = Resources.FindObjectsOfTypeAll<TonalArtMapGeneratorWindow>()[0];
                }

                TonalArtMapGeneratorWindow.window.InitializeTool(SketchRendererManager.ResourceAsset);
            }
        }
    }
}