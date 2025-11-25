using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestartManager : MonoBehaviour
{
    public static GameRestartManager Instance { get; private set; }

    BasePlayerMovement2D playerMovementScript;
    public static Vector2 checkPointLocation;
    public static event Action GameRestart;
    public static event Action<Vector2> CharacterRespawned;

    private bool pendingRespawn = false;
    private Vector2? pendingRespawnLocation = null;

    private void Awake()
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
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.OnPlayerSet += SetPlayer;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.OnPlayerSet -= SetPlayer;
        if (playerMovementScript)
        {
            playerMovementScript.PlayerDied -= PlayerDeath;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If there's a pending respawn, execute it after the scene loads
        if (pendingRespawn)
        {
            pendingRespawn = false;
            if (pendingRespawnLocation.HasValue)
            {
                // Wait a frame for player to be spawned, then respawn at checkpoint
                StartCoroutine(RespawnAfterSceneLoad(pendingRespawnLocation.Value));
            }
        }
    }

    private System.Collections.IEnumerator RespawnAfterSceneLoad(Vector2 respawnLocation)
    {
        // Wait for player to be spawned
        yield return null;

        // Now trigger respawn at the checkpoint location
        CharacterRespawned?.Invoke(respawnLocation);
    }

    public bool HasPendingRespawn()
    {
        return pendingRespawn;
    }

    void SetPlayer(GameObject player)
    {
        if (player == null)
        {
            Debug.Log("GameRestartManager: Player is null");
            return;
        }

        // Unsubscribe from old player if it exists
        if (playerMovementScript != null)
        {
            playerMovementScript.PlayerDied -= PlayerDeath;
        }

        // Subscribe to new player
        playerMovementScript = player.GetComponent<BasePlayerMovement2D>();
        if (playerMovementScript != null)
        {
            playerMovementScript.PlayerDied += PlayerDeath;
            Debug.Log($"GameRestartManager: Subscribed to new player's PlayerDied event");
        }
        else
        {
            Debug.LogError("GameRestartManager: Player does not have BasePlayerMovement2D component!");
        }
    }

    void PlayerDeath()
    {
        //pull up menu
        // PauseController.SetPause(true); //pause game
    }

    // Respawns the character at the specified checkpoint location.
    // If no location is provided, uses the last saved checkpoint.
    public void RespawnCharacter(Vector2? checkpointLocation = null)
    {
        // Clean up all active projectiles and lasers before respawning
        CleanupProjectilesAndLasers();

        Vector2 respawnLocation;

        if (checkpointLocation.HasValue)
        {
            respawnLocation = checkpointLocation.Value;
        }
        else if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint())
        {
            respawnLocation = CheckpointManager.Instance.GetCheckpointPosition();
        }
        else
        {
            // Fallback to spawn point if no checkpoint
            respawnLocation = GameManager.Instance != null ? GameManager.Instance.transform.position : Vector2.zero;
            Debug.LogWarning("No checkpoint saved! Respawning at spawn point.");
        }

        // Check if we're currently loading a scene - if so, queue the respawn
        if (SceneManager.GetActiveScene().name == "Menu" || GameManager.Instance == null || GameManager.Instance.player == null)
        {
            // Scene might be loading or player doesn't exist yet - queue respawn
            pendingRespawn = true;
            pendingRespawnLocation = respawnLocation;
            Debug.Log($"GameRestartManager: Queuing respawn at {respawnLocation} for after scene load");
        }
        else
        {
            // Player exists, respawn immediately
            // Find the current player and call OnRespawn directly instead of using event
            // This ensures we're respawning the current player, not a destroyed one
            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                BasePlayerMovement2D currentPlayer = GameManager.Instance.player.GetComponent<BasePlayerMovement2D>();
                if (currentPlayer != null)
                {
                    currentPlayer.OnRespawn(respawnLocation);
                    // Also invoke the event so UI elements (like death screen) can respond
                    CharacterRespawned?.Invoke(respawnLocation);
                }
                else
                {
                    Debug.LogError("GameRestartManager: Current player doesn't have BasePlayerMovement2D component!");
                }
            }
            else
            {
                Debug.LogWarning("GameRestartManager: Player doesn't exist, cannot respawn");
            }
        }
    }

    // Destroys all active projectiles and lasers in the scene.
    private void CleanupProjectilesAndLasers()
    {
        // Destroy all enemy projectiles
        GuardBullet[] guardBullets = FindObjectsByType<GuardBullet>(FindObjectsSortMode.None);
        foreach (GuardBullet bullet in guardBullets)
        {
            if (bullet != null && bullet.gameObject != null)
            {
                Destroy(bullet.gameObject);
            }
        }

        // Destroy all player projectiles
        Bullet[] bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        foreach (Bullet bullet in bullets)
        {
            if (bullet != null && bullet.gameObject != null)
            {
                Destroy(bullet.gameObject);
            }
        }

        ShotgunBullet[] shotgunBullets = FindObjectsByType<ShotgunBullet>(FindObjectsSortMode.None);
        foreach (ShotgunBullet bullet in shotgunBullets)
        {
            if (bullet != null && bullet.gameObject != null)
            {
                Destroy(bullet.gameObject);
            }
        }

        // Destroy all dynamite
        Dynamite[] dynamites = FindObjectsByType<Dynamite>(FindObjectsSortMode.None);
        foreach (Dynamite dynamite in dynamites)
        {
            if (dynamite != null && dynamite.gameObject != null)
            {
                Destroy(dynamite.gameObject);
            }
        }

        // Destroy all landmines
        Landmine[] landmines = FindObjectsByType<Landmine>(FindObjectsSortMode.None);
        foreach (Landmine landmine in landmines)
        {
            if (landmine != null && landmine.gameObject != null)
            {
                Destroy(landmine.gameObject);
            }
        }

        // Destroy all nukes
        Nuke[] nukes = FindObjectsByType<Nuke>(FindObjectsSortMode.None);
        foreach (Nuke nuke in nukes)
        {
            if (nuke != null && nuke.gameObject != null)
            {
                Destroy(nuke.gameObject);
            }
        }

        // Destroy all rockets
        RPGRocket[] rockets = FindObjectsByType<RPGRocket>(FindObjectsSortMode.None);
        foreach (RPGRocket rocket in rockets)
        {
            if (rocket != null && rocket.gameObject != null)
            {
                Destroy(rocket.gameObject);
            }
        }

        // Destroy all lasers
        GroundLaserBeam[] lasers = FindObjectsByType<GroundLaserBeam>(FindObjectsSortMode.None);
        foreach (GroundLaserBeam laser in lasers)
        {
            if (laser != null && laser.gameObject != null)
            {
                Destroy(laser.gameObject);
            }
        }
    }
}
