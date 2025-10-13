using System.Collections;
using UnityEngine;

public class DoorTeleporter : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    [SerializeField] private DoorTeleporter linkedDoor; // The door to teleport to
    [SerializeField] private Transform exitPoint; // Where player spawns (optional)
    [SerializeField] private float exitOffset = 1f; // Distance from door when teleporting
    [SerializeField] private bool faceRightOnExit = true; // Which way player faces after teleport

    private BasePlayerMovement2D playerMovement;
    
    [Header("Cooldown Settings")]
    [SerializeField] private float teleportCooldown = 0.5f; // Prevent immediate re-teleport
    private float lastTeleportTime = -999f;
    
    // [Header("Optional Effects")]
    // [SerializeField] private AudioClip teleportSound;
    // [SerializeField] private GameObject teleportEffect;
    

    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
    }

    private void OnDisable(){
        GameManager.OnPlayerSet -= HandlePlayerSet;
    }

    private void HandlePlayerSet(GameObject player)  {
        playerMovement = player.GetComponent<BasePlayerMovement2D>();
        if (playerMovement == null)
        {
            Debug.LogError("HealthBarScript: Player missing BasePlayerMovement2D component!");
            return;
        }
    }

    public bool CanInteract()
    {
        // Can't interact if no linked door or still on cooldown
        if (linkedDoor == null)
        {
            Debug.LogWarning($"{gameObject.name}: No linked door assigned!");
            return false;
        }
        
        return Time.time >= lastTeleportTime + teleportCooldown;
    }
    
    public void Interact()
    {
        if (!CanInteract()) return;
        
        // Find the player
        GameObject player = GameManager.Instance?.player;
        if (player == null)
        {
            Debug.LogError("DoorTeleporter: No player found!");
            return;
        }
        
        Debug.Log($"Teleporting from {gameObject.name} to {linkedDoor.gameObject.name}");
        
        // Teleport the player
        TeleportPlayer(player);
    }
    
    private void TeleportPlayer(GameObject player)
    {
        // Play effects at current location
        // PlayTeleportEffects(transform.position);
        
        // Calculate exit position
        Vector3 exitPosition;
        if (linkedDoor.exitPoint != null)
        {
            // Use the exit point if specified
            exitPosition = linkedDoor.exitPoint.position;
        }
        else
        {
            // Default: spawn in front of the linked door
            float direction = linkedDoor.faceRightOnExit ? 1f : -1f;
            exitPosition = linkedDoor.transform.position + new Vector3(direction * linkedDoor.exitOffset, 0, 0);
        }
        
        // Teleport player
        player.transform.position = exitPosition;
        
        {
            bool needsFlip = (linkedDoor.faceRightOnExit && !playerMovement.isFacingRight) || (!linkedDoor.faceRightOnExit && playerMovement.isFacingRight);
            
            if (needsFlip)
            {
                playerMovement.FlipSprite();
            }
        }
        
        // Play effects at destination
        // PlayTeleportEffects(linkedDoor.transform.position);
        // Set cooldown for both doors to prevent ping-ponging
        lastTeleportTime = Time.time;
        linkedDoor.lastTeleportTime = Time.time;
    }
    
    // private void PlayTeleportEffects(Vector3 position)
    // {
    //     // Spawn particle effect
    //     if (teleportEffect != null)
    //     {
    //         Instantiate(teleportEffect, position, Quaternion.identity);
    //     }
        
    //     // Play sound
    //     if (teleportSound != null)
    //     {
    //         AudioSource.PlayClipAtPoint(teleportSound, position);
    //     }
    // }
    
    private void OnDrawGizmos()
    {
        if (linkedDoor != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, linkedDoor.transform.position);
            
            // Draw exit position
            Vector3 exitPos;
            if (linkedDoor.exitPoint != null)
            {
                exitPos = linkedDoor.exitPoint.position;
            }
            else
            {
                float direction = linkedDoor.faceRightOnExit ? 1f : -1f;
                exitPos = linkedDoor.transform.position + new Vector3(direction * linkedDoor.exitOffset, 0, 0);
            }
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(exitPos, 0.5f);
            Gizmos.DrawLine(linkedDoor.transform.position, exitPos);
        }
    }
}