using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;   // IMPORTANT for Image

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
    public GameObject cheatPanel;   // not used yet, can be null

    [Header("Cheat Mode Visuals")]
    public Image cheatButtonImage;       // assign in Inspector
    public Color cheatOffColor = Color.white;
    public Color cheatOnColor = Color.green;

    [Header("Main Menu Scene Name")]
    public string mainMenuSceneName = "Main Menu 1";


    private void Awake()
    {
        // Ensure the settings menu starts hidden at runtime
        if (Application.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ShowSettingsHub();
        UpdateCheatVisual();
    }

    void ShowSettingsHub()
    {
        // show hub buttons
        btnBackToGame.SetActive(true);
        btnGameSettings.SetActive(true);
        btnControlSettings.SetActive(true);
        btnCheatMode.SetActive(true);
        btnQuitToMenu.SetActive(true);

        // hide sub-panels
        gameSettingsPanel.SetActive(false);
        if (controlSettingsPanel != null) controlSettingsPanel.SetActive(false);
        if (cheatPanel != null) cheatPanel.SetActive(false);

        UpdateCheatVisual();
    }

    void HideHubButtons()
    {
        btnBackToGame.SetActive(false);
        btnGameSettings.SetActive(false);
        btnControlSettings.SetActive(false);
        btnCheatMode.SetActive(false);
        btnQuitToMenu.SetActive(false);
    }

    // ----- hub buttons -----

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

    // THIS is your toggle behavior
    public void OnCheatModePressed()
    {
        if (CheatManager.Instance != null)
        {
            CheatManager.Instance.ToggleInvulnerability();
            UpdateCheatVisual();
        }
        else
        {
            Debug.LogWarning("CheatManager instance not found in scene.");
        }
    }

    public void OnQuitToMenuPressed()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ----- back buttons -----

    public void OnBackToSettingsHubPressed()
    {
        ShowSettingsHub();
    }

    public void OnBackToGamePressed()
    {
        var opener = FindObjectOfType<SettingsOpener>();
        if (opener != null)
        {
            opener.SetOpen(false);
        }
        else
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }
    }

    // ----- helper -----

    void UpdateCheatVisual()
    {
        if (cheatButtonImage == null) return;

        bool on = CheatManager.Instance != null && CheatManager.Instance.invulnerable;
        cheatButtonImage.color = on ? cheatOnColor : cheatOffColor;
    }
}
