using SketchRenderer.Runtime.Data;
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
    }
}