using System;
using SketchRenderer.Runtime.Data;
using UnityEngine;

namespace TextureTools.Material
{
    [CreateAssetMenu(fileName = "MaterialDataAsset", menuName = SketchRendererData.PackageAssetItemPath  + "MaterialDataAsset")]
    public class MaterialDataAsset : ScriptableObject
    {
        public bool UseGranularity;
        public GranularityData Granularity;

        [Space(10)] 
        public bool UseLaidLines;
        public LaidLineData LaidLines;
    
        [Space(10)]
        public bool UseCrumples;
        public CrumpleData Crumples;
    
        [Space(10)]
        public bool UseNotebookLines;
        public NotebookLineData NotebookLines;

        public void CopyFrom(MaterialDataAsset materialDataAsset)
        {
            if(materialDataAsset == null)
                return;
            
            UseGranularity = materialDataAsset.UseGranularity;
            Granularity = materialDataAsset.Granularity;
            
            UseLaidLines = materialDataAsset.UseLaidLines;
            LaidLines = materialDataAsset.LaidLines;
            
            UseCrumples = materialDataAsset.UseCrumples;
            Crumples = materialDataAsset.Crumples;
            
            UseNotebookLines = materialDataAsset.UseNotebookLines;
            NotebookLines = materialDataAsset.NotebookLines;
        }
    }
}