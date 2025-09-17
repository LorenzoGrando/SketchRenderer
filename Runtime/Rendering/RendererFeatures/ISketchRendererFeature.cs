using SketchRenderer.Runtime.Data;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public interface ISketchRendererFeature
    {
        public void ConfigureByContext(SketchRendererContext context, SketchResourceAsset resources);
    }
}