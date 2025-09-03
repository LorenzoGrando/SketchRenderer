using System;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.TextureTools.Strokes;
using SketchRenderer.Runtime.TextureTools.TonalArtMap;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Data
{
    [CreateAssetMenu(fileName = "SketchResourceAsset", menuName = SketchRendererData.PackageAssetItemPath + "SketchResourceAsset")]
    public class SketchResourceAsset : ScriptableObject
    {
        /// <summary>
        /// Class holding references to all shader files used in the package.
        /// </summary>
        [Serializable, ReloadGroup]
        public class ShaderData
        {
            [SerializeField] [HideInInspector] [Reload("Shader/Luminance/Luminance.shader")]
            public Shader Luminance;
            
            [SerializeField] [HideInInspector] [Reload("Shader/MaterialSurface.shader")]
            public Shader MaterialSurface;
            
            [SerializeField] [HideInInspector] [Reload("Shader/Outlining/EdgeDetection/ColorSilhouette.shader")]
            public Shader ColorEdgeDetection;
            
            [SerializeField] [HideInInspector] [Reload("Shader/Outlining/EdgeDetection/DepthNormalsSilhouette.shader")]
            public Shader DepthNormalsEdgeDetection;
            
            [SerializeField] [HideInInspector] [Reload("Shader/Outlining/AccentedOutline.shader")]
            public Shader AccentedOutline;
            
            [SerializeField] [HideInInspector] [Reload("Shader/Outlining/EdgeCompositor.shader")]
            public Shader EdgeCompositor;
            
            [SerializeField] [HideInInspector] [Reload("Shader/Outlining/ThicknessDilation.shader")]
            public Shader ThicknessDilation;
            
            [SerializeField] [HideInInspector] [Reload("Shader/RenderUVs.shader")]
            public Shader RenderUVs;
            
            [SerializeField] [HideInInspector] [Reload("Shader/SketchComposition.shader")]
            public Shader SketchComposition;
        }

        [SerializeField] [HideInInspector]
        public ShaderData Shaders = new ShaderData();

        /// <summary>
        /// Class holding references to all compute shader files used in the package.
        /// </summary>
        [Serializable, ReloadGroup]
        public class ComputeShaderData
        {
            [SerializeField] [HideInInspector] [Reload("Shader/TonalArtMapGeneratorCompute.compute")]
            public ComputeShader TonalArtMapGenerator;
            
            [SerializeField] [HideInInspector] [Reload("Shader/Outlining/SketchStrokesCompute.compute")]
            public ComputeShader SketchStrokes;
        }
        
        [SerializeField] [HideInInspector]
        public ComputeShaderData ComputeShaders = new ComputeShaderData();

        /// <summary>
        /// Class grouping groups with default asset ScriptableObject instances of package data.
        /// </summary>
        [Serializable, ReloadGroup]
        public class DefaultScriptables
        {
            /// <summary>
            /// Class grouping groups with default asset ScriptableObject instances of package data.
            /// </summary>
            [Serializable, ReloadGroup]
            public class StrokeAssets
            {
                [SerializeField] [HideInInspector] [Reload("Runtime/Data/DefaultScriptables/DefaultSimpleStroke.asset")]
                public StrokeAsset DefaultSimpleStroke;
                [SerializeField] [HideInInspector] [Reload("Runtime/Data/DefaultScriptables/DefaultHatchingStroke.asset")]
                public HatchingStrokeAsset DefaultHatchingStroke;
                [SerializeField] [HideInInspector] [Reload("Runtime/Data/DefaultScriptables/DefaultZigzagStroke.asset")]
                public ZigzagStrokeAsset DefaultZigzagStroke;
                [SerializeField] [HideInInspector] [Reload("Runtime/Data/DefaultScriptables/DefaultFeatheringStroke.asset")]
                public FeatheringStrokeAsset DefaultFeatheringStroke;
            }

            [SerializeField] [HideInInspector]
            public StrokeAssets Strokes;
            
            [SerializeField] [HideInInspector] [Reload("Runtime/Data/DefaultScriptables/DefaultTonalArtMap.asset")]
            public TonalArtMapAsset TonalArtMap;
        }
        
        [SerializeField] [HideInInspector]
        public DefaultScriptables Scriptables = new DefaultScriptables();
    }
}