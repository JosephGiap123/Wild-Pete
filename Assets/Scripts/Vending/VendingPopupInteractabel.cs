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
    [SerializeField] private Sprite normalVendingSprite; // Original sprite for the vending machine (for restoration)

    [Header("Keypad (optional)")]
    [SerializeField] private KeypadUI keypadPrefab;     // Drag KeypadUI prefab
    private KeypadUI keypadInstance;

    [Header("Screw Panel (optional)")]
    [SerializeField] private ScrewPanelUI screwPanelPrefab; // Drag ScrewPanelUI prefab
    private ScrewPanelUI screwPanelInstance;
    
    [Header("Screwdriver Requirement")]
    [SerializeField] private string screwdriverItemName = "Screwdriver"; // Name of the screwdriver item in inventory (must match ItemSO name)

    [Header("Bread Drop")]
    [SerializeField] private GameObject itemPrefab; // Item prefab to instantiate (same one used for other items, like Item.prefab)
    [SerializeField] private string breadItemName = "Bread"; // Name of the bread item in ItemSO (must match exactly)
    [SerializeField] private Transform breadDropPosition; // Where to drop the bread (optional - if null, uses vending machine position)
    
    // Track if bread has been collected (game completed once)
    private bool breadCollected = false;
    

    // Public property for checkpoint system
    public bool IsBreadCollected => breadCollected;
    public void SetBreadCollected(bool value)
    {
        breadCollected = value;
        // Update sprite immediately when state changes
        UpdateVendingSprite();

        // If restoring to uncollected state, reset mini-games
        if (!breadCollected)
        {
            ResetMiniGames();
        }
    }

    private void ResetMiniGames()
    {
        // Reset keypad if it exists
        if (keypadInstance != null)
        {
            keypadInstance.ResetGame();
        }

        // Reset screw panel and wire game if it exists
        if (screwPanelInstance != null)
        {
            // Reset the entire panel state (this will reset wasOpenedBefore, wire game, etc.)
            var screwPanel = screwPanelInstance.GetComponent<ScrewPanelUI>();
            if (screwPanel != null)
            {
                screwPanel.ResetPanel();
            }

            // Reset screws in the panel
            var screws = screwPanelInstance.GetComponentsInChildren<Screw>(true);
            foreach (var screw in screws)
            {
                if (screw != null)
                {
                    screw.ResetScrew();
                }
            }
        }

        Debug.Log("[VendingPopupInteractable] Reset mini-games to initial state");
    }

    private void UpdateVendingSprite()
    {
        if (vendingPopup == null) return;

        Image vendingImage = vendingPopup.GetComponent<Image>();
        if (vendingImage == null)
        {
            vendingImage = vendingPopup.GetComponentInChildren<Image>();
        }

        if (vendingImage != null)
        {
            if (breadCollected && emptyVendingSprite != null)
            {
                vendingImage.sprite = emptyVendingSprite;
            }
            else if (!breadCollected && normalVendingSprite != null)
            {
                vendingImage.sprite = normalVendingSprite;
            }
        }
    }

    private void OnEnable()
    {
        // Subscribe to player death event to close vending popup when player dies
        if (HealthManager.instance != null)
        {
            HealthManager.instance.OnPlayerDeath += OnPlayerDeath;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from player death event
        if (HealthManager.instance != null)
        {
            HealthManager.instance.OnPlayerDeath -= OnPlayerDeath;
        }
    }
    
    private void OnDestroy()
    {
        // Safety: unsubscribe in case OnDisable wasn't called
        if (HealthManager.instance != null)
        {
            HealthManager.instance.OnPlayerDeath -= OnPlayerDeath;
        }

        // Unregister from CheckpointManager
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.UnregisterVendingMachine(this);
        }
    }
    
    private void OnPlayerDeath()
    {
        // Close all vending UI when player dies
        CloseAll();
        Debug.Log("[VendingPopupInteractable] Player died - closed all vending UI");
    }

    private void Start()
    {
        // Register with CheckpointManager
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RegisterVendingMachine(this);
        }

        // Update sprite based on current state (in case it was restored before Start was called)
        UpdateVendingSprite();
    }

    // --- IInteractable ---
    public bool CanInteract() => true;

    public string InteractMessage()
    {
        return " to use the vending machine";
    }

    // Called by your world interaction (e.g., player presses E).
    // Shows the mini-game canvas and the vending popup screen.
    public void Interact()
    {
        if (miniGameCanvas && !miniGameCanvas.gameObject.activeSelf)
            miniGameCanvas.gameObject.SetActive(true);

        if (vendingPopup) vendingPopup.SetActive(true);

        // Pause the game when popup opens
        PauseController.SetPause(true);
        
        // If bread was already collected, show empty vending machine sprite
        if (breadCollected && emptyVendingSprite != null)
        {
            Image vendingImage = vendingPopup.GetComponent<Image>();
            if (vendingImage == null)
            {
                vendingImage = vendingPopup.GetComponentInChildren<Image>();
            }
            
            if (vendingImage != null)
            {
                vendingImage.sprite = emptyVendingSprite;
                Debug.Log("[VendingPopupInteractable] Showing empty vending machine (bread already collected)");
            }
        }

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

        // If instance doesn't exist or is inactive, create/recreate it
        if (keypadInstance == null)
        {
            keypadInstance = Instantiate(keypadPrefab, miniGameCanvas.transform, false);
        }
        else if (!keypadInstance.gameObject.activeInHierarchy)
        {
            // Instance exists but is inactive - destroy it and create a new one
            Destroy(keypadInstance.gameObject);
            keypadInstance = Instantiate(keypadPrefab, miniGameCanvas.transform, false);
        }

        // Ensure the keypad instance is active before setting it up
        if (keypadInstance != null && !keypadInstance.gameObject.activeSelf)
        {
            keypadInstance.gameObject.SetActive(true);
        }

        // Tell keypad where to return when Hide() is called
        keypadInstance.SetVendingPopup(vendingPopup);

        // Tell keypad about this controller so it can change the sprite when code is correct
        keypadInstance.SetVendingPopupController(this);

        // Link wire game reference to keypad so it knows if wires are connected
        // Wire game is inside the screw panel, so we need to ensure the screw panel exists (even if hidden)
        if (screwPanelInstance == null && screwPanelPrefab != null)
        {
            // Instantiate the screw panel (but keep it hidden) so we can get the wire game reference
            screwPanelInstance = Instantiate(screwPanelPrefab, miniGameCanvas.transform, false);
            screwPanelInstance.gameObject.SetActive(false); // Keep it hidden
            screwPanelInstance.SetVendingPopup(vendingPopup);
            Debug.Log("[VendingPopupInteractable] Instantiated screw panel (hidden) to get wire game reference");
        }

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
        else
        {
            Debug.LogWarning("[VendingPopupInteractable] Screw panel prefab not assigned - cannot link wire game to keypad");
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
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

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

        // Unpause the game when popup closes
        PauseController.SetPause(false);

        Debug.Log("[VendingPopupInteractable] Closed all UI");
    }

    // Called when the keypad code is correct - closes everything and drops bread
    public void OnVendingMachineEmpty()
    {
        Debug.Log("[VendingPopupInteractable] Keypad completed - closing UI and dropping bread");
        
        // Mark that bread has been collected
        breadCollected = true;

<<<<<<< Updated upstream
=======
        // Play success before UI is hidden so audio source remains active
        EnsureKeypadAudioManager();
        if (keypadAudioManager != null)
        {
            Vector3 playPos = Camera.main ? Camera.main.transform.position : transform.position;
            keypadAudioManager.PlaySuccessAtPosition(playPos);
        }
        else
        {
            Debug.LogWarning("[VendingPopupInteractable] KeyPadAudioManager not found, cannot play success sound");
        }
        
>>>>>>> Stashed changes
        // Close all UI immediately
        CloseAll();
        
        // Drop bread from the vending machine
        DropBread();
    }
    
    /// <summary>
    /// Drops bread item from the vending machine position
    /// </summary>
    private void DropBread()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("[VendingPopupInteractable] itemPrefab is NULL! Assign Item.prefab in Inspector!");
            return;
        }
        
        if (string.IsNullOrEmpty(breadItemName))
        {
            Debug.LogError("[VendingPopupInteractable] breadItemName is not set! Set it in Inspector!");
            return;
        }
        
        // Find the Bread ItemSO
        ItemSO breadItemSO = null;
        
        if (PlayerInventory.instance != null)
        {
            // Check consumableSOs
            if (PlayerInventory.instance.consumableSOs != null)
            {
                foreach (var so in PlayerInventory.instance.consumableSOs)
                {
                    if (so != null && so.itemName == breadItemName)
                    {
                        breadItemSO = so;
                        break;
                    }
                }
            }
            
            // Check equipmentSOs
            if (breadItemSO == null && PlayerInventory.instance.equipmentSOs != null)
            {
                foreach (var so in PlayerInventory.instance.equipmentSOs)
                {
                    if (so != null && so.itemName == breadItemName)
                    {
                        breadItemSO = so;
                        break;
                    }
                }
            }
            
            // Check itemSOs
            if (breadItemSO == null && PlayerInventory.instance.itemSOs != null)
            {
                foreach (var so in PlayerInventory.instance.itemSOs)
                {
                    if (so != null && so.itemName == breadItemName)
                    {
                        breadItemSO = so;
                        break;
                    }
                }
            }
        }
        
        if (breadItemSO == null)
        {
            Debug.LogError($"[VendingPopupInteractable] Could not find ItemSO named '{breadItemName}'! Make sure it's in PlayerInventory arrays.");
            return;
        }
        
        // Get drop position (use breadDropPosition if set, otherwise use vending machine position)
        Vector3 dropPos = transform.position;
        if (breadDropPosition != null)
        {
            dropPos = breadDropPosition.position;
        }
        
        // Spawn bread item
        GameObject breadItem = Instantiate(itemPrefab, dropPos, Quaternion.identity);
        Item itemComponent = breadItem.GetComponent<Item>();
        if (itemComponent != null)
        {
            // Initialize with upward velocity so it pops out
            itemComponent.Initialize(new Vector2(0f, 3f), breadItemSO);
            Debug.Log($"[VendingPopupInteractable] ✅ Dropped bread at position: {dropPos}");
        }
        else
        {
            Debug.LogError("[VendingPopupInteractable] Item prefab doesn't have Item component!");
            Destroy(breadItem);
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
