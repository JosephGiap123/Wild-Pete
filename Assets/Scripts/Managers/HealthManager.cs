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

    private void HandlePlayerSet(GameObject player)
    {
        // Get max health from StatsManager if available, otherwise use player's base maxHealth
        if (StatsManager.instance != null)
        {
            maxHealth = StatsManager.instance.maxHealth;
        }
        else
    {
        maxHealth = player.GetComponent<BasePlayerMovement2D>().maxHealth;
        }
        
        health = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(health);
        
        // Subscribe to stat changes for max health updates
        if (StatsManager.instance != null)
        {
            StatsManager.instance.OnStatChanged += HandleMaxHealthChanged;
        }
    }
    
    private void HandleMaxHealthChanged(EquipmentSO.Stats stat, float value)
    {
        if (stat == EquipmentSO.Stats.MaxHealth && StatsManager.instance != null)
        {
            int newMaxHealth = StatsManager.instance.maxHealth;
            
            // If current health exceeds new max, cap it before updating
            if (health > newMaxHealth)
            {
                health = newMaxHealth;
            }
            
            // Update max health (this will also update the UI)
            SetMaxHealth(newMaxHealth);
        }
    }
    
    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
        
        // Unsubscribe from stat changes
        if (StatsManager.instance != null)
        {
            StatsManager.instance.OnStatChanged -= HandleMaxHealthChanged;
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
        
        // Update health bar UI
        if (healthBar != null)
        {
            healthBar.UpdateMaxHealth(maxHealth);
            // Also update current health display to reflect new max
            healthBar.UpdateHealthBar(health, maxHealth);
        }
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
    public virtual bool IsHealthFull()
    {
        return health >= maxHealth;
    }

    protected virtual void ChangeHealth(int change)
    {
        health += change;
    }

    // Gets the current health value.
    public int GetCurrentHealth()
    {
        return health;
    }

    // Sets the current health value (for respawn/checkpoint system).
    public void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);
        healthBar.UpdateHealthBar(health, maxHealth);
    }

}
