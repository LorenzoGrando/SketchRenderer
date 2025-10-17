using System;
using SketchRenderer.Runtime.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class SketchCompositionRendererFeature : ScriptableRendererFeature, ISketchRendererFeature
    {
        [SerializeField] [HideInInspector]
        public SketchCompositionPassData CompositionPassData = new SketchCompositionPassData();
        
        [SerializeField] [HideInInspector]
        private Shader sketchCompositionShader;
        
        private Material sketchMaterial;
        private SketchCompositionRenderPass sketchRenderPass;

        public override void Create()
        {
            sketchMaterial = CreateSketchMaterial();
            sketchRenderPass = new SketchCompositionRenderPass();
        }
        
        public void ConfigureByContext(SketchRendererContext context, SketchResourceAsset resources)
        {
            CompositionPassData.CopyFrom(context.CompositionFeatureData);
            sketchCompositionShader = resources.Shaders.SketchComposition;
            Create();
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
            
            if(!CompositionPassData.IsAllPassDataValid())
                return;

            sketchRenderPass.Setup(CompositionPassData.GetPassDataByVolume(), sketchMaterial);
            renderer.EnqueuePass(sketchRenderPass);
        }

        protected override void Dispose(bool disposing)
        {
            sketchRenderPass?.Dispose();
            sketchRenderPass = null;

            if (Application.isPlaying)
            {
                if (sketchMaterial)
                    Destroy(sketchMaterial);
            }
        }

        private Material CreateSketchMaterial()
        {
            if(sketchCompositionShader == null)
                return null;
            
            Material material = new Material(sketchCompositionShader);
            return material;
        }

        private bool AreAllMaterialsValid()
        {
            return sketchMaterial != null;
        }
    }
}