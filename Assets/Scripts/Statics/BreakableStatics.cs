using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableStatics : MonoBehaviour, IHasFacing
{

    [Header("Static Info")]
    [SerializeField] protected int health = 10;
    public bool IsFacingRight => transform.lossyScale.x > 0; // IHasFacing implementation (statics use transform scale)

    [Header("References")]
    // [SerializeField] protected Animator animator;
    protected Rigidbody2D rb;

    [SerializeField] protected SpriteRenderer sr;
    
    [SerializeField] protected int maxHealth = 10; // Store max health for respawn

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        sr.material = new Material(sr.sharedMaterial); // duplicate the base material
        
        // Store max health
        maxHealth = health;
        
        // Register with CheckpointManager (uses GameObject instance ID automatically)
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RegisterStatic(this);
        }
    }
    
    protected virtual void OnDestroy()
    {
        // Unregister from CheckpointManager
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.UnregisterStatic(this);
        }
    }

    public virtual void Damage(int dmg, Vector2 knockbackForce)
    {
        health -= dmg;
        Debug.Log(health);
        StartCoroutine(DamageFlash(0.2f));
        if (health <= 0)
        {
            //run some code
            Break();
        }
    }

    protected virtual void Break()
    {
        // Disable instead of destroy to allow respawn system to restore it
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Restores the static to its original state at the specified position (for respawn system).
    /// </summary>
    /// <param name="position">Position to restore at. If Vector2.zero, keeps current position.</param>
    public virtual void Restore(Vector2? position = null)
    {
        health = maxHealth;
        gameObject.SetActive(true);
        
        // Restore position if provided
        if (position.HasValue && position.Value != Vector2.zero)
        {
            transform.position = position.Value;
        }
        
        // Reset physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public virtual IEnumerator DamageFlash(float duration)
    {
        sr.material.SetFloat("_FlashAmount", 1f);
        yield return new WaitForSeconds(duration);
        sr.material.SetFloat("_FlashAmount", 0f);
    }
}