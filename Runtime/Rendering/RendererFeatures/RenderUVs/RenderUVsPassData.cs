using SketchRenderer.Runtime.Rendering.Volume;
using UnityEngine;
using UnityEngine.Rendering;

namespace SketchRenderer.Runtime.Rendering.RendererFeatures
{
    [System.Serializable]
    public class RenderUVsPassData : ISketchRenderPassData<RenderUVsPassData>
    {
        [Range(0, 3)]
        public int SkyboxRotationStep = 0;
        [Range(0, 360)]
        private float SkyboxRotation => (float)SkyboxRotationStep * 120f;

        public int SkyboxScale;
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

        public RenderUVsPassData()
        {
            SkyboxRotationStep = 0;
            SkyboxScale = 10;
        }

        public void CopyFrom(RenderUVsPassData passData)
        {
            SkyboxRotationStep = passData.SkyboxRotationStep;
            SkyboxScale = passData.SkyboxScale;
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
            
            RenderUVsVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<RenderUVsVolumeComponent>();
            if (volumeComponent != null)
            {
                RenderUVsPassData passData = new RenderUVsPassData();
                passData.SkyboxRotationStep = volumeComponent.RotationStep.overrideState ? volumeComponent.RotationStep.value : SkyboxRotationStep;
                passData.SkyboxScale = volumeComponent.Scale.overrideState ? volumeComponent.Scale.value : SkyboxScale;
                if(passData.ShouldRotate)
                    passData.SkyboxRotationMatrix = ConstructRotationMatrix(passData.ExpectedRotation);
                return passData;
            }
            else
            {
                if (ShouldRotate)
                    SkyboxRotationMatrix = ConstructRotationMatrix(ExpectedRotation);

                return this;
            }
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