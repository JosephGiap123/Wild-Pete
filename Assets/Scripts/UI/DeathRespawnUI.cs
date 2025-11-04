using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathRespawnUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Button respawnButton;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private TextMeshProUGUI respawnPromptText;

    [Header("Settings")]
    [SerializeField] private string deathMessage = "You Died\nDo you want to revive?";
    [SerializeField] private string respawnPrompt = "Press [J] to Revive";
    [SerializeField] private float showDelay = 2f;

    private bool canRespawn = false;

    void Awake()
    {
        InitializeUI();
    }

    void Start()
    {
        SubscribeToEvents();
    }

    public void InitializeUI()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (respawnButton != null)
        {
            respawnButton.onClick.RemoveAllListeners();
            respawnButton.onClick.AddListener(RespawnPlayer);
            respawnButton.gameObject.SetActive(false);
        }

        if (deathText != null)
            deathText.text = deathMessage;

        if (respawnPromptText != null)
        {
            respawnPromptText.text = respawnPrompt;
            respawnPromptText.gameObject.SetActive(false);
        }
    }

    public void SetUIReferences(GameObject panel, Button button, TextMeshProUGUI deathTxt, TextMeshProUGUI promptTxt)
    {
        deathPanel = panel;
        respawnButton = button;
        deathText = deathTxt;
        respawnPromptText = promptTxt;
        InitializeUI();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnPlayerDeath += ShowDeathScreen;
            RespawnManager.Instance.OnPlayerRespawn += HideDeathScreen;
        }
        else
        {
            StartCoroutine(RetrySubscribeToEvents());
        }
    }

    private System.Collections.IEnumerator RetrySubscribeToEvents()
    {
        yield return new WaitForSeconds(0.1f);
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnPlayerDeath += ShowDeathScreen;
            RespawnManager.Instance.OnPlayerRespawn += HideDeathScreen;
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnPlayerDeath -= ShowDeathScreen;
            RespawnManager.Instance.OnPlayerRespawn -= HideDeathScreen;
        }
    }

    void Update()
    {
        if (canRespawn && Input.GetKeyDown(KeyCode.J))
        {
            RespawnPlayer();
        }
    }

    private void ShowDeathScreen()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);

        if (deathText != null)
            deathText.gameObject.SetActive(true);

        StartCoroutine(ShowRespawnOptionAfterDelay());
    }

    private System.Collections.IEnumerator ShowRespawnOptionAfterDelay()
    {
        yield return new WaitForSeconds(showDelay);
        canRespawn = true;

        if (respawnButton != null)
            respawnButton.gameObject.SetActive(true);

        if (respawnPromptText != null)
            respawnPromptText.gameObject.SetActive(true);
    }

    private void HideDeathScreen()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);
        if (respawnButton != null)
            respawnButton.gameObject.SetActive(false);
        if (respawnPromptText != null)
            respawnPromptText.gameObject.SetActive(false);
        if (deathText != null)
            deathText.gameObject.SetActive(false);
        canRespawn = false;
    }

    public void RespawnPlayer()
    {
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.RespawnPlayer();
    }
}
