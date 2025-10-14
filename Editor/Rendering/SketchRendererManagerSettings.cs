using System;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering;
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
        internal event Action OnContextSettingsChanged;
        
        [SerializeField] [HideInInspector]
        private SketchRendererContext currentRendererContext;
        internal SketchRendererContext CurrentRendererContext
        {
            get => currentRendererContext;
            set
            {
                currentRendererContext = value; 
                TrackCurrentContext();
            }
        }
        internal SketchRendererContext listenerContext;
        
        [SerializeField] [HideInInspector]
        internal bool AlwaysUpdateRendererData = false;
        
        [SerializeField] [HideInInspector]
        internal bool DisplayInSceneView = false;

        internal readonly int delayedValidateEditorFrameCount = 500;

        internal void ValidateGlobalSettings()
        {
            TrackCurrentContext();
            SketchGlobalFrameData.AllowSceneRendering = DisplayInSceneView;
        }

        internal void ForceDirtySettings()
        {
            if (CurrentRendererContext != null)
            {
                bool previous = AlwaysUpdateRendererData;
                AlwaysUpdateRendererData = true;
                OnContextSettingsChanged?.Invoke();
                AlwaysUpdateRendererData = previous;
            }
        }

        private void TrackCurrentContext()
        {
            if (listenerContext != null)
                listenerContext.OnValidated -= RendererContext_OnValidate;
                
            listenerContext = CurrentRendererContext;
            listenerContext.OnValidated += RendererContext_OnValidate;
        }

        private void RendererContext_OnValidate()
        {
            if(listenerContext.IsDirty)
                OnContextSettingsChanged?.Invoke();
        }

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