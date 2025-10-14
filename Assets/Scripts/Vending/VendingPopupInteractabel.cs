// Assets/Scripts/Vending/VendingPopupInteractable.cs
using UnityEngine;

public class VendingPopupInteractable : MonoBehaviour, IInteractable
{
    [Header("Popup Roots")]
    [SerializeField] private Canvas miniGameCanvas;     // Drag your MiniGameCanvas
    [SerializeField] private GameObject vendingPopup;   // Drag the VendingPopup (UI Image/panel)

    [Header("Keypad (optional)")]
    [SerializeField] private KeypadUI keypadPrefab;     // Drag KeypadUI prefab
    private KeypadUI keypadInstance;

    [Header("Screw Panel (optional)")]
    [SerializeField] private ScrewPanelUI screwPanelPrefab; // Drag ScrewPanelUI prefab
    private ScrewPanelUI screwPanelInstance;

    // --- IInteractable ---
    public bool CanInteract() => true;

    /// <summary>
    /// Called by your world interaction (e.g., player presses E).
    /// Shows the mini-game canvas and the vending popup screen.
    /// </summary>
    public void Interact()
    {
        if (miniGameCanvas && !miniGameCanvas.gameObject.activeSelf)
            miniGameCanvas.gameObject.SetActive(true);

        if (vendingPopup) vendingPopup.SetActive(true);

        Debug.Log("[VendingPopupInteractable] Popup shown");
    }

    /// <summary>
    /// Opens the Keypad UI, hides the vending image, and centers the keypad.
    /// Wire a Vending button/hotspot to this.
    /// </summary>
    public void OpenKeypad()
    {
        if (!EnsureCanvasActive()) return;

        if (!keypadPrefab)
        {
            Debug.LogError("[VendingPopupInteractable] Keypad prefab not assigned.");
            return;
        }

        if (!keypadInstance)
            keypadInstance = Instantiate(keypadPrefab, miniGameCanvas.transform, false);

        // Tell keypad where to return when Hide() is called
        keypadInstance.SetVendingPopup(vendingPopup);

        // Hide vending while keypad is up
        if (vendingPopup) vendingPopup.SetActive(false);

        // Bring to front & center at a predictable size
        var rt = keypadInstance.GetComponent<RectTransform>();
        keypadInstance.transform.SetAsLastSibling();
        rt.localScale = Vector3.one;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        // Lock a fixed on-screen size (tweak as you like)
        float w = 260f, h = 260f;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   h);

        keypadInstance.Show();
        Debug.Log("[VendingPopupInteractable] Keypad shown");
    }

    /// <summary>
    /// Opens the Screw Panel UI, hides the vending image, and centers the panel.
    /// Wire a Vending button/hotspot to this.
    /// </summary>
    public void OpenScrewPanel()
    {
        if (!EnsureCanvasActive()) return;

        if (!screwPanelPrefab)
        {
            Debug.LogError("[VendingPopupInteractable] ScrewPanel prefab not assigned.");
            return;
        }

        if (!screwPanelInstance)
            screwPanelInstance = Instantiate(screwPanelPrefab, miniGameCanvas.transform, false);

        // Tell screw panel where to return when Hide() is called
        screwPanelInstance.SetVendingPopup(vendingPopup);

        // Hide vending while screw panel is up
        if (vendingPopup) vendingPopup.SetActive(false);

        // Bring to front & center
        var rt = screwPanelInstance.GetComponent<RectTransform>();
        screwPanelInstance.transform.SetAsLastSibling();
        rt.localScale = Vector3.one;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        screwPanelInstance.Show();
        Debug.Log("[VendingPopupInteractable] Screw panel shown");
    }

    /// <summary>
    /// Vending-level Exit: hide keypad/screw panels (if present),
    /// hide the vending screen, and disable the mini-game canvas.
    /// Wire the VendingPopup "X" button to this.
    /// </summary>
    public void CloseAll()
    {
        if (keypadInstance) keypadInstance.Hide();
        if (screwPanelInstance) screwPanelInstance.Hide();

        if (vendingPopup) vendingPopup.SetActive(false);
        if (miniGameCanvas) miniGameCanvas.gameObject.SetActive(false);

        Debug.Log("[VendingPopupInteractable] Closed all UI");
    }

    // --- helpers ---
    private bool EnsureCanvasActive()
    {
        if (!miniGameCanvas)
        {
            Debug.LogError("[VendingPopupInteractable] MiniGameCanvas not assigned.");
            return false;
        }
        if (!miniGameCanvas.gameObject.activeSelf)
            miniGameCanvas.gameObject.SetActive(true);
        return true;
    }
}
