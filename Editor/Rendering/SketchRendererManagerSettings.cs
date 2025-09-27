using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.TextureTools.Strokes;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;
using TextureTools.Material;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Editor.Rendering
{
    internal class SketchRendererManagerSettings : ScriptableObject
    {
        [SerializeField] [HideInInspector]
        private SketchRendererContext currentRendererContext;
        internal SketchRendererContext CurrentRendererContext
        {
            get => currentRendererContext;
            set => currentRendererContext = value;
        }
        
        [SerializeField] [HideInInspector]
        internal bool AlwaysUpdateRendererData = false;

        internal readonly int delayedValidateEditorFrameCount = 500;
        
        #region Texture Tool Persistent Asset References

        private void OnPersistentAssetChanged()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
        
        [SerializeField] [HideInInspector] 
        private StrokeAsset cachedStrokeAsset;
        internal StrokeAsset PersistentStrokeAsset
        {
            get => cachedStrokeAsset;
            set
            {
                cachedStrokeAsset = value;
                OnPersistentAssetChanged();
            }
        }
        
        [SerializeField] [HideInInspector] 
        private TonalArtMapAsset cachedTonalArtMapAsset;
        internal TonalArtMapAsset PersistentTonalArtMapAsset
        {
            get => cachedTonalArtMapAsset;
            set
            {
                cachedTonalArtMapAsset = value;
                OnPersistentAssetChanged();
            }
        }

        [SerializeField] [HideInInspector]
        private MaterialDataAsset cachedMaterialDataAsset;
        internal MaterialDataAsset PersistentMaterialDataAsset
        {
            get => cachedMaterialDataAsset;
            set
            {
                cachedMaterialDataAsset = value; 
                OnPersistentAssetChanged();
            }
        }
        #endregion
    }
}