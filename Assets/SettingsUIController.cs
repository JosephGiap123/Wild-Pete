using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;      // <- make sure you have this at the top!

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
        HideHubButtons();
        gameSettingsPanel.SetActive(true);
    }

    public void OnControlSettingsPressed()
    {
        HideHubButtons();
        if (controlSettingsPanel != null) controlSettingsPanel.SetActive(true);
    }

    // 🔴 CHEAT: this should just toggle, not open another panel
    public void OnCheatModePressed()
    {
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
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ---------- BACK BUTTON HANDLERS ----------

    public void OnBackToSettingsHubPressed()
    {
        ShowSettingsHub();
    }

    public void OnBackToGamePressed()
{
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


    // ---------- CHEAT VISUALS ----------

    private void SyncCheatVisual()
    {
        if (cheatModeButtonImage == null) return;

        bool on = CheatManager.Instance != null && CheatManager.Instance.invulnerable;
        cheatModeButtonImage.color = on ? cheatOnColor : cheatOffColor;
    }
}
