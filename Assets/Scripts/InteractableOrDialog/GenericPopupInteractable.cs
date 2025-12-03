using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// A generic popup system that can be attached to any GameObject.
/// When the player interacts with this object, it opens a popup with a configurable sprite/image.
/// </summary>
public class GenericPopupInteractable : MonoBehaviour, IInteractable
{
    [Header("World Object")]
    [SerializeField] private Sprite worldSprite; // The sprite to show in the world (auto-assigned to SpriteRenderer)
    [SerializeField] private SpriteRenderer worldSpriteRenderer; // The sprite renderer for the object in the world (auto-finds if not assigned)

    [Header("Popup Settings")]
    [SerializeField] private Sprite popupSprite; // The sprite/image to show in the popup
    [SerializeField] private string interactionMessage = " to view"; // Message shown when player can interact
    [SerializeField] private bool pauseGameOnOpen = true; // Should the game pause when popup opens?

    [Header("Exit Hint Text")]
    [SerializeField] private string exitHintText = "Click anywhere to exit."; // Hint text shown under the popup
    [SerializeField] private Font exitHintFont; // Optional custom font (falls back to Arial if null)
    [SerializeField] private int exitHintFontSize = 18; // Font size for the exit hint text

    [Header("Popup Size (Optional)")]
    [SerializeField] private Vector2 popupSize = new Vector2(0f, 0f); // Size of the popup (0,0 = use sprite native size - default for 480x270 canvas)

    [Header("Canvas (Optional - Auto-finds if not assigned)")]
    [SerializeField] private Canvas uiCanvas; // The UI Canvas to spawn popup under (auto-finds "UI" if not assigned)

    // Private fields
    private GameObject popupInstance; // The current popup instance
    private Image popupImage; // The image component on the popup
    private GameObject overlayInstance; // The click-outside overlay
    private Coroutine clickDetectionCoroutine; // Coroutine to detect clicks

    private void Awake()
    {
        EnsureSpriteRenderer();
        UpdateWorldSprite();
        FindUICanvas();
    }

    private void Start()
    {
        // Force update in Start as well (runs after Awake)
        EnsureSpriteRenderer();
        UpdateWorldSprite();
    }

    // This runs in the Editor when values change, so sprite updates immediately
    private void OnValidate()
    {
        EnsureSpriteRenderer();
        UpdateWorldSprite();

        // Check if BoxCollider2D is set up correctly
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            if (!col.isTrigger)
            {
                Debug.LogWarning($"[GenericPopupInteractable] ⚠️ BoxCollider2D on {gameObject.name} is NOT set as Trigger! Interaction won't work. Set 'Is Trigger' to TRUE.");
            }
        }
        else
        {
            Debug.LogWarning($"[GenericPopupInteractable] ⚠️ {gameObject.name} is missing BoxCollider2D! Interaction won't work. Add a BoxCollider2D with Is Trigger = true.");
        }
    }

    private void EnsureSpriteRenderer()
    {
        // Auto-find sprite renderer if not assigned
        if (worldSpriteRenderer == null)
        {
            worldSpriteRenderer = GetComponent<SpriteRenderer>();

            // If SpriteRenderer doesn't exist, create it
            if (worldSpriteRenderer == null)
            {
                worldSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
    }

    private void UpdateWorldSprite()
    {
        // Auto-assign world sprite to SpriteRenderer if set
        if (worldSprite != null && worldSpriteRenderer != null)
        {
            // Force assignment
            worldSpriteRenderer.sprite = worldSprite;

            // Force enable
            worldSpriteRenderer.enabled = true;

            // Force color to white/opaque
            worldSpriteRenderer.color = Color.white;

            // Force material if needed
            if (worldSpriteRenderer.sharedMaterial == null)
            {
                // Try to find default sprite material
                Material defaultSpriteMat = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
                if (defaultSpriteMat != null)
                {
                    worldSpriteRenderer.sharedMaterial = defaultSpriteMat;
                }
            }

            // Log detailed info
            Debug.Log($"[GenericPopupInteractable] ✅ FORCED sprite '{worldSprite.name}' assignment.");
            Debug.Log($"  - Enabled: {worldSpriteRenderer.enabled}");
            Debug.Log($"  - Color: {worldSpriteRenderer.color}");
            Debug.Log($"  - Material: {worldSpriteRenderer.sharedMaterial?.name ?? "NULL"}");
            Debug.Log($"  - Sprite Texture: {worldSprite.texture?.name ?? "NULL"} ({worldSprite.texture?.width ?? 0}x{worldSprite.texture?.height ?? 0})");
            Debug.Log($"  - Sprite PPU: {worldSprite.pixelsPerUnit}");
            Debug.Log($"  - Sorting Layer: {worldSpriteRenderer.sortingLayerName} (ID: {worldSpriteRenderer.sortingLayerID})");
            Debug.Log($"  - Order in Layer: {worldSpriteRenderer.sortingOrder}");
            Debug.Log($"  - Position: {transform.position}");
            Debug.Log($"  - Scale: {transform.localScale}");
            Debug.Log($"  - GameObject Active: {gameObject.activeSelf}");
            Debug.Log($"  - GameObject Active in Hierarchy: {gameObject.activeInHierarchy}");
        }
        else
        {
            if (worldSprite == null)
            {
                Debug.LogWarning($"[GenericPopupInteractable] ❌ worldSprite is NULL on {gameObject.name}");
            }
            if (worldSpriteRenderer == null)
            {
                Debug.LogWarning($"[GenericPopupInteractable] ❌ worldSpriteRenderer is NULL on {gameObject.name}");
            }
        }
    }

    private void FindUICanvas()
    {
        // Auto-find UI Canvas if not assigned
        if (uiCanvas == null)
        {
            GameObject uiObject = GameObject.Find("UI");
            if (uiObject != null)
            {
                uiCanvas = uiObject.GetComponent<Canvas>();
            }
        }
    }

    // --- IInteractable Implementation ---

    public string InteractMessage()
    {
        return interactionMessage;
    }

    public bool CanInteract()
    {
        // Can interact if popup is not already open
        bool canInteract = popupInstance == null;

        // Debug logging
        if (!canInteract)
        {
            Debug.Log($"[GenericPopupInteractable] Cannot interact - popup already open on {gameObject.name}");
        }

        return canInteract;
    }

    public void Interact()
    {
        Debug.Log($"[GenericPopupInteractable] 🔵 Interact() called on {gameObject.name}");

        if (popupInstance != null)
        {
            Debug.LogWarning($"[GenericPopupInteractable] Popup already open for {gameObject.name}");
            return;
        }

        if (popupSprite == null)
        {
            Debug.LogError($"[GenericPopupInteractable] No popup sprite assigned to {gameObject.name}!");
            return;
        }

        if (uiCanvas == null)
        {
            Debug.LogError($"[GenericPopupInteractable] No UI Canvas found! Make sure there's a GameObject named 'UI' with a Canvas component.");
            return;
        }

        Debug.Log($"[GenericPopupInteractable] ✅ Opening popup for {gameObject.name}");
        OpenPopup();
    }

    // --- Popup Management ---

    /// <summary>
    /// Opens the popup with the assigned sprite
    /// </summary>
    private void OpenPopup()
    {
        // Create popup GameObject
        popupInstance = new GameObject("GenericPopup");
        popupInstance.transform.SetParent(uiCanvas.transform, false);

        // Add RectTransform
        RectTransform rectTransform = popupInstance.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        // Add Image component first
        popupImage = popupInstance.AddComponent<Image>();
        popupImage.sprite = popupSprite;
        popupImage.preserveAspect = true;
        popupImage.raycastTarget = false; // Don't block clicks - let them pass through

        // Set size AFTER image is added (so we can use sprite dimensions)
        if (popupSize.x > 0 && popupSize.y > 0)
        {
            rectTransform.sizeDelta = popupSize;
        }
        else
        {
            // Use sprite native size (for 480x270 canvas, this should match what you drew)
            rectTransform.sizeDelta = new Vector2(popupSprite.texture.width, popupSprite.texture.height);
        }

        // Add CanvasGroup for potential fade effects
        CanvasGroup canvasGroup = popupInstance.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false; // Don't block interactions
        canvasGroup.blocksRaycasts = false; // Don't block raycasts - let clicks pass through

        // Create hint text below the popup
        CreateExitHintText(rectTransform);

        // Create full-screen overlay (for visual, but clicks handled by coroutine)
        CreateClickOutsideOverlay();

        // Bring popup to front (so it's visible above overlay)
        popupInstance.transform.SetAsLastSibling();

        // Start click detection coroutine (works even when paused)
        // This will detect ANY mouse click and close the popup
        clickDetectionCoroutine = StartCoroutine(DetectClicks());

        Debug.Log("[GenericPopupInteractable] ✅ Click detection coroutine started - ANY click will close popup");

        // Pause game if enabled
        if (pauseGameOnOpen)
        {
            PauseController.SetPause(true);
        }

        Debug.Log($"[GenericPopupInteractable] Opened popup for {gameObject.name}");
    }

    /// <summary>
    /// Creates a simple UI Text under the popup that shows the exit hint.
    /// </summary>
    private void CreateExitHintText(RectTransform popupRect)
    {
        if (popupRect == null || popupInstance == null) return;

        GameObject textGO = new GameObject("ExitHintText");
        textGO.transform.SetParent(popupInstance.transform, false);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 1f);

        // Place it just below the popup image
        float yOffset = -(popupRect.sizeDelta.y * 0.5f + 20f);
        textRect.anchoredPosition = new Vector2(0f, yOffset);
        textRect.sizeDelta = new Vector2(popupRect.sizeDelta.x, 30f);

        Text uiText = textGO.AddComponent<Text>();
        uiText.text = exitHintText;
        uiText.alignment = TextAnchor.UpperCenter;
        uiText.color = Color.white;
        uiText.raycastTarget = false;

        // Use custom font if assigned, otherwise fall back to built-in Arial
        if (exitHintFont != null)
        {
            uiText.font = exitHintFont;
        }
        else
        {
            uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // Use serialized font size (with a sensible minimum)
        uiText.fontSize = Mathf.Max(8, exitHintFontSize);
    }

    /// <summary>
    /// Coroutine that continuously checks for mouse clicks to close the popup
    /// This works even when the game is paused (Time.timeScale = 0)
    /// SIMPLE VERSION: Just close on ANY click, anywhere
    /// </summary>
    private IEnumerator DetectClicks()
    {
        while (popupInstance != null)
        {
            // Check for mouse click - Input.GetMouseButtonDown works even when paused!
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("[GenericPopupInteractable] ✅ MOUSE CLICKED - Closing popup immediately!");
                ClosePopup();
                yield break; // Exit coroutine
            }

            // Wait for next frame
            yield return null;
        }
    }

    /// <summary>
    /// Creates a full-screen overlay that closes the popup when clicked anywhere
    /// </summary>
    private void CreateClickOutsideOverlay()
    {
        // Create overlay GameObject (full screen, behind popup)
        overlayInstance = new GameObject("ClickOutsideOverlay");
        overlayInstance.transform.SetParent(uiCanvas.transform, false);

        // Add RectTransform (full screen)
        RectTransform overlayRect = overlayInstance.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        // Add Image component (transparent, just for raycasting)
        Image overlayImage = overlayInstance.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0f); // Fully transparent
        overlayImage.raycastTarget = true;

        // Add CanvasGroup to ensure it can receive clicks
        CanvasGroup overlayCanvasGroup = overlayInstance.AddComponent<CanvasGroup>();
        overlayCanvasGroup.alpha = 1f;
        overlayCanvasGroup.interactable = true;
        overlayCanvasGroup.blocksRaycasts = true;
        overlayCanvasGroup.ignoreParentGroups = true; // Don't be affected by parent

        // Add EventTrigger for click detection (works even when game is paused)
        EventTrigger overlayTrigger = overlayInstance.AddComponent<EventTrigger>();
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) =>
        {
            Debug.Log("[GenericPopupInteractable] ✅ Screen clicked - closing popup");
            ClosePopup();
        });
        overlayTrigger.triggers.Add(clickEntry);

        // Also add Button component as backup (works even when paused)
        Button overlayButton = overlayInstance.AddComponent<Button>();
        overlayButton.onClick.AddListener(() =>
        {
            Debug.Log("[GenericPopupInteractable] ✅ Screen clicked (Button) - closing popup");
            ClosePopup();
        });

        // Put overlay BEHIND popup (so popup is on top)
        overlayInstance.transform.SetSiblingIndex(0);

        Debug.Log("[GenericPopupInteractable] ✅ Created full-screen click overlay");
    }

    /// <summary>
    /// Closes the popup
    /// </summary>
    public void ClosePopup()
    {
        if (popupInstance == null) return;

        // Stop click detection coroutine
        if (clickDetectionCoroutine != null)
        {
            StopCoroutine(clickDetectionCoroutine);
            clickDetectionCoroutine = null;
        }

        // Unpause game if it was paused
        if (pauseGameOnOpen)
        {
            PauseController.SetPause(false);
        }

        // Destroy overlay
        if (overlayInstance != null)
        {
            Destroy(overlayInstance);
            overlayInstance = null;
        }

        // Destroy popup
        Destroy(popupInstance);
        popupInstance = null;
        popupImage = null;

        Debug.Log($"[GenericPopupInteractable] Closed popup for {gameObject.name}");
    }

    private void OnDestroy()
    {
        // Clean up popup if object is destroyed
        if (popupInstance != null)
        {
            ClosePopup();
        }
    }
}

