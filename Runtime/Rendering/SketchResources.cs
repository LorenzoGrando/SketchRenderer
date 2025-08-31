using UnityEngine;

namespace SketchRenderer.Runtime.Rendering
{
    public static class SketchResources
    {
        public static class ScreenUV
        {
            public const string TextureName = "_CameraUVsTexture";
            private static readonly int textureShaderID = Shader.PropertyToID(TextureName);

            public static int GetUVTextureID => textureShaderID;
        }
    }
}