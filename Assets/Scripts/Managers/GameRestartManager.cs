using System;
using UnityEngine;

public class GameRestartManager : MonoBehaviour
{
    public static GameRestartManager Instance { get; private set; }

    BasePlayerMovement2D playerMovementScript;
    public static Vector2 checkPointLocation;
    public static event Action GameRestart;
    public static event Action<Vector2> CharacterRespawned;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        GameManager.OnPlayerSet += SetPlayer;
    }

    void OnDisable()
    {
        GameManager.OnPlayerSet -= SetPlayer;
        if (playerMovementScript)
        {
            playerMovementScript.PlayerDied -= PlayerDeath;
        }
    }

    void SetPlayer(GameObject player)
    {
        if (player == null)
        {
            Debug.Log("Player is null guh");
            return;
        }
        playerMovementScript = player.GetComponent<BasePlayerMovement2D>();
        playerMovementScript.PlayerDied += PlayerDeath;
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

        CharacterRespawned?.Invoke(respawnLocation);
    }

    // Destroys all active projectiles and lasers in the scene.
    private void CleanupProjectilesAndLasers()
    {
        // Destroy all enemy projectiles
        GuardBullet[] guardBullets = FindObjectsOfType<GuardBullet>();
        foreach (GuardBullet bullet in guardBullets)
        {
            if (bullet != null && bullet.gameObject != null)
            {
                Destroy(bullet.gameObject);
            }
        }

        // Destroy all player projectiles
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet bullet in bullets)
        {
            if (bullet != null && bullet.gameObject != null)
            {
                Destroy(bullet.gameObject);
            }
        }

        ShotgunBullet[] shotgunBullets = FindObjectsOfType<ShotgunBullet>();
        foreach (ShotgunBullet bullet in shotgunBullets)
        {
            if (bullet != null && bullet.gameObject != null)
            {
                Destroy(bullet.gameObject);
            }
        }

        // Destroy all dynamite
        Dynamite[] dynamites = FindObjectsOfType<Dynamite>();
        foreach (Dynamite dynamite in dynamites)
        {
            if (dynamite != null && dynamite.gameObject != null)
            {
                Destroy(dynamite.gameObject);
            }
        }

        // Destroy all lasers
        GroundLaserBeam[] lasers = FindObjectsOfType<GroundLaserBeam>();
        foreach (GroundLaserBeam laser in lasers)
        {
            if (laser != null && laser.gameObject != null)
            {
                Destroy(laser.gameObject);
            }
        }
    }
}
