using UnityEngine;
using Unity.Cinemachine;   // v3 namespace

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    [Header("Defaults (can be overridden per-call)")]
    [SerializeField] float defaultAmplitude = 1.5f;
    [SerializeField] float defaultFrequency = 2f;

    CinemachineImpulseSource impulse;

    void Awake()
    {
        Instance = this;
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float? amplitude = null, float? frequency = null)
    {
        // 🔴 GLOBAL KILL SWITCH
        if (ScreenShakeManager.Instance != null &&
            !ScreenShakeManager.Instance.screenshakeEnabled)
        {
            // Screenshake is globally OFF → do nothing
            return;
        }

        if (!impulse) return;

        // update definition on the fly (optional)
        impulse.ImpulseDefinition.AmplitudeGain = amplitude ?? defaultAmplitude;
        impulse.ImpulseDefinition.FrequencyGain = frequency ?? defaultFrequency;

        impulse.GenerateImpulse();                 // omni
        // or: impulse.GenerateImpulse(Vector3.down); // directional
    }

    // quick test key so you can verify it works
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))  // change/remove later
            Shake();
    }
}
