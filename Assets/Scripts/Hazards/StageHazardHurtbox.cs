using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hurtbox for stage hazards like spikes that deal continuous damage to players standing on them.
/// Uses OnTriggerStay2D to detect players continuously, dealing damage every X seconds.
/// </summary>
public class StageHazardHurtbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Attack hitbox info containing damage, knockback, and layer settings")]
    [SerializeField] private AttackHitboxInfo attackHitboxInfo;
    
    [Header("Damage Timing")]
    [Tooltip("Time in seconds between damage ticks while player is standing on the hazard")]
    [SerializeField] private float damageInterval = 0.5f;
    
    [Header("Knockback Settings")]
    [Tooltip("If true, knockback direction is calculated radially from hazard center. If false, uses fixed knockback direction from AttackHitboxInfo.")]
    [SerializeField] private bool useRadialKnockback = false;
    
    [Header("Optional Settings")]
    [Tooltip("If true, the hazard will only damage the player once when they first enter, then wait for them to leave and re-enter")]
    [SerializeField] private bool damageOnEnterOnly = false;
    
    private float damageTimer = 0f;
    private HashSet<GameObject> playersInTrigger = new HashSet<GameObject>();
    
    void Update()
    {
        if (PauseController.IsGamePaused) return;
        
        // Decrease damage timer
        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if collision is on player layer
        if (attackHitboxInfo == null || ((1 << collision.gameObject.layer) & attackHitboxInfo.player) == 0)
        {
            return;
        }
        
        // Get player component (check parent first, then self)
        GameObject targetRoot = collision.transform.parent != null ? collision.transform.parent.gameObject : collision.gameObject;
        BasePlayerMovement2D player = targetRoot.GetComponent<BasePlayerMovement2D>();
        
        if (player == null)
        {
            return;
        }
        
        // Add player to tracking set
        playersInTrigger.Add(targetRoot);
        
        // If damage on enter only, deal damage immediately
        if (damageOnEnterOnly && damageTimer <= 0f)
        {
            DealDamage(player, targetRoot);
        }
    }
    
    void OnTriggerStay2D(Collider2D collision)
    {
        if (PauseController.IsGamePaused || attackHitboxInfo == null) return;
        
        // Check if collision is on player layer
        if (((1 << collision.gameObject.layer) & attackHitboxInfo.player) == 0)
        {
            return;
        }
        
        // Get player component (check parent first, then self)
        GameObject targetRoot = collision.transform.parent != null ? collision.transform.parent.gameObject : collision.gameObject;
        BasePlayerMovement2D player = targetRoot.GetComponent<BasePlayerMovement2D>();
        
        if (player == null)
        {
            return;
        }
        
        // Ensure player is in tracking set
        playersInTrigger.Add(targetRoot);
        
        // Skip if damage on enter only (already handled in OnTriggerEnter2D)
        if (damageOnEnterOnly)
        {
            return;
        }
        
        // Deal damage if cooldown is ready
        if (damageTimer <= 0f)
        {
            DealDamage(player, targetRoot);
        }
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        // Check if collision is on player layer
        if (attackHitboxInfo == null || ((1 << collision.gameObject.layer) & attackHitboxInfo.player) == 0)
        {
            return;
        }
        
        // Get player component (check parent first, then self)
        GameObject targetRoot = collision.transform.parent != null ? collision.transform.parent.gameObject : collision.gameObject;
        
        // Remove player from tracking set
        playersInTrigger.Remove(targetRoot);
    }
    
    void DealDamage(BasePlayerMovement2D player, GameObject playerObject)
    {
        // Don't damage if player is dead
        if (HealthManager.instance != null && HealthManager.instance.IsDead())
        {
            return;
        }
        
        // Reset damage timer
        damageTimer = damageInterval;
        
        // Calculate knockback
        Vector2 finalKnockback = attackHitboxInfo.knockbackForce;
        Vector2? hitboxCenter = null;
        
        if (useRadialKnockback)
        {
            // Calculate radial knockback from hazard center to player
            Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
            float distance = Vector2.Distance(player.transform.position, transform.position);
            
            // Scale knockback based on distance (closer = stronger)
            float knockbackScale = Mathf.Max(0.5f, 1f - (distance / 5f)); // Scale down if far away
            finalKnockback = new Vector2(
                direction.x * attackHitboxInfo.knockbackForce.x * knockbackScale,
                direction.y * attackHitboxInfo.knockbackForce.y * knockbackScale
            );
            hitboxCenter = transform.position;
        }
        
        // Deal damage to player
        player.HurtPlayer(attackHitboxInfo.damage, finalKnockback, null, hitboxCenter);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw the trigger area in the editor
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Red with transparency
            
            if (col is BoxCollider2D boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(circleCol.offset, circleCol.radius);
            }
        }
    }
}

