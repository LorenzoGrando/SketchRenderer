using System;
using SketchRenderer.Runtime.Data;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor
{
    internal abstract class SketchEditorToolWindow<T> : EditorWindow where T : EditorWindow
    {
        internal event Action OnWindowClosed;
        internal static T window;

        internal virtual Vector2 ExpectedMinWindowSize
        {
            get
            {
                return new Vector2(250, 500);
            }
        }
        
        internal virtual Vector2 ExpectedMaxWindowSize
        {
            get
            {
                return new Vector2(350, 900);
            }
        }

        internal abstract void InitializeTool(SketchResourceAsset resources);
        
        internal abstract void FinalizeTool();

        internal virtual void OnDestroy()
        {
            OnWindowClosed?.Invoke();
        }
    }
}