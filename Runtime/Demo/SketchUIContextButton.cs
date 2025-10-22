using TMPro;
using UnityEngine;

public class SketchUIContextButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textMesh;
    
    public void SetText(string text) => textMesh.text = text;
}
