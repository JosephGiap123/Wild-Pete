using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsUIController : MonoBehaviour
{
    [Header("Main Buttons")]
    public GameObject btnBackToGame;
    public GameObject btnGameSettings;
    public GameObject btnControlSettings;
    public GameObject btnCheatMode;
    public GameObject btnQuitToMenu;

    [Header("Panels")]
    public GameObject gameSettingsPanel;
    public GameObject controlSettingsPanel;
    public GameObject cheatPanel;

    [Header("Main Menu Scene Name")]
    public string mainMenuSceneName = "Main Menu 1";

    [Header("Opener (for pause/unpause)")]
    public SettingsOpener settingsOpener;

    // 🔽 ADD THIS BLOCK 🔽
    [Header("Cheat Mode Visuals")]
    public Image cheatModeButtonImage;
    public Color cheatOffColor = Color.white;
    public Color cheatOnColor = Color.green;
    // 🔼 ADD THIS BLOCK 🔼

    [Header("Main Menu Panel (optional)")]
    public GameObject mainMenuPanel;


    [Header("Audio")]
    [SerializeField] private AudioMixerGroup sfxMixer;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioSource audioSrc;



    private void Awake()
    {
        if (settingsOpener == null)
        {
            settingsOpener = FindObjectOfType<SettingsOpener>();
        }

        // Auto-find the Image for the cheat button if not set
        if (cheatModeButtonImage == null && btnCheatMode != null)
        {
            cheatModeButtonImage = btnCheatMode.GetComponent<Image>();
        }
    }


    private void OnEnable()
    {
        ShowSettingsHub();
        SyncCheatVisual();
    }

    void ShowSettingsHub()
    {
        btnBackToGame.SetActive(true);
        btnGameSettings.SetActive(true);
        btnControlSettings.SetActive(true);
        btnCheatMode.SetActive(true);
        btnQuitToMenu.SetActive(true);

        gameSettingsPanel.SetActive(false);
        if (controlSettingsPanel != null) controlSettingsPanel.SetActive(false);
        if (cheatPanel != null) cheatPanel.SetActive(false);
    }

    void HideHubButtons()
    {
        btnBackToGame.SetActive(false);
        btnGameSettings.SetActive(false);
        btnControlSettings.SetActive(false);
        btnCheatMode.SetActive(false);
        btnQuitToMenu.SetActive(false);
    }

    // ---------- HUB BUTTON HANDLERS ----------

    public void OnGameSettingsPressed()
    {
        PlayClickSound();
        HideHubButtons();
        gameSettingsPanel.SetActive(true);
    }

    public void OnControlSettingsPressed()
    {
        PlayClickSound();
        HideHubButtons();
        if (controlSettingsPanel != null) controlSettingsPanel.SetActive(true);
    }

    // 🔴 CHEAT: this should just toggle, not open another panel
    public void OnCheatModePressed()
    {
        PlayClickSound();
        if (CheatManager.Instance != null)
        {
            CheatManager.Instance.ToggleInvulnerability();
            SyncCheatVisual();
        }
        else
        {
            Debug.LogWarning("SettingsUIController: CheatManager.Instance is null, cannot toggle cheat.");
        }
    }

    public void OnQuitToMenuPressed()
    {
        PlayClickSound();
        // Clear inventory
        if (PlayerInventory.instance != null)
        {
            PlayerInventory.instance.ClearInventory();

            // Clear all equipment slots
            if (PlayerInventory.instance.equipmentSlots != null)
            {
                foreach (var equipmentSlot in PlayerInventory.instance.equipmentSlots)
                {
                    if (equipmentSlot != null)
                    {
                        equipmentSlot.ClearSlot();
                    }
                }
            }
        }

        // Clear checkpoint
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.ClearCheckpoint();
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ---------- BACK BUTTON HANDLERS ----------

    public void OnBackToSettingsHubPressed()
    {
        PlayClickSound();
        ShowSettingsHub();
    }

    public void OnBackToGamePressed()
    {
        PlayClickSound();
        if (settingsOpener != null)
        {
            // Gameplay scenes: use opener to unpause + hide menu
            settingsOpener.CloseFromUI();
        }
        else
        {
            // Main menu scene: show the main menu UI again
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            // Hide the settings root (SettingsMenu / SettingsUIRoot)
            gameObject.SetActive(false);
        }
    }

    //audio stuff man
    private void PlayClickSound(bool detach = true)
    {
        if (detach)
        {
            var temp = new GameObject("ClickSound_Temp").AddComponent<AudioSource>();
            temp.spatialBlend = 0f;
            temp.outputAudioMixerGroup = sfxMixer;
            temp.PlayOneShot(clickSound);
            Destroy(temp.gameObject, clickSound.length);
        }
        if (audioSrc != null)
        {
            audioSrc.PlayOneShot(clickSound);
        }
    }


    // ---------- CHEAT VISUALS ----------

    private void SyncCheatVisual()
    {
        if (cheatModeButtonImage == null) return;

        bool on = CheatManager.Instance != null && CheatManager.Instance.invulnerable;
        cheatModeButtonImage.color = on ? cheatOnColor : cheatOffColor;
    }
}
