using System;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Respawn Settings")]
    [SerializeField] private bool respawnAtDeathLocation = true;

    private Vector3 deathPosition;
    private bool isDead = false;
    private BasePlayerMovement2D playerMovement;

    public event Action OnPlayerDeath;
    public event Action OnPlayerRespawn;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        HealthManager.instance.OnPlayerDeath += HandlePlayerDeath;
        GameManager.OnPlayerSet += HandlePlayerSet;
    }

    private void OnDisable()
    {
        if (HealthManager.instance != null)
            HealthManager.instance.OnPlayerDeath -= HandlePlayerDeath;
        GameManager.OnPlayerSet -= HandlePlayerSet;
    }

    private void HandlePlayerSet(GameObject player)
    {
        playerMovement = player.GetComponent<BasePlayerMovement2D>();
    }

    private void HandlePlayerDeath()
    {
        Debug.Log("RespawnManager: HandlePlayerDeath called!");
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            // Save death position
            deathPosition = GameManager.Instance.player.transform.position;
            isDead = true;
            Debug.Log($"RespawnManager: Death position saved: {deathPosition}, isDead: {isDead}");
            Debug.Log($"RespawnManager: OnPlayerDeath event has {OnPlayerDeath?.GetInvocationList().Length ?? 0} subscribers");
            OnPlayerDeath?.Invoke();
        }
        else
        {
            Debug.LogError("RespawnManager: GameManager.Instance or player is null!");
        }
    }

    public void RespawnPlayer()
    {
        if (!isDead || GameManager.Instance.player == null)
        {
            Debug.LogWarning("RespawnManager: Cannot respawn - player is not dead or doesn't exist");
            return;
        }

        GameObject player = GameManager.Instance.player;
        BasePlayerMovement2D movement = player.GetComponent<BasePlayerMovement2D>();

        if (movement == null)
        {
            Debug.LogError("RespawnManager: Player does not have BasePlayerMovement2D component!");
            return;
        }

        // Respawn at death location
        Vector3 respawnPosition = respawnAtDeathLocation ? deathPosition : player.transform.position;

        // Ensure player is active first
        player.SetActive(true);

        // Reset health FIRST before resetting player state
        if (HealthManager.instance != null && movement != null)
        {
            // Directly set health to max (not heal, in case health is 0)
            HealthManager.instance.ResetHealth();
            Debug.Log($"RespawnManager: Health reset to {movement.maxHealth}");
        }

        // Reset player state (this resets all movement flags, coroutines, etc.)
        // This must happen AFTER health is reset
        movement.Respawn();

        // Set position after respawn
        player.transform.position = respawnPosition;

        // Reset flags
        isDead = false;

        OnPlayerRespawn?.Invoke();
        Debug.Log($"RespawnManager: Player respawned at {respawnPosition}");
    }

    public bool IsPlayerDead()
    {
        return isDead;
    }

    public Vector3 GetDeathPosition()
    {
        return deathPosition;
    }
}

