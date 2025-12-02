using UnityEngine;
using Unity.Cinemachine;   // v3

public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance { get; private set; }

    [Tooltip("If false, no screen shake will be applied anywhere.")]
    public bool screenshakeEnabled = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Apply initial state to listeners
        ApplyToListeners(screenshakeEnabled);
    }

    public void SetEnabled(bool enabled)
    {
        screenshakeEnabled = enabled;
        Debug.Log($"[ScreenShakeManager] screenshakeEnabled = {enabled}");
        ApplyToListeners(enabled);
    }

    private void ApplyToListeners(bool enabled)
    {
        // Find all CinemachineImpulseListeners in the scene(s)
        var listeners = FindObjectsByType<CinemachineImpulseListener>(FindObjectsSortMode.None);
        foreach (var listener in listeners)
        {
            listener.enabled = enabled;
        }
    }
}
