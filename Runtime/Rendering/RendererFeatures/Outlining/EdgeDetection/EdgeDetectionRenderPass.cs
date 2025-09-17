using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public abstract class EdgeDetectionRenderPass : ScriptableRenderPass, ISketchRenderPass<EdgeDetectionPassData>
    {
        public abstract string PassName { get; }
        
        protected Material edgeDetectionMaterial;
        protected EdgeDetectionPassData passData;
        public bool IsSecondary { get; protected set; }
        
        protected static readonly int outlineOffsetShaderID = Shader.PropertyToID("_OutlineOffset");
        protected static readonly int outlineThresholdShaderID = Shader.PropertyToID("_OutlineThreshold");
        protected static readonly int outlineDistanceFalloffShaderID = Shader.PropertyToID("_OutlineDistanceFalloff");
        protected static readonly int outlineAngleSensitivityShaderID = Shader.PropertyToID("_OutlineShallowThresholdSensitivity");
        protected static readonly int outlineAngleConstraintShaderID = Shader.PropertyToID("_OutlineShallowThresholdStrength");
        protected static readonly int outlineNormalSensitivityShaderID = Shader.PropertyToID("_OutlineNormalDistanceSensitivity");
        
        protected const string OUTLINE_TEXTURE_NAME = "OutlineTexture";
        protected const string OUTLINE_SECONDARY_TEXTURE_NAME = "OutlineSecondaryTexture";
        
        protected LocalKeyword outputGreyscaleKeyword;
        protected LocalKeyword outputDirectionAngleDataKeyword;
        protected LocalKeyword outputDirectionVectorDataKeyword;
        
        // Scale bias is used to control how the blit operation is done. The x and y parameter controls the scale
        // and z and w controls the offset.
        static protected Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

        public virtual void Setup(EdgeDetectionPassData passData, Material mat)
        {
            this.passData = passData;
            edgeDetectionMaterial = mat;
            
            //inherited
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            
            ConfigureMaterial();
        }
        
        public void SetSecondary(bool isSecondary) => this.IsSecondary = isSecondary;

        public virtual void ConfigureMaterial()
        {
            outputGreyscaleKeyword = new LocalKeyword(edgeDetectionMaterial.shader, EdgeDetectionGlobalData.OUTPUT_GREYSCALE_KEYWORD);
            outputDirectionAngleDataKeyword = new LocalKeyword(edgeDetectionMaterial.shader, EdgeDetectionGlobalData.OUPUT_DIRECTION_ANGLE_KEYWORD);
            outputDirectionVectorDataKeyword = new LocalKeyword(edgeDetectionMaterial.shader, EdgeDetectionGlobalData.OUPUT_DIRECTION_VECTOR_KEYWORD);
            
            edgeDetectionMaterial.SetInteger(outlineOffsetShaderID, passData.OutlineOffset);
            edgeDetectionMaterial.SetFloat(outlineThresholdShaderID, passData.OutlineThreshold);
            edgeDetectionMaterial.SetFloat(outlineDistanceFalloffShaderID, passData.OutlineDistanceFalloff);
            edgeDetectionMaterial.SetFloat(outlineAngleSensitivityShaderID, passData.OutlineAngleSensitivity);
            edgeDetectionMaterial.SetFloat(outlineAngleConstraintShaderID, passData.OutlineAngleConstraint);
            edgeDetectionMaterial.SetFloat(outlineNormalSensitivityShaderID, passData.OutlineNormalSensitivity);

            switch (passData.OutputType)
            {
                case EdgeDetectionGlobalData.EdgeDetectionOutputType.OUTPUT_GREYSCALE:
                    edgeDetectionMaterial.SetKeyword(outputGreyscaleKeyword, true);
                    edgeDetectionMaterial.SetKeyword(outputDirectionAngleDataKeyword, false);
                    edgeDetectionMaterial.SetKeyword(outputDirectionVectorDataKeyword, false);
                    break;
                case EdgeDetectionGlobalData.EdgeDetectionOutputType.OUTPUT_DIRECTION_DATA_ANGLE:
                    edgeDetectionMaterial.SetKeyword(outputGreyscaleKeyword, false);
                    edgeDetectionMaterial.SetKeyword(outputDirectionAngleDataKeyword, true);
                    edgeDetectionMaterial.SetKeyword(outputDirectionVectorDataKeyword, false);
                    break;
                case EdgeDetectionGlobalData.EdgeDetectionOutputType.OUTPUT_DIRECTION_DATA_VECTOR:
                    edgeDetectionMaterial.SetKeyword(outputGreyscaleKeyword, false);
                    edgeDetectionMaterial.SetKeyword(outputDirectionAngleDataKeyword, false);
                    edgeDetectionMaterial.SetKeyword(outputDirectionVectorDataKeyword, true);
                    break;
            }
        }

        public virtual void Dispose() {}
        
        protected class BlitPassData
        {
            public TextureHandle src;
            public int passID;
            public Material mat;
        }
        
        protected static void ExecuteSobelEdgePass(BlitPassData data, RasterGraphContext context)
        {
            Blitter.BlitTexture(context.cmd, data.src, scaleBias, data.mat, data.passID);
        }
        
        public abstract override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData);
    }
}