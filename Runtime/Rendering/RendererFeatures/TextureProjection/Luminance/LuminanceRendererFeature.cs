using SketchRenderer.Runtime.Data;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class LuminanceRendererFeature : ScriptableRendererFeature, ISketchRendererFeature
    {
        [SerializeField] [HideInInspector]
        public LuminancePassData LuminanceData = new LuminancePassData();
        
        private Material luminanceMaterial;
        [SerializeField] [HideInInspector]
        private Shader luminanceShader;
        
        private LuminanceRenderPass luminanceRenderPass;

        public override void Create()
        {
            if (luminanceShader == null)
                return;
            
            luminanceMaterial = CreateLuminanceMaterial();
            luminanceRenderPass = new LuminanceRenderPass();
        }
        
        public void ConfigureByContext(SketchRendererContext context, SketchResourceAsset resources)
        {
            if (context.UseLuminanceFeature)
            {
                LuminanceData.CopyFrom(context.LuminanceFeatureData);
                luminanceShader = resources.Shaders.Luminance;
                Create();
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.SceneView && !SketchGlobalFrameData.AllowSceneRendering)
                return;

            if (!renderingData.postProcessingEnabled)
                return;
            
            if(!renderingData.cameraData.postProcessEnabled)
                return;

            if (!AreAllMaterialsValid())
                return;
            
            if(!LuminanceData.IsAllPassDataValid())
                return;
            
            if(!LuminanceData.ActiveInVolume())
                return;

            luminanceRenderPass.Setup(LuminanceData.GetPassDataByVolume(), luminanceMaterial);
            renderer.EnqueuePass(luminanceRenderPass);
        }

        protected override void Dispose(bool disposing)
        {
            luminanceRenderPass?.Dispose();
            luminanceRenderPass = null;

            if (Application.isPlaying)
            {
                if (luminanceMaterial)
                    Destroy(luminanceMaterial);
            }
        }

        private bool AreAllMaterialsValid()
        {
            return luminanceMaterial != null;
        }

        private Material CreateLuminanceMaterial()
        {
            Material mat = new Material(luminanceShader);

            return mat;
        }
    }
}