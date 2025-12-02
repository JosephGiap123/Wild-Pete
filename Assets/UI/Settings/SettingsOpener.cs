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
        // Don't allow opening settings if vending machine UI is open
        if (IsVendingMachineUIOpen())
        {
            return;
        }

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
        // Don't allow opening settings if vending machine UI is open
        if (!isOpen && IsVendingMachineUIOpen())
        {
            return;
        }

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

    /// <summary>
    /// Checks if the vending machine UI is currently open
    /// </summary>
    private bool IsVendingMachineUIOpen()
    {
        // Find all VendingPopupInteractable instances and check if their UI is active
        VendingPopupInteractable[] vendingMachines = FindObjectsByType<VendingPopupInteractable>(FindObjectsSortMode.None);
        foreach (VendingPopupInteractable vending in vendingMachines)
        {
            if (vending == null) continue;

            // Use reflection to check if vendingPopup or miniGameCanvas is active
            var vendingPopupField = typeof(VendingPopupInteractable).GetField("vendingPopup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var miniGameCanvasField = typeof(VendingPopupInteractable).GetField("miniGameCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (vendingPopupField != null)
            {
                GameObject vendingPopup = vendingPopupField.GetValue(vending) as GameObject;
                if (vendingPopup != null && vendingPopup.activeSelf)
                {
                    return true;
                }
            }

            if (miniGameCanvasField != null)
            {
                Canvas miniGameCanvas = miniGameCanvasField.GetValue(vending) as Canvas;
                if (miniGameCanvas != null && miniGameCanvas.gameObject.activeSelf)
                {
                    return true;
                }
            }
        }

        return false;
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
