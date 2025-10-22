using SketchRenderer.Runtime.Rendering.Volume;
using UnityEngine;

public class SketchLightningController : MonoBehaviour
{
    [SerializeField] 
    private GameObject mainLight;
    [SerializeField]
    private Vector3 minRotation;
    [SerializeField]
    private Vector3 maxRotation;

    private bool increasing;
    [SerializeField]
    [Range(0f, 1f)]
    public float Speed;

    public void Update()
    {
        if (increasing)
        {
            Vector3 targetRot = Vector3.MoveTowards(mainLight.transform.rotation.eulerAngles, maxRotation, Speed * 100f * Time.deltaTime);
            mainLight.transform.rotation = Quaternion.Euler(targetRot);
            if(Vector3.Magnitude(targetRot - minRotation) < 0.01f)
                increasing = false;
        }
        else
        {
            Vector3 targetRot = Vector3.MoveTowards(mainLight.transform.rotation.eulerAngles, minRotation, Speed * 100f * Time.deltaTime);
            mainLight.transform.rotation = Quaternion.Euler(targetRot);
            if(Vector3.Magnitude(targetRot - minRotation) < 0.01f)
                increasing = true;
        }
    }
}
