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
        [Header("Audio")]
    [SerializeField] protected AudioClip hitSound;
    [SerializeField, Range(0f, 1f)] protected float hitVolume = 1f;
    [SerializeField] protected AudioClip breakSound;
    [SerializeField] protected AudioSource sfxSource;
    [SerializeField, Range(0f, 1f)] protected float breakVolume = 1f;

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
        PlaySound(hitSound, hitVolume);
        StartCoroutine(DamageFlash(0.2f));
        if (health <= 0)
        {
            //run some code
            Break();
        }
    }

    protected virtual void Break()
    {
        Destroy(gameObject);
    }

    public virtual void Restore(Vector2 position)
    {
        Restore();
        transform.position = position;
    }

    public virtual void Restore()
    {
        health = Mathf.Max(health, 1);
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public virtual IEnumerator DamageFlash(float duration)
    {
        sr.material.SetFloat("_FlashAmount", 1f);
        yield return new WaitForSeconds(duration);
        sr.material.SetFloat("_FlashAmount", 0f);
    }
    protected void PlayHitSound()
    {
        PlaySound(hitSound, hitVolume);
    }

    private void PlaySound(AudioClip clip, float volume = 1f, bool useDetachedSource = false)
    {
        if (!clip) return;
        volume = Mathf.Clamp01(volume <= 0f ? 1f : volume);

        if (useDetachedSource)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
            return;
        }

        if (!sfxSource)
        {
            sfxSource = GetComponent<AudioSource>();
            if (!sfxSource)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
                sfxSource.spatialBlend = 1f;
            }
        }

        sfxSource.PlayOneShot(clip, volume);
    }
}
