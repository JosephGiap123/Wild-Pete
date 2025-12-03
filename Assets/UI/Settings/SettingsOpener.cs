using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SettingsOpener : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;   // will be auto-filled at runtime

    bool isOpen = false;
    float prevTimeScale = 1f;

    void Awake()
    {
        // Don't assume it's assigned in Inspector anymore.
        // Just make sure it's not visible at startup if it already exists.
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Don't allow opening settings in menu scenes via shortcut
        string currentSceneName = SceneManager.GetActiveScene().name;
        if ((currentSceneName.Contains("Menu") || currentSceneName.Contains("menu")) && !isOpen)
        {
            return;
        }

        // Settings can only open if game isn't paused
        if (PauseController.IsGamePaused && !isOpen)
        {
            return;
        }

        KeyCode settingsKey = KeyCode.Y; // Default fallback

        // Try to get settings key from ControlManager
        if (ControlManager.instance != null && ControlManager.instance.inputMapping != null)
        {
            if (ControlManager.instance.inputMapping.ContainsKey(PlayerControls.Setting))
            {
                settingsKey = ControlManager.instance.inputMapping[PlayerControls.Setting];
            }
        }

        // Check for key press
        if (Input.GetKeyDown(settingsKey))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        // If panel is not yet assigned, try to find it
        if (settingsPanel == null)
        {
            var controller = FindFirstObjectByType<SettingsUIController>();
            if (controller != null)
                settingsPanel = controller.gameObject;
        }

        if (settingsPanel == null)
        {
            Debug.LogWarning("SettingsOpener: No SettingsUIRoot found in scene.");
            return;
        }

        if (isOpen)
        {
            // Settings can only close if it was opened (isOpen flag is true)
            CloseInternal();
        }
        else
        {
            // Settings can only open if game isn't paused
            if (PauseController.IsGamePaused)
            {
                return;
            }
            OpenInternal();
        }
    }

    void OpenInternal()
    {
        isOpen = true;
        settingsPanel.SetActive(true);

        // Pause
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void CloseInternal()
    {
        isOpen = false;
        settingsPanel.SetActive(false);

        // Unpause
        Time.timeScale = prevTimeScale;
        // If you relock the cursor in gameplay, do it here:
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    // ----- methods for UI buttons -----

    public void OpenFromUI()
    {
        if (!isOpen)
            Toggle();   // Toggle will lazy-find the panel if needed
    }

    public void CloseFromUI()
    {
        if (isOpen)
            CloseInternal();
    }

    void OnDisable()
    {
        // Safety: if this gets disabled while open, unpause
        if (isOpen)
        {
            isOpen = false;
            Time.timeScale = prevTimeScale;
        }
    }
}
