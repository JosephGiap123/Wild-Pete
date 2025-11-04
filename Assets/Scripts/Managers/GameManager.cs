using System;
using UnityEngine;
using UnityEngine.UI;
// using Unity.Cinemachine; // Commented out if Cinemachine package not available
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject Pete;
    [SerializeField] private GameObject Alice;
    [SerializeField] private MonoBehaviour cinemachineCam; // Changed to MonoBehaviour to handle missing CinemachineCamera
    public int selectedCharacter = 1;
    public GameObject player { get; private set; }

    public static event Action<GameObject> OnPlayerSet;

    void Awake()
    {
        // singleton of death and doom
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); //keep across scene loads
    }

    public void SetPlayer()
    {
        // selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);
        switch (selectedCharacter)
        {
            case 1:
                player = Instantiate(Pete, transform.position, Quaternion.identity);
                break;
            case 2:
                player = Instantiate(Alice, transform.position, Quaternion.identity);
                break;
            default:
                break;
        }
        GameObject camFollowTarget = new GameObject("CameraFollowTarget");
        camFollowTarget.transform.SetParent(player.transform);
        camFollowTarget.transform.localPosition = new Vector3(0, 2f, 0);
        OnPlayerSet?.Invoke(player); // Notify listeners
        if (cinemachineCam != null)
        {
            // Try to set Follow and LookAt if CinemachineCamera exists
            var followProp = cinemachineCam.GetType().GetProperty("Follow");
            if (followProp != null) followProp.SetValue(cinemachineCam, camFollowTarget.transform);
            var lookAtProp = cinemachineCam.GetType().GetProperty("LookAt");
            if (lookAtProp != null) lookAtProp.SetValue(cinemachineCam, camFollowTarget.transform);
        }
    }

    void Start()
    {
        // Auto-setup respawn system
        SetupRespawnSystem();
        
        SetPlayer();
    }

    private void SetupRespawnSystem()
    {
        // Setup RespawnManager if it doesn't exist
        if (RespawnManager.Instance == null)
        {
            GameObject respawnManagerObj = new GameObject("RespawnManager");
            respawnManagerObj.AddComponent<RespawnManager>();
            Debug.Log("GameManager: RespawnManager auto-created.");
        }

        // Setup Death UI if it doesn't exist
        if (GameObject.Find("DeathRespawnCanvas") == null)
        {
            CreateDeathUI();
        }
    }

    private void CreateDeathUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("DeathRespawnCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
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
        panelImage.color = new Color(0, 0, 0, 0.8f);

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
        deathText.text = "You Died\nDo you want to revive?";
        deathText.color = Color.white;
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
        promptText.text = "Press [J] to Revive";
        promptText.color = Color.yellow;
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
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);

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

        // Set UI references
        deathUI.SetUIReferences(panelObj, button, deathText, promptText);

        Debug.Log("GameManager: Death UI auto-created with all components.");
        Debug.Log($"GameManager: RespawnManager.Instance exists: {RespawnManager.Instance != null}");
    }
}
