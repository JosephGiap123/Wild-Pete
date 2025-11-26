using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SettingsOpener : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;   // assign SettingsUIRoot here

    float prevTimeScale = 1f;

    void Awake()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle with Y
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
        if (settingsPanel == null) return;

        bool newOpenState = !settingsPanel.activeSelf;
        SetOpen(newOpenState);
    }

    // This is what the Settings UI (red X button) will call
    public void SetOpen(bool open)
    {
        if (settingsPanel == null) return;

        settingsPanel.SetActive(open);

        if (open)
        {
            // Pause
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // Unpause
            Time.timeScale = prevTimeScale;

            // If you want to relock the cursor in gameplay scenes:
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }
    }

    void OnDisable()
    {
        // Safety: if something disables this while menu is open, unpause
        if (settingsPanel != null && settingsPanel.activeSelf)
            Time.timeScale = prevTimeScale;
    }
}
