using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameSettingsPanelController : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Slider masterVolumeSlider;   // VolumeSlider

    [Header("Music Volume")]
    [SerializeField] private Slider musicSlider;      // <- NEW
    private const string MusicVolumeParam = "MusicVolume";

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
    // ---- Master init ----
    if (masterVolumeSlider != null)
    {
        masterVolumeSlider.minValue = 0f;
        masterVolumeSlider.maxValue = 1f;
        masterVolumeSlider.wholeNumbers = false;
        masterVolumeSlider.value = 1f;
    }
    AudioListener.volume = 1f;

    // ---- Music init ----
    if (musicSlider != null)
    {
        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;
        musicSlider.wholeNumbers = false;
        musicSlider.value = 1f;
    }
    ApplyMusicVolume(musicSlider != null ? musicSlider.value : 1f);   // <- NEW

    // ---- SFX init ----
    if (sfxSlider != null)
    {
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;
        sfxSlider.wholeNumbers = false;
        sfxSlider.value = 1f;
    }
    ApplySfxVolume(sfxSlider != null ? sfxSlider.value : 1f);

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

    // === MUSIC ===
    public void OnMusicVolumeChanged(float value)
    {
        ApplyMusicVolume(value);
    }

    private void ApplyMusicVolume(float sliderValue)
    {
        if (masterMixer == null) return;

        // convert 0–1 slider to decibels
        float v  = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float dB = Mathf.Log10(v) * 20f;

        masterMixer.SetFloat(MusicVolumeParam, dB);
        // Debug.Log($"[GameSettings] Music slider = {sliderValue}, dB = {dB}");
    }


}
