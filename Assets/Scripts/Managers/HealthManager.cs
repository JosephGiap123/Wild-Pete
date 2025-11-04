using UnityEngine;
using System;
public class HealthManager : MonoBehaviour
{
    //singleton instance
    public static HealthManager instance;
    public MonoBehaviour healthBar; // Changed to MonoBehaviour to handle missing HealthBarScript
    private int health;
    private int maxHealth;

    public event Action<int, int> OnHealthChanged; // current, max
    public event Action<int> OnMaxHealthChanged;
    public event Action OnPlayerDeath;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
    }

    private void HandlePlayerSet(GameObject player)
    {
        maxHealth = player.GetComponent<BasePlayerMovement2D>().maxHealth;
        health = maxHealth;
        if (healthBar != null)
        {
            // Try to call methods if HealthBarScript exists
            var method = healthBar.GetType().GetMethod("SetMaxHealth");
            if (method != null) method.Invoke(healthBar, new object[] { maxHealth });
            method = healthBar.GetType().GetMethod("SetHealth");
            if (method != null) method.Invoke(healthBar, new object[] { health });
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            OnPlayerDeath?.Invoke();
        }
        OnHealthChanged?.Invoke(health, maxHealth);
        if (healthBar != null)
        {
            var method = healthBar.GetType().GetMethod("UpdateHealthBar");
            if (method != null) method.Invoke(healthBar, new object[] { health, maxHealth });
        }
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        OnMaxHealthChanged?.Invoke(maxHealth);
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
        OnHealthChanged?.Invoke(health, maxHealth);
        if (healthBar != null)
        {
            var method = healthBar.GetType().GetMethod("UpdateHealthBar");
            if (method != null) method.Invoke(healthBar, new object[] { health, maxHealth });
        }
    }

    public void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);
        if (healthBar != null)
        {
            var method = healthBar.GetType().GetMethod("UpdateHealthBar");
            if (method != null) method.Invoke(healthBar, new object[] { health, maxHealth });
        }
    }

    public void ResetHealth()
    {
        health = maxHealth;
        OnHealthChanged?.Invoke(health, maxHealth);
        if (healthBar != null)
        {
            var method = healthBar.GetType().GetMethod("UpdateHealthBar");
            if (method != null) method.Invoke(healthBar, new object[] { health, maxHealth });
        }
    }

    public virtual float GetHealthPercentage()
    {
        return (float)health / maxHealth;
    }
    public virtual bool IsHealthFull()
    {
        return health >= maxHealth;
    }

    protected virtual void ChangeHealth(int change)
    {
        health += change;
    }

}
