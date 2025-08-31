namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    using UnityEngine;

    public interface ISketchRenderPass<T> where T : ISketchRenderPassData<T>
    {
        public string PassName { get; }
    
        public void Setup(T passData, Material mat);
    
        public void ConfigureMaterial();

        public void Dispose();
    }
}