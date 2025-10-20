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
        internal static bool hasDelayedTonalWindowCall;
        internal static bool hasDelayedMaterialWindowCall;
        static TextureToolWizard()
        {
            EditorApplication.playModeStateChanged += _ => ValidateTonalArtMapWindow();
            EditorApplication.playModeStateChanged += _ => ValidateMaterialWindow();
            
            //Delayed init on project open so layout has time to load.
            if (!SessionState.GetBool("SketchRendererTonalArtMapWindowInitialized", false))
            {
                EditorApplication.delayCall += ValidateTonalArtMapWindow;
                hasDelayedTonalWindowCall = true;
                SessionState.SetBool("SketchRendererTonalArtMapWindowInitialized", true);
            }
            else
                ValidateTonalArtMapWindow();
            
            if (!SessionState.GetBool("SketchRendererMaterialWindowInitialized", false))
            {
                EditorApplication.delayCall += ValidateMaterialWindow;
                hasDelayedMaterialWindowCall = true;
                SessionState.SetBool("SketchRendererMaterialWindowInitialized", true);
            }
            else
                ValidateMaterialWindow();
        }
        
        #region Tonal Art Map Generator
        
        [MenuItem(SketchRendererData.PackageMenuItemPath + SketchRendererData.PackageMenuTextureToolSubPath + "Tonal Art Map Generator", false, priority:(int)SketchRendererData.MenuPriority.Default)]
        internal static void CreateTonalArtMapWindow()
        {
            if (TonalArtMapGeneratorWindow.window != null)
            {
                TonalArtMapGeneratorWindow.window.Focus(); 
                return;
            }
            
            TonalArtMapGeneratorWindow tamWindow = EditorWindow.GetWindow<TonalArtMapGeneratorWindow>();
            tamWindow.OnWindowClosed += DestroyTonalArtMapWindow;
            tamWindow.InitializeTool(SketchRendererManager.ResourceAsset, SketchRendererManager.ManagerSettings);
            tamWindow.Show();
            TonalArtMapGeneratorWindow.window = tamWindow;
        }

        internal static void DestroyTonalArtMapWindow()
        {
            TonalArtMapGeneratorWindow.window.OnWindowClosed -= DestroyTonalArtMapWindow;
            TonalArtMapGeneratorWindow.window.FinalizeTool();
            TonalArtMapGeneratorWindow.window = (TonalArtMapGeneratorWindow) null;
        }

        internal static void ValidateTonalArtMapWindow()
        {
            if (SketchRendererManager.ResourceAsset != null && SketchRendererManager.ManagerSettings != null)
            {
                if (EditorWindow.HasOpenInstances<TonalArtMapGeneratorWindow>())
                {
                    if (TonalArtMapGeneratorWindow.window == null)
                    {
                        //Do this instead of EditorWindow.GetWindow so we find the window regardless of docked state
                        TonalArtMapGeneratorWindow.window =
                            Resources.FindObjectsOfTypeAll<TonalArtMapGeneratorWindow>()[0];
                    }

                    //Clear any existing buffers before they are lost
                    TonalArtMapGeneratorWindow.window.InitializeTool(SketchRendererManager.ResourceAsset,
                        SketchRendererManager.ManagerSettings);
                }
            }

            if (hasDelayedTonalWindowCall)
            {
                EditorApplication.delayCall -= ValidateTonalArtMapWindow;
                hasDelayedTonalWindowCall = false;
            }
        }
        
        #endregion
        
        #region Material Generator
        
        [MenuItem(SketchRendererData.PackageMenuItemPath + SketchRendererData.PackageMenuTextureToolSubPath + "Material Generator", false, priority:(int)SketchRendererData.MenuPriority.Default)]
        internal static void CreateMaterialWindow()
        {
            if (MaterialGeneratorWindow.window != null)
            {
                MaterialGeneratorWindow.window.Focus(); 
                return;
            }
            
            MaterialGeneratorWindow materialWindow = EditorWindow.GetWindow<MaterialGeneratorWindow>();
            materialWindow.OnWindowClosed += DestroyMaterialWindow;
            materialWindow.InitializeTool(SketchRendererManager.ResourceAsset, SketchRendererManager.ManagerSettings);
            materialWindow.Show();
            MaterialGeneratorWindow.window = materialWindow;
        }

        internal static void DestroyMaterialWindow()
        {
            MaterialGeneratorWindow.window.OnWindowClosed -= DestroyMaterialWindow;
            MaterialGeneratorWindow.window.FinalizeTool();
            MaterialGeneratorWindow.window = (MaterialGeneratorWindow) null;
        }

        internal static void ValidateMaterialWindow()
        {
            if (SketchRendererManager.ResourceAsset != null && SketchRendererManager.ManagerSettings != null)
            {
                if (EditorWindow.HasOpenInstances<MaterialGeneratorWindow>())
                {
                    if (MaterialGeneratorWindow.window == null)
                    {
                        //Do this instead of EditorWindow.GetWindow so we find the window regardless of docked state
                        MaterialGeneratorWindow.window = Resources.FindObjectsOfTypeAll<MaterialGeneratorWindow>()[0];
                    }

                    //Clear any existing buffers before they are lost
                    MaterialGeneratorWindow.window.InitializeTool(SketchRendererManager.ResourceAsset,
                        SketchRendererManager.ManagerSettings);
                }
            }

            if (hasDelayedMaterialWindowCall)
            {
                EditorApplication.delayCall -= ValidateMaterialWindow;
                hasDelayedMaterialWindowCall = false;
            }
        }
        
        #endregion
    }
}