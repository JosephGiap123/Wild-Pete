using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Info")]
    [SerializeField] protected float maxHealth = 10f;
    protected float health;

    [Header("References")]
    // Add common references here if needed
    // [SerializeField] protected Animator animator;
    [SerializeField] protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        health = maxHealth;
    }
    public virtual void Hurt(float dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // Override in derived classes for death animations, drops, etc.
        Destroy(gameObject);
    }

    public float GetHealthPercentage()
    {
        return health / maxHealth;
    }

    public bool IsAlive()
    {
        return health > 0;
    }
}