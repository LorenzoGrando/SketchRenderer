using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    [VolumeComponentMenu(SketchRendererData.PackageInspectorVolumePath + "UVs")]
    public class RenderUVsVolumeComponent : VolumeComponent, ISketchVolumeComponent
    {
        public ClampedIntParameter RotationStep = new ClampedIntParameter(0, 0, 3);
        public MinIntParameter Scale = new MinIntParameter(1, 1);
        
        public void CopyFromContext(SketchRendererContext context)
        {
            RotationStep.value = context.UVSFeatureData.SkyboxRotationStep;
            Scale.value = context.UVSFeatureData.SkyboxScale;
        }
    }
}