using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void OnEnable()
    {
        ShowSettingsHub();
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

    public void OnCheatModePressed()
    {
        HideHubButtons();
        if (cheatPanel != null) cheatPanel.SetActive(true);
    }

    public void OnQuitToMenuPressed()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ----- back buttons -----

    // Back inside GameSettings/Control/Cheat → return to 4-button hub
    public void OnBackToSettingsHubPressed()
    {
        ShowSettingsHub();
    }

    // Back on the hub → close settings and return to game
    public void OnBackToGamePressed()
{
    // Try to close via SettingsOpener if it exists
    var opener = FindObjectOfType<SettingsOpener>();
    if (opener != null)
    {
        opener.SetOpen(false);
    }
    else
    {
        // Fallback (e.g., in main menu scene without opener)
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}

}
