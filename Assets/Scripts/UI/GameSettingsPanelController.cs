using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameSettingsPanelController : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Slider masterVolumeSlider;   // VolumeSlider

    [Header("SFX Volume")]
    [SerializeField] private Slider sfxSlider;            // SFXSlider
    [SerializeField] private AudioMixer masterMixer;      // MasterMixer asset
    private const string SfxVolumeParam = "SFXVolume";    // exposed param name

    [Header("Screenshake Button")]
    [SerializeField] private Image screenshakeButtonImage; // Btn_Screenshake Image
    [SerializeField] private Color screenshakeOffColor = Color.green;
    [SerializeField] private Color screenshakeOnColor  = Color.red;

    // start ON (set false if you want default off)
    private bool isScreenshakeOn = true;



    void Awake()
    {
        // ---- Volume init ----
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.wholeNumbers = false;
            masterVolumeSlider.value = 1f;   // full volume
        }

        AudioListener.volume = 1f;

        // ---- Screenshake visual init ----
        UpdateScreenshakeVisual();

        // Make sure manager reflects initial state if it exists
        if (ScreenShakeManager.Instance != null)
            ScreenShakeManager.Instance.SetEnabled(isScreenshakeOn);
    }

    void Update()
    {
        // Volume follows slider every frame
        if (masterVolumeSlider != null)
        {
            float vol = Mathf.Clamp01(masterVolumeSlider.value);
            AudioListener.volume = vol;
        }
    }

    // not used anymore, but safe to keep
    public void OnMasterVolumeChanged(float value) { }

    // === CALLED BY Btn_Screenshake OnClick ===
    public void OnScreenshakeButtonPressed()
    {
        // flip true/false
        isScreenshakeOn = !isScreenshakeOn;
        Debug.Log($"[GameSettings] Screenshake is now: {isScreenshakeOn}");

        UpdateScreenshakeVisual();

        // Tell the global manager (this is what actually turns shake on/off)
        if (ScreenShakeManager.Instance != null)
            ScreenShakeManager.Instance.SetEnabled(isScreenshakeOn);
    }

    private void UpdateScreenshakeVisual()
    {
        if (screenshakeButtonImage == null) return;

        screenshakeButtonImage.color = isScreenshakeOn
            ? screenshakeOnColor
            : screenshakeOffColor;
    }

    private void ApplySfxVolume(float sliderValue)
    {    
    if (masterMixer == null) return;

    float v = Mathf.Clamp(sliderValue, 0.0001f, 1f);
    float dB = Mathf.Log10(v) * 20f;
    masterMixer.SetFloat(SfxVolumeParam, dB);
    }


    public void OnSfxVolumeChanged(float value)
    {
        ApplySfxVolume(value);
    }

}
