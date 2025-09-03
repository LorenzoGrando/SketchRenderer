using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace SketchRenderer.Runtime.Rendering
{
    public class SketchFrameData : ContextItem
    {
        public bool PrebakedDistortedUVs;
        public bool PrebakedDistortedMultipleUVs;
        public Vector4 LuminanceBasisDirection;
        public TextureHandle MaterialTexture;
        public TextureHandle DirectionalTexture;
        public TextureHandle OutlinesTexture;
        public TextureHandle OutlinesSecondaryTexture;
        public TextureHandle LuminanceTexture;

        public override void Reset()
        {
            LuminanceBasisDirection = Vector4.zero;
            MaterialTexture = TextureHandle.nullHandle;
            DirectionalTexture = TextureHandle.nullHandle;
            OutlinesTexture = TextureHandle.nullHandle;
            OutlinesSecondaryTexture = TextureHandle.nullHandle;
            LuminanceTexture = TextureHandle.nullHandle;
        }
    }
}