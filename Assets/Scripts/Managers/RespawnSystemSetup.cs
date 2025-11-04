using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Automatically sets up the respawn system when the game starts.
/// Creates RespawnManager and Death UI Canvas with all necessary components.
/// </summary>
public class RespawnSystemSetup : MonoBehaviour
{
    [Header("Auto-Setup Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createRespawnManager = true;
    [SerializeField] private bool createDeathUI = true;

    [Header("Death UI Settings")]
    [SerializeField] private string deathMessage = "You Died\nDo you want to revive?";
    [SerializeField] private string respawnPrompt = "Press [J] to Revive";
    [SerializeField] private float showDelay = 2f;
    [SerializeField] private Color deathPanelColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color deathTextColor = Color.white;
    [SerializeField] private Color promptTextColor = Color.yellow;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupRespawnSystem();
        }
    }

    public void SetupRespawnSystem()
    {
        if (createRespawnManager)
        {
            SetupRespawnManager();
        }

        if (createDeathUI)
        {
            SetupDeathUI();
        }
    }

    private void SetupRespawnManager()
    {
        // Check if RespawnManager already exists
        if (RespawnManager.Instance != null)
        {
            Debug.Log("RespawnSystemSetup: RespawnManager already exists, skipping creation.");
            return;
        }

        // Create RespawnManager GameObject
        GameObject respawnManagerObj = new GameObject("RespawnManager");
        RespawnManager respawnManager = respawnManagerObj.AddComponent<RespawnManager>();
        
        Debug.Log("RespawnSystemSetup: RespawnManager created successfully.");
    }

    private void SetupDeathUI()
    {
        // Check if Death UI already exists
        GameObject existingCanvas = GameObject.Find("DeathRespawnCanvas");
        if (existingCanvas != null)
        {
            Debug.Log("RespawnSystemSetup: Death UI Canvas already exists, skipping creation.");
            return;
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("DeathRespawnCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // High sorting order to appear on top
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Death Panel
        GameObject panelObj = new GameObject("DeathPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = deathPanelColor;

        // Add DeathRespawnUI component
        DeathRespawnUI deathUI = panelObj.AddComponent<DeathRespawnUI>();

        // Create Death Text
        GameObject deathTextObj = new GameObject("DeathText");
        deathTextObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform deathTextRect = deathTextObj.AddComponent<RectTransform>();
        deathTextRect.anchorMin = new Vector2(0.5f, 0.6f);
        deathTextRect.anchorMax = new Vector2(0.5f, 0.6f);
        deathTextRect.pivot = new Vector2(0.5f, 0.5f);
        deathTextRect.sizeDelta = new Vector2(800, 200);
        deathTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI deathText = deathTextObj.AddComponent<TextMeshProUGUI>();
        deathText.text = deathMessage;
        deathText.color = deathTextColor;
        deathText.fontSize = 72;
        deathText.alignment = TextAlignmentOptions.Center;
        deathText.fontStyle = FontStyles.Bold;

        // Create Respawn Prompt Text
        GameObject promptTextObj = new GameObject("RespawnPromptText");
        promptTextObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform promptTextRect = promptTextObj.AddComponent<RectTransform>();
        promptTextRect.anchorMin = new Vector2(0.5f, 0.4f);
        promptTextRect.anchorMax = new Vector2(0.5f, 0.4f);
        promptTextRect.pivot = new Vector2(0.5f, 0.5f);
        promptTextRect.sizeDelta = new Vector2(800, 150);
        promptTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI promptText = promptTextObj.AddComponent<TextMeshProUGUI>();
        promptText.text = respawnPrompt;
        promptText.color = promptTextColor;
        promptText.fontSize = 48;
        promptText.alignment = TextAlignmentOptions.Center;

        // Create Respawn Button
        GameObject buttonObj = new GameObject("RespawnButton");
        buttonObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.25f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.25f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(300, 80);
        buttonRect.anchoredPosition = Vector2.zero;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green button

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        // Create Button Text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Revive (J)";
        buttonText.color = Color.white;
        buttonText.fontSize = 36;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;

        // Set UI references using public method
        deathUI.SetUIReferences(panelObj, button, deathText, promptText);
        
        // Use reflection to set private fields for settings
        SetPrivateField(deathUI, "deathMessage", deathMessage);
        SetPrivateField(deathUI, "respawnPrompt", respawnPrompt);
        SetPrivateField(deathUI, "showDelay", showDelay);

        Debug.Log("RespawnSystemSetup: Death UI created successfully with all components.");
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }
}

