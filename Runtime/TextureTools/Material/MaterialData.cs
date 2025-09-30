using UnityEngine;

namespace TextureTools.Material
{
    [System.Serializable]
    public struct GranularityData
    {
        [Range(2, 20)]
        public Vector2Int Scale;
        [Range(1, 10)]
        public int DetailLevel;
        [Range(0, 50)]
        public int DetailFrequency;
        [Range(0, 1)]
        public float DetailPersistence;
        [Range(0, 1)] 
        public float MinimumGranularity;
        [Range(0, 1)]
        public float MaximumGranularity;
        public Color GranularityTint;
    }

    [System.Serializable]
    public struct LaidLineData
    {
        public int LineFrequency;
        [Range(0, 1)]
        public float LineThickness;
        [Range(0, 1)]
        public float LineStrength;
        [Range(0, 1)]
        public float LineGranularityDisplacement;
        [Range(0, 1)]
        public float LineGranularityMasking;
        public Color LineTint;
    }

    [System.Serializable]
    public struct CrumpleData
    {
        public Vector2Int CrumpleScale;
        [Range(0, 1)]
        public float CrumpleJitter;
        [Range(0, 1)]
        public float CrumpleStrength;
        [Range(1, 10)]
        public int CrumpleDetailLevel;
        [Range(0, 50)]
        public int CrumpleDetailFrequency;
        [Range(0, 1)]
        public float CrumpleDetailPersistence;
        [Range(0, 1)]
        public float CrumpleTintStrength;
        [Range(1, 10)]
        public float CrumpleTintSharpness;
        public Color CrumpleTint;
    }

    [System.Serializable]
    public struct NotebookLineData
    {
        [Range(0, 1)]
        public float NotebookLineGranularitySensitivity;
        
        public float HorizontalLineFrequency;
        [Range(0, 1)]
        public float HorizontalLineOffset;
        [Range(0, 1)]
        public float HorizontalLineThickness;
        public Color HorizontalLineTint;

        public float VerticalLineFrequency;
        [Range(0, 1)]
        public float VerticalLineOffset;
        [Range(0, 1)]
        public float VerticalLineThickness;
        public Color VerticalLineTint;
    }
    
    [System.Serializable]
    public struct WrinkleData
    {
        public Vector2Int WrinkleScale;
        [Range(0, 1)]
        public float WrinkleJitter;
        [Range(0, 1)]
        public float WrinkleStrength;
        [Range(1, 10)]
        public int WrinkleDetailLevel;
        [Range(0, 50)]
        public int WrinkleDetailFrequency;
        [Range(0, 1)]
        public float WrinkleDetailPersistence;
    }
}