using UnityEngine;
using System;
public class HealthManager : MonoBehaviour
{
    //singleton instance
    public static HealthManager instance;
    public HealthBarScript healthBar;
    private int health;
    private int maxHealth;

    public int numDeaths = 0;

    public event Action<int, int> OnHealthChanged; // current, max
    public event Action<int> OnMaxHealthChanged;
    public event Action OnPlayerDeath;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            this.numDeaths = 0;
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
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

        // Unsubscribe from stat changes
        if (StatsManager.instance != null)
        {
            StatsManager.instance.OnStatChanged -= HandleMaxHealthChanged;
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Find the health bar in the new scene
        if (healthBar == null || healthBar.gameObject == null)
        {
            HealthBarScript foundBar = FindFirstObjectByType<HealthBarScript>();
            if (foundBar != null)
            {
                healthBar = foundBar;
                Debug.Log($"HealthManager: Found health bar in new scene: {healthBar.name}");

                // Update the health bar with current values
                if (healthBar != null)
                {
                    healthBar.SetMaxHealth(maxHealth);
                    healthBar.SetHealth(health);
                }
            }
            else
            {
                Debug.LogWarning("HealthManager: No health bar found in new scene!");
            }
        }
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

        // Find health bar if not set
        if (healthBar == null || healthBar.gameObject == null)
        {
            healthBar = FindFirstObjectByType<HealthBarScript>();
        }

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(health);
        }

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

    public void TakeDamage(int damage)
    {
        bool wasAlive = health > 0;
        health -= damage;
        if (health <= 0)
        {
            health = 0;
        }
        OnHealthChanged?.Invoke(health, maxHealth);
        healthBar.UpdateHealthBar(health, maxHealth);

        // Fire death event if player was alive and is now dead
        if (wasAlive && health <= 0)
        {
            numDeaths++;
            OnPlayerDeath?.Invoke();
        }
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    // Kills the player by setting health to 0 and firing death event
    // Use this for non-damage deaths (like fall death)
    public void KillPlayer()
    {
        bool wasAlive = health > 0;
        health = 0;
        OnHealthChanged?.Invoke(health, maxHealth);
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(health, maxHealth);
        }

        if (wasAlive)
        {
            numDeaths++;
            OnPlayerDeath?.Invoke();
        }
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
    // Does NOT fire death event - use KillPlayer() if you want to trigger death
    public void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);
        healthBar.UpdateHealthBar(health, maxHealth);
    }

}
