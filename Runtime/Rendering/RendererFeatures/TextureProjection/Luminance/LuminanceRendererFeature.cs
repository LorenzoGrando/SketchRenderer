using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class LuminanceRendererFeature : ScriptableRendererFeature, ISketchRendererFeature
    {
        [Header("Parameters")] [Space(5)] [SerializeField]
        public LuminancePassData LuminanceData = new LuminancePassData();
        
        [HideInInspector]
        private Material luminanceMaterial;
        [SerializeField]
        private Shader luminanceShader;
        
        private LuminanceRenderPass luminanceRenderPass;

        public override void Create()
        {
            if(luminanceShader == null)
                return;
            
            luminanceMaterial = CreateLuminanceMaterial();
            luminanceRenderPass = new LuminanceRenderPass();
        }
        
        public void ConfigureByContext(SketchRendererContext context)
        {
            if (context.UseLuminanceFeature)
            {
                LuminanceData.CopyFrom(context.LuminanceFeatureData);
                Create();
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.SceneView)
                return;

            if (!renderingData.postProcessingEnabled)
                return;
            
            if(!renderingData.cameraData.postProcessEnabled)
                return;

            if (!AreAllMaterialsValid())
                return;
            
            if(!LuminanceData.IsAllPassDataValid())
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