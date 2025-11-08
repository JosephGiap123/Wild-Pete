using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SettingsOpener : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;   // assign SettingsUIRoot here

    bool isOpen = false;
    float prevTimeScale = 1f;

    void Awake()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
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

        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);

        if (isOpen)
        {
            // Pause
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // Optional: make cursor usable in menus
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // If you use an Input System Action Map for UI:
            // PlayerInput.all[0]?.SwitchCurrentActionMap("UI");
        }
        else
        {
            // Unpause
            Time.timeScale = prevTimeScale;

            // Optional: re-lock cursor for gameplay
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;

            // PlayerInput.all[0]?.SwitchCurrentActionMap("Gameplay");
        }
    }

    void OnDisable()
    {
        // Safety: if the script/scene disables while open, unpause
        if (isOpen) Time.timeScale = prevTimeScale;
    }
}
