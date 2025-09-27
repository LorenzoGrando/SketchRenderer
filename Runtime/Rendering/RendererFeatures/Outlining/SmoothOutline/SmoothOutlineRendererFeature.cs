using System;
using SketchRenderer.Runtime.Data;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    public class SmoothOutlineRendererFeature : ScriptableRendererFeature, ISketchRendererFeature
    {
        [HideInInspector]
        public EdgeDetectionPassData EdgeDetectionPassData = new EdgeDetectionPassData();
        private EdgeDetectionPassData CurrentEdgeDetectionPassData { get { return EdgeDetectionPassData.GetPassDataByVolume(); } }
        
        [HideInInspector]
        public ThicknessDilationPassData ThicknessPassData = new ThicknessDilationPassData();
        private ThicknessDilationPassData CurrentThicknessPassData { get { return ThicknessPassData.GetPassDataByVolume(); } }
        
        [HideInInspector]
        public AccentedOutlinePassData AccentedOutlinePassData = new AccentedOutlinePassData();
        public AccentedOutlinePassData CurrentAccentOutlinePassData { get { return AccentedOutlinePassData.GetPassDataByVolume(); } }
        
        [SerializeField] [HideInInspector]
        private Shader depthNormalsEdgeDetectionShader;
        [SerializeField] [HideInInspector]
        private Shader colorEdgeDetectionShader;
        [SerializeField] [HideInInspector]
        private Shader edgeDetectionCompositorShader;

        [SerializeField] [HideInInspector]
        private Shader thicknessDilationDetectionShader;
        [SerializeField] [HideInInspector]
        private Shader accentedOutlinesShader;
        
        private Material edgeDetectionMaterial;
        private Material secondaryEdgeDetectionMaterial;
        private Material edgeCompositorMaterial;
        private Material thicknessDilationMaterial;
        private Material accentedOutlinesMaterial;
        
        private EdgeDetectionRenderPass edgeDetectionPass;
        private EdgeDetectionRenderPass secondaryEdgeDetectionPass;
        private EdgeCompositorRenderPass edgeCompositorPass;
        private ThicknessDilationRenderPass thicknessDilationPass;
        private AccentedOutlineRenderPass accentedOutlinePass;
        
        public override void Create()
        {
            //Material accumulation still requires relevant direction data, so ensure this is the case
            if(EdgeDetectionPassData != null)
                EdgeDetectionPassData.OutputType = EdgeDetectionGlobalData.EdgeDetectionOutputType.OUTPUT_DIRECTION_DATA_VECTOR;

            if (CurrentEdgeDetectionPassData.Source != EdgeDetectionGlobalData.EdgeDetectionSource.ALL)
            {
                edgeDetectionMaterial = CreateEdgeDetectionMaterial(CurrentEdgeDetectionPassData.Source);
                edgeDetectionPass = CreateEdgeDetectionPass(CurrentEdgeDetectionPassData.Source);
                ReleaseSecondaryEdgeDetectionComponents();
            }
            else
            {
                edgeDetectionMaterial = CreateEdgeDetectionMaterial(EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH_NORMALS);
                edgeDetectionPass = CreateEdgeDetectionPass(EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH_NORMALS);
                secondaryEdgeDetectionMaterial = CreateEdgeDetectionMaterial(EdgeDetectionGlobalData.EdgeDetectionSource.COLOR);
                secondaryEdgeDetectionPass = CreateEdgeDetectionPass(EdgeDetectionGlobalData.EdgeDetectionSource.COLOR);
            }
            
            if(edgeDetectionCompositorShader != null)
                edgeCompositorMaterial = new Material(edgeDetectionCompositorShader);
            edgeCompositorPass = new EdgeCompositorRenderPass();

            if(thicknessDilationDetectionShader != null)
                thicknessDilationMaterial = new Material(thicknessDilationDetectionShader);
            thicknessDilationPass = new ThicknessDilationRenderPass();
            
            if(accentedOutlinesShader != null) 
                accentedOutlinesMaterial = new Material(accentedOutlinesShader);
            accentedOutlinePass = new AccentedOutlineRenderPass();
        }

        public void OnValidate()
        {
            if (AccentedOutlinePassData.UseAccentedOutlines)
                AccentedOutlinePassData.ForceRebake = true;
        }
        
        public void ConfigureByContext(SketchRendererContext context, SketchResourceAsset resources)
        {
            if (context.UseSmoothOutlineFeature)
            {
                EdgeDetectionPassData.CopyFrom(context.EdgeDetectionFeatureData);
                EdgeDetectionPassData.OutputType = EdgeDetectionGlobalData.EdgeDetectionOutputType.OUTPUT_DIRECTION_DATA_VECTOR;
                AccentedOutlinePassData.CopyFrom(context.AccentedOutlineFeatureData);
                ThicknessPassData.CopyFrom(context.ThicknessDilationFeatureData);
                depthNormalsEdgeDetectionShader = resources.Shaders.DepthNormalsEdgeDetection;
                colorEdgeDetectionShader = resources.Shaders.ColorEdgeDetection;
                edgeDetectionCompositorShader = resources.Shaders.EdgeCompositor;
                accentedOutlinesShader = resources.Shaders.AccentedOutline;
                thicknessDilationDetectionShader = resources.Shaders.ThicknessDilation;
                Create();
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.SceneView && !SketchGlobalFrameData.AllowSceneRendering)
                return;
            
            if(!renderingData.postProcessingEnabled)
                return;
            
            if(!renderingData.cameraData.postProcessEnabled)
                return;
            
            if(!AreCurrentDynamicsValid())
                Create();
            
            if(!AreAllMaterialsValid())
                return;

            if (CurrentEdgeDetectionPassData.IsAllPassDataValid())
            {
                if (CurrentEdgeDetectionPassData.Source != EdgeDetectionGlobalData.EdgeDetectionSource.ALL)
                {
                    edgeDetectionPass.Setup(CurrentEdgeDetectionPassData, edgeDetectionMaterial);
                    edgeDetectionPass.SetSecondary(false);
                    renderer.EnqueuePass(edgeDetectionPass);
                }
                else
                {
                    EdgeDetectionPassData primaryData = new EdgeDetectionPassData();
                    primaryData.CopyFrom(CurrentEdgeDetectionPassData);
                    primaryData.Source = EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH_NORMALS;
                    edgeDetectionPass.Setup(primaryData, edgeDetectionMaterial);
                    edgeDetectionPass.SetSecondary(false);
                    
                    EdgeDetectionPassData secondaryData = new EdgeDetectionPassData();
                    secondaryData.CopyFrom(CurrentEdgeDetectionPassData);
                    secondaryData.Source = EdgeDetectionGlobalData.EdgeDetectionSource.COLOR;
                    renderer.EnqueuePass(edgeDetectionPass);
                    secondaryEdgeDetectionPass.Setup(secondaryData, secondaryEdgeDetectionMaterial);
                    secondaryEdgeDetectionPass.SetSecondary(true);
                    renderer.EnqueuePass(secondaryEdgeDetectionPass);
                    
                    edgeCompositorPass.Setup(edgeCompositorMaterial);
                    renderer.EnqueuePass(edgeCompositorPass);
                }
            }

            if (CurrentThicknessPassData.IsAllPassDataValid())
            {
                thicknessDilationPass.Setup(CurrentThicknessPassData, thicknessDilationMaterial);
                renderer.EnqueuePass(thicknessDilationPass);
            }

            if (CurrentAccentOutlinePassData.IsAllPassDataValid())
            {
                accentedOutlinePass.Setup(CurrentAccentOutlinePassData, accentedOutlinesMaterial);
                renderer.EnqueuePass(accentedOutlinePass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            edgeDetectionPass?.Dispose();
            edgeDetectionPass = null;
            
            secondaryEdgeDetectionPass?.Dispose();
            secondaryEdgeDetectionPass = null;

            edgeCompositorPass = null;
            
            thicknessDilationPass?.Dispose();
            thicknessDilationPass = null;
            
            accentedOutlinePass?.Dispose();
            accentedOutlinePass = null;

            if (Application.isPlaying)
            {
                if (edgeDetectionMaterial)
                    Destroy(edgeDetectionMaterial);
                if(secondaryEdgeDetectionMaterial)
                    Destroy(secondaryEdgeDetectionMaterial);
                if(edgeCompositorMaterial)
                    Destroy(edgeCompositorMaterial);
                if(thicknessDilationMaterial)
                    Destroy(thicknessDilationMaterial);
                if(accentedOutlinesMaterial)
                    Destroy(accentedOutlinesMaterial);
            }
        }

        private void ReleaseSecondaryEdgeDetectionComponents()
        {
            secondaryEdgeDetectionPass?.Dispose();
            secondaryEdgeDetectionPass = null;
            
            if (Application.isPlaying)
            {
                if (secondaryEdgeDetectionMaterial)
                    Destroy(edgeDetectionMaterial);
                secondaryEdgeDetectionMaterial = null;
            }
        }

        private bool AreAllMaterialsValid()
        {
            return edgeDetectionMaterial != null && edgeCompositorMaterial != null && thicknessDilationMaterial != null && accentedOutlinesMaterial != null && (CurrentEdgeDetectionPassData.Source != EdgeDetectionGlobalData.EdgeDetectionSource.ALL || secondaryEdgeDetectionMaterial != null);
        }

        private bool AreCurrentDynamicsValid()
        {
            switch (CurrentEdgeDetectionPassData.Source)
            {
                case EdgeDetectionGlobalData.EdgeDetectionSource.COLOR:
                    return (edgeDetectionMaterial != null && edgeDetectionMaterial.shader == colorEdgeDetectionShader);
                case EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH:
                case EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH_NORMALS:
                    return (edgeDetectionMaterial != null && edgeDetectionMaterial.shader == depthNormalsEdgeDetectionShader);
                case EdgeDetectionGlobalData.EdgeDetectionSource.ALL:
                    return (edgeDetectionMaterial != null && edgeDetectionMaterial.shader == depthNormalsEdgeDetectionShader) && 
                           (secondaryEdgeDetectionMaterial != null && secondaryEdgeDetectionMaterial.shader == colorEdgeDetectionShader);
            }
            
            return false;
        }

        private Material CreateEdgeDetectionMaterial(EdgeDetectionGlobalData.EdgeDetectionSource edgeDetectionMethod)
        {
            Material mat = null;
            switch (edgeDetectionMethod)
            {
                case EdgeDetectionGlobalData.EdgeDetectionSource.COLOR:
                    if(colorEdgeDetectionShader != null)
                        mat = new Material(colorEdgeDetectionShader);
                    break;
                case EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH:
                case EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH_NORMALS:
                    if(depthNormalsEdgeDetectionShader != null)
                        mat =  new Material(depthNormalsEdgeDetectionShader);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edgeDetectionMethod), edgeDetectionMethod, null);
            }
            return mat;
        }

        private EdgeDetectionRenderPass CreateEdgeDetectionPass(EdgeDetectionGlobalData.EdgeDetectionSource source)
        {
            switch (source)
            {
                case EdgeDetectionGlobalData.EdgeDetectionSource.COLOR:
                    return new ColorSilhouetteRenderPass();
                case EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH:
                case EdgeDetectionGlobalData.EdgeDetectionSource.DEPTH_NORMALS:
                    return new DepthNormalsSilhouetteRenderPass();
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }
    }
}