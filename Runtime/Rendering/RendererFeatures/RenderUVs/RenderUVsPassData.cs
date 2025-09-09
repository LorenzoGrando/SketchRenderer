using SketchRenderer.Runtime.Rendering.Volume;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    [System.Serializable]
    public class RenderUVsPassData : ISketchRenderPassData<RenderUVsPassData>
    {
        [Header("Base Parameters")] 
        [Range(0, 360)]
        public float SkyboxRotation;
        private float ExpectedRotation { get {return Mathf.Floor(SkyboxRotation/90) * 90;}}
        [HideInInspector] 
        public Matrix4x4 SkyboxRotationMatrix;
        public bool ShouldRotate
        {
            get
            {
                float rot = ExpectedRotation;
                return rot > 0f && rot < 360f;
            }
        }

        public void CopyFrom(RenderUVsPassData passData)
        {
            SkyboxRotation = passData.SkyboxRotation;
            SkyboxRotationMatrix = ConstructRotationMatrix(ExpectedRotation);
        }
    
        public bool IsAllPassDataValid()
        {
            return true;
        }

        public RenderUVsPassData GetPassDataByVolume()
        {
            if(VolumeManager.instance == null || VolumeManager.instance.stack == null)
                return this;
            LuminanceVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<LuminanceVolumeComponent>();
            if (volumeComponent != null)
                SkyboxRotation = volumeComponent.SkyboxRotation.overrideState ? Mathf.Lerp(0, 360,volumeComponent.SkyboxRotation.value) : SkyboxRotation;
            if (ShouldRotate)
            {
                SkyboxRotationMatrix = ConstructRotationMatrix(ExpectedRotation);
            }

            return this;
        }

        private Matrix4x4 ConstructRotationMatrix(float beta)
        {
            beta = Mathf.Deg2Rad * beta;
            Matrix4x4 rot = new Matrix4x4();
            rot.m00 = Mathf.Cos(beta);
            rot.m01 = -Mathf.Sin(beta);
            rot.m10 = Mathf.Sin(beta);
            rot.m11 = Mathf.Cos(beta);
        
            return rot;
        }
    }
}