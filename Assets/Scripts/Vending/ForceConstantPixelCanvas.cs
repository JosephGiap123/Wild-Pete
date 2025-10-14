using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class ForceConstantPixelCanvas : MonoBehaviour
{
    [SerializeField] private float scaleFactor = 1f; // usually 1

    void Awake()
    {
        var scaler = GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = scaleFactor;
        // Optional: keep default reference pixels per unit
        // scaler.referencePixelsPerUnit = 100f;
    }
}
