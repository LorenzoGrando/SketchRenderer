using UnityEngine;

namespace SketchRenderer.Runtime.Rendering
{
    public static class ComputeData
    {
        public enum KernelSize2D
        {
            SIZE_4X4,
            SIZE_8X8,
            SIZE_16X16,
            SIZE_32X32
        }

        public static Vector2Int GetKernelSizeFromEnum(KernelSize2D size)
        {
            switch (size)
            {
                case KernelSize2D.SIZE_4X4:
                    return new Vector2Int(4, 4);
                case KernelSize2D.SIZE_8X8:
                    return new Vector2Int(8, 8);
                case KernelSize2D.SIZE_16X16:
                    return new Vector2Int(16, 16);
                case KernelSize2D.SIZE_32X32:
                    return new Vector2Int(32, 32);
            }

            return Vector2Int.zero;
        }
    }
}