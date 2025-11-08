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
    [SerializeField] protected GameObject damageText;
    [SerializeField] protected DropItemsOnDeath dropItemsOnDeath;
    public AttackHitboxInfo[] attackHitboxes;
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
    public virtual int GetHealth()
    {
        return health;
    }

    public virtual int GetMaxHealth()
    {
        return maxHealth;
    }
}