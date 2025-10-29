using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{

    [Header("Enemy Info")]
    [SerializeField] protected int maxHealth = 10;
    protected int health;

    [Header("References")]
    // [SerializeField] protected Animator animator;
    [SerializeField] protected Rigidbody2D rb;

    [SerializeField] protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        health = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();
        sr.material = new Material(sr.sharedMaterial); // duplicate the base material
    }

    public virtual void EndHurtState()
    {
        return;
    }

    public virtual void Hurt(int dmg, Vector2 knockbackForce)
    {
        health -= dmg;
        Debug.Log(health);
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
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
}