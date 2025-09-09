using System;
using UnityEngine;
using SketchRenderer.Runtime.Data;

namespace SketchRenderer.Runtime.TextureTools.TonalArtMap
{
    [CreateAssetMenu(fileName = "TonalArtMapAsset", menuName = SketchRendererData.PackageAssetItemPath + "TonalArtMapAsset")]
    public class TonalArtMapAsset : ScriptableObject
    {
        [Range(1, 9)] 
        public int ExpectedTones = 6;
        [SerializeField] [HideInInspector]
        private int numberOfTones;
        public int TotalTones
        {
            get => numberOfTones; private set => numberOfTones = value; 
        }
        [HideInInspector]
        public Texture2D[] Tones = new Texture2D[1];

        public bool ForceFirstToneFullWhite = true;
        public bool ForceFinalToneFullBlack = false;
        [SerializeField] [HideInInspector]
        private bool isFirstToneFullWhite = true;
        [SerializeField] [HideInInspector]
        private bool isFinalToneFullBlack = false;
    
        [SerializeField] [HideInInspector] public bool isPrePacked = false;
        [SerializeField] [HideInInspector] public Vector4 TAMBasisDirection;
    
        public bool IsPacked {get {return isPrePacked;}}

        private void OnEnable()
        {
            if(Tones == null)
                Tones = new Texture2D[ExpectedTones];
        }

        public float GetHomogenousFillRateThreshold()
        {
            return 1f/(float)ExpectedTones;
        }

        public void ResetTones()
        {
            Tones = new Texture2D[ExpectedTones];
            isPrePacked = false;
        }

        public void SetPackedTams(Texture2D[] packedTams)
        {
            Tones = packedTams;
            TotalTones = ExpectedTones;
            isFirstToneFullWhite = ForceFirstToneFullWhite;
            isFinalToneFullBlack = ForceFinalToneFullBlack;
            isPrePacked = true;
        }

        public bool HasDirtyProperties()
        {
            return (ExpectedTones != TotalTones) || (ForceFirstToneFullWhite != isFirstToneFullWhite || (ForceFinalToneFullBlack != isFinalToneFullBlack));
        }
    }
}