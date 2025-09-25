using SketchRenderer.Runtime.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class MaterialSurfaceRendererFeature : ScriptableRendererFeature, ISketchRendererFeature
    {
        [SerializeField] [HideInInspector]
        public MaterialSurfacePassData MaterialData = new MaterialSurfacePassData();
        
        private Material materialMat;
        [SerializeField] [HideInInspector]
        private Shader materialSurfaceShader;
        
        private MaterialSurfaceRenderPass materialRenderPass;

        public override void Create()
        {
            if(materialSurfaceShader == null)
                return;
            
            materialMat = CreateMaterial();
            materialRenderPass = new MaterialSurfaceRenderPass();
        }
        
        public void ConfigureByContext(SketchRendererContext context, SketchResourceAsset resources)
        {
            if (context.UseMaterialFeature)
            {
                MaterialData.CopyFrom(context.MaterialFeatureData);
                materialSurfaceShader = resources.Shaders.MaterialSurface;
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
            
            if(!MaterialData.IsAllPassDataValid())
                return;
            

            materialRenderPass.Setup(MaterialData.GetPassDataByVolume(), materialMat);
            renderer.EnqueuePass(materialRenderPass);
        }

        protected override void Dispose(bool disposing)
        {
            materialRenderPass?.Dispose();
            materialRenderPass = null;

            if (Application.isPlaying)
            {
                if (materialMat)
                    Destroy(materialMat);
            }
        }

        private bool AreAllMaterialsValid()
        {
            return materialMat != null;
        }

        private Material CreateMaterial()
        {
            Material mat = new Material(materialSurfaceShader);

            return mat;
        }
    }
}