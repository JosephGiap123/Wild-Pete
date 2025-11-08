using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IHasFacing
{

    [Header("Enemy Info")]
    [SerializeField] protected int maxHealth = 10;
    protected int health;

    [Header("References")]
    // [SerializeField] protected Animator animator;
    [SerializeField] protected Rigidbody2D rb;

    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected GameObject damageText;
    [SerializeField] protected DropItemsOnDeath dropItemsOnDeath;
    public AttackHitboxInfo[] attackHitboxes;
    private Vector2 spawnPoint;

    [Header("Facing")]
    public bool isFacingRight = true;
    public bool IsFacingRight => isFacingRight; // IHasFacing implementation
    
    protected virtual void Awake()
    {
        health = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();
        sr.material = new Material(sr.sharedMaterial); // duplicate the base material
        spawnPoint = this.transform.position;
        
        // Register with CheckpointManager (uses GameObject instance ID automatically)
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RegisterEnemy(this);
        }
    }
    
    protected virtual void OnDestroy()
    {
        // Unregister from CheckpointManager
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.UnregisterEnemy(this);
        }
    }

    public virtual void EndHurtState()
    {
        return;
    }

    /// <summary>
    /// Respawns the enemy at the specified position (or spawn point if not provided) with full health.
    /// Called by checkpoint system to restore enemies that were alive at checkpoint time.
    /// </summary>
    /// <param name="position">Position to respawn at. If Vector2.zero, uses spawn point.</param>
    /// <param name="facingRight">Facing direction to restore. If null, keeps current facing.</param>
    public virtual void Respawn(Vector2? position = null, bool? facingRight = null)
    {
        Vector2 respawnPosition = position.HasValue && position.Value != Vector2.zero 
            ? position.Value 
            : spawnPoint;
        this.transform.position = respawnPosition;
        this.health = maxHealth;
        
        // Restore facing direction if provided
        if (facingRight.HasValue)
        {
            // Set facing direction directly (only flip if it doesn't match)
            if (isFacingRight != facingRight.Value)
            {
                // Flip the sprite to match the saved facing direction
                Vector3 scale = transform.localScale;
                scale.x = facingRight.Value ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
                isFacingRight = facingRight.Value;
            }
        }
        
        this.gameObject.SetActive(true);
    }
    public virtual void Hurt(int dmg, Vector2 knockbackForce)
    {
        health -= dmg;
        Debug.Log(health);
        if (damageText != null)
        {
            GameObject dmgText = Instantiate(damageText, transform.position, transform.rotation);
            dmgText.GetComponentInChildren<DamageText>().Initialize(new(knockbackForce.x, 5f), dmg, new Color(0.8862745f, 0.3660145f, 0.0980392f, 1f), Color.red);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        this.gameObject.SetActive(false);
    }

    public float GetHealthPercentage()
    {
        return (float)health / maxHealth;
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public virtual IEnumerator DamageFlash(float duration)
    {
        sr.material.SetFloat("_FlashAmount", 1f);
        yield return new WaitForSeconds(duration);
        sr.material.SetFloat("_FlashAmount", 0f);
    }
    public virtual int GetHealth()
    {
        return health;
    }

    public virtual int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Flips the sprite horizontally by inverting the X scale.
    /// Override this method to add additional flip logic (e.g., rotating projectile spawn points).
    /// </summary>
    public virtual void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

}