using System;
using SketchRenderer.Runtime.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class RenderUVsRendererFeature : ScriptableRendererFeature, ISketchRendererFeature
    {
        [Header("Base Parameters")] 
        [Space(5)] 
        [SerializeField] public RenderUVsPassData UvsPassData = new RenderUVsPassData();
        private RenderUVsPassData CurrentUVsPassData { get { return UvsPassData.GetPassDataByVolume(); } }

        [SerializeField] [HideInInspector]
        private Shader renderUVsShader;
        
        private Material renderUVsMaterial;
        
        private RenderUVsRenderPass renderUVsRenderPass;
        
        public override void Create()
        {
            if (renderUVsShader != null)
            {
                renderUVsMaterial = new Material(renderUVsShader);
            }

            renderUVsRenderPass = new RenderUVsRenderPass();
        }
        
        public void ConfigureByContext(SketchRendererContext context, SketchResourceAsset resources)
        {
            if (context.UseUVsFeature)
            {
                UvsPassData.CopyFrom(context.UVSFeatureData);
                renderUVsShader = resources.Shaders.RenderUVs;
                Create();
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(renderingData.cameraData.cameraType == CameraType.SceneView)
                return;
            
            if(!renderingData.postProcessingEnabled)
                return;
            
            if(!renderingData.cameraData.postProcessEnabled)
                return;
            
            if(!AreAllMaterialsValid())
                return;
            
            if(!UvsPassData.IsAllPassDataValid())
                return;
            
            renderUVsRenderPass.Setup(CurrentUVsPassData, renderUVsMaterial);
            renderer.EnqueuePass(renderUVsRenderPass);
        }

        protected override void Dispose(bool disposing)
        {
            renderUVsRenderPass?.Dispose();
            renderUVsRenderPass = null;
            
            if (Application.isPlaying)
            {
                if (renderUVsMaterial)
                    Destroy(renderUVsMaterial);
            }
        }

        private bool AreAllMaterialsValid()
        {
            return renderUVsMaterial != null;
        }
    }
}