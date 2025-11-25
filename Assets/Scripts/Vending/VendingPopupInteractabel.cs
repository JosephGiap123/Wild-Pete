// Assets/Scripts/Vending/VendingPopupInteractable.cs
using UnityEngine;
using UnityEngine.UI;

public class VendingPopupInteractable : MonoBehaviour, IInteractable
{
    [Header("Popup Roots")]
    [SerializeField] private Canvas miniGameCanvas;     // Drag your MiniGameCanvas
    [SerializeField] private GameObject vendingPopup;   // Drag the VendingPopup (UI Image/panel)

    [Header("Vending Machine Sprites")]
    [SerializeField] private Sprite emptyVendingSprite; // Sprite to show when vending machine is empty (after code is correct)

    [Header("Keypad (optional)")]
    [SerializeField] private KeypadUI keypadPrefab;     // Drag KeypadUI prefab
    private KeypadUI keypadInstance;

    [Header("Screw Panel (optional)")]
    [SerializeField] private ScrewPanelUI screwPanelPrefab; // Drag ScrewPanelUI prefab
    private ScrewPanelUI screwPanelInstance;

    // --- IInteractable ---
    public bool CanInteract() => true;

    // Called by your world interaction (e.g., player presses E).
    // Shows the mini-game canvas and the vending popup screen.
    public void Interact()
    {
        if (miniGameCanvas && !miniGameCanvas.gameObject.activeSelf)
            miniGameCanvas.gameObject.SetActive(true);

        if (vendingPopup) vendingPopup.SetActive(true);

        Debug.Log("[VendingPopupInteractable] Popup shown");
    }

    // Opens the Keypad UI, hides the vending image, and centers the keypad.
    // Wire a Vending button/hotspot to this.
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
        
        // Tell keypad about this controller so it can change the sprite when code is correct
        keypadInstance.SetVendingPopupController(this);
        
        // Link wire game reference to keypad so it knows if wires are connected
        // Wire game is inside the screw panel, so find it there
        if (screwPanelInstance != null)
        {
            var wireGame = screwPanelInstance.GetComponentInChildren<WireConnectionGame>(true);
            if (wireGame != null)
            {
                keypadInstance.SetWireGameReference(wireGame);
                Debug.Log("[VendingPopupInteractable] Linked wire game to keypad");
            }
            else
            {
                Debug.LogWarning("[VendingPopupInteractable] Wire game not found in screw panel instance");
            }
        }

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

    // Opens the Screw Panel UI, hides the vending image, and centers the panel.
    // Wire a Vending button/hotspot to this.
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

        // Check if wire game is already complete - if so, don't allow reopening
        var wireGame = screwPanelInstance.GetComponentInChildren<WireConnectionGame>(true);
        if (wireGame != null && wireGame.IsComplete())
        {
            Debug.Log("[VendingPopupInteractable] Wire game already complete - side panel disabled");
            return; // Don't open the panel if game is complete
        }

        // Tell screw panel where to return when Hide() is called
        screwPanelInstance.SetVendingPopup(vendingPopup);

        // Hide vending while screw panel is up
        if (vendingPopup) vendingPopup.SetActive(false);

        // Bring to front and ensure exact dimensions match prefab
        var rt = screwPanelInstance.GetComponent<RectTransform>();
        screwPanelInstance.transform.SetAsLastSibling();
        
        // Match closed panel dimensions exactly:
        // Anchor: center-center (0.5, 0.5)
        // Position: (0, 3.2)
        // Size: 800x600
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0.0000030994f, 3.2f);
        rt.sizeDelta = new Vector2(800f, 600f);
        rt.localScale = Vector3.one;

        screwPanelInstance.Show();
        Debug.Log("[VendingPopupInteractable] Screw panel shown");
    }

    // Vending-level Exit: hide keypad/screw panels (if present),
    // hide the vending screen, and disable the mini-game canvas.
    // Wire the VendingPopup "X" button to this.
    public void CloseAll()
    {
        if (keypadInstance) keypadInstance.Hide();
        if (screwPanelInstance) screwPanelInstance.Hide();

        if (vendingPopup) vendingPopup.SetActive(false);
        if (miniGameCanvas) miniGameCanvas.gameObject.SetActive(false);

        Debug.Log("[VendingPopupInteractable] Closed all UI");
    }

    // Called when the keypad code is correct - changes vending machine to empty sprite
    public void OnVendingMachineEmpty()
    {
        if (vendingPopup == null)
        {
            Debug.LogWarning("[VendingPopupInteractable] VendingPopup is null, cannot change sprite");
            return;
        }
        
        // Find the Image component on the vending popup
        Image vendingImage = vendingPopup.GetComponent<Image>();
        if (vendingImage == null)
        {
            // Try to find it in children
            vendingImage = vendingPopup.GetComponentInChildren<Image>();
        }
        
        if (vendingImage != null && emptyVendingSprite != null)
        {
            vendingImage.sprite = emptyVendingSprite;
            Debug.Log("[VendingPopupInteractable] Changed vending machine sprite to empty version");
        }
        else
        {
            if (vendingImage == null)
            {
                Debug.LogWarning("[VendingPopupInteractable] No Image component found on vendingPopup GameObject");
            }
            if (emptyVendingSprite == null)
            {
                Debug.LogWarning("[VendingPopupInteractable] Empty vending sprite not assigned");
            }
        }
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
