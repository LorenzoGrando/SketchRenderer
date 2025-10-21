using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;

namespace SketchRenderer.Runtime.Rendering.Volume
{
    public interface ISketchVolumeComponent
    {
        public void CopyFromContext(SketchRendererContext context);
    }
}