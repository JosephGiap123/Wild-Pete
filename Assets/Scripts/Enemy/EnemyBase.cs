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

    protected virtual void Awake()
    {
        health = maxHealth;
    }
    
    public virtual void Hurt(int dmg)
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
}