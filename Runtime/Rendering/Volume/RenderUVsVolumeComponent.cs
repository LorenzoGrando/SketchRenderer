using SketchRenderer.Runtime.Data;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "UVs")]
    public class RenderUVsVolumeComponent : VolumeComponent
    {
        public ClampedIntParameter RotationStep = new ClampedIntParameter(0, 0, 3);
        public MinIntParameter Scale = new MinIntParameter(1, 1);
    }
}