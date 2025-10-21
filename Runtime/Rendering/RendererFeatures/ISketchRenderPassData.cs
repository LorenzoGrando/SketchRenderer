namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public interface ISketchRenderPassData<T>
    {
        public bool IsAllPassDataValid();

        public T GetPassDataByVolume();

        public bool ActiveInVolume();
    }
}