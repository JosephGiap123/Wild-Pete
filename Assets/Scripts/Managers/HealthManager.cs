using UnityEngine;
using System;
public class HealthManager : MonoBehaviour
{
    //singleton instance
    public static HealthManager instance;
    public HealthBarScript healthBar;
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
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(health);
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
        healthBar.UpdateHealthBar(health, maxHealth);
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
        healthBar.UpdateHealthBar(health, maxHealth);
    }

    public virtual float GetHealthPercentage()
    {
        return (float)health / maxHealth;
    }

    protected virtual void ChangeHealth(int change)
    {
        health += change;
    }

}
