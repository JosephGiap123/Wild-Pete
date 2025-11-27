using UnityEngine;
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
        // Toggle with Y in gameplay scenes
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)
            Toggle();
        #else
        if (Input.GetKeyDown(KeyCode.Y))
            Toggle();
        #endif
    }

    public void Toggle()
    {
        // If panel is not yet assigned, try to find it
        if (settingsPanel == null)
        {
            var controller = FindObjectOfType<SettingsUIController>();
            if (controller != null)
                settingsPanel = controller.gameObject;
        }

        if (settingsPanel == null)
        {
            Debug.LogWarning("SettingsOpener: No SettingsUIRoot found in scene.");
            return;
        }

        if (isOpen)
            CloseInternal();
        else
            OpenInternal();
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
