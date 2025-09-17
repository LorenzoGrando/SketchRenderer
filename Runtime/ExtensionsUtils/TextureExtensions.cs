using UnityEngine;

namespace SketchRenderer.Runtime.Extensions
{
    public static class TextureExtensions
    {
        public static Vector4 GetTexelSize(this Texture texture)
        {
            return new Vector4(1f/(float)texture.width, 1f/(float)texture.height, texture.width, texture.height);
        }
    }
}