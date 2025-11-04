using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    protected int maxHealth = 10;
    protected int health;
    protected SpriteRenderer sr;
    protected GameObject damageText;

    protected virtual void Awake()
    {
        health = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.material = new Material(sr.sharedMaterial);
    }

    public virtual void EndHurtState() { }

    public virtual void Hurt(int dmg, Vector2 knockbackForce)
    {
        health -= dmg;
        if (health <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public virtual IEnumerator DamageFlash(float duration)
    {
        if (sr != null && sr.material != null)
        {
            sr.material.SetFloat("_FlashAmount", 1f);
            yield return new WaitForSeconds(duration);
            sr.material.SetFloat("_FlashAmount", 0f);
        }
    }
}

