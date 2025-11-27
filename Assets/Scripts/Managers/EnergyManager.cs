using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager instance;
    [SerializeField] EnergyBarScript energyBar;
    public float energy = 10f;
    public float maxEnergy = 10f;
    public float energyRegenRate = 1.5f;
    public float energyRegenDelay = 1f; //seconds to wait before regen starts
    public float energyRegenDelayTimer; //seconds to wait before regen starts (max)

    public event Action<float> OnEnergyChanged;
    public event Action<float> OnMaxEnergyChanged;
    public event Action<float> OnEnergyRegenRateChanged;
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

    void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Unsubscribe from stat changes
        if (StatsManager.instance != null)
        {
            StatsManager.instance.OnStatChanged -= HandleStatChanged;
        }
    }

    void HandlePlayerSet(GameObject player)
    {
        // Get max energy and regen rate from StatsManager
        if (StatsManager.instance != null)
        {
            maxEnergy = StatsManager.instance.maxEnergy;
            energyRegenRate = StatsManager.instance.energyRegenRate;
        }
        else
        {
            throw new Exception("StatsManager is not set");
        }

        energy = maxEnergy;

        // Find energy bar if not set
        if (energyBar == null || energyBar.gameObject == null)
        {
            energyBar = FindFirstObjectByType<EnergyBarScript>();
        }

        if (energyBar != null)
        {
            energyBar.SetMaxEnergy(maxEnergy);
            energyBar.SetEnergy(energy);
        }

        // Subscribe to stat changes for max energy/regen rate updates
        if (StatsManager.instance != null)
        {
            StatsManager.instance.OnStatChanged += HandleStatChanged;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            return;
        }

        // Find energy bar if not set
        if (energyBar == null || energyBar.gameObject == null)
        {
            energyBar = FindFirstObjectByType<EnergyBarScript>();
        }

        if (energyBar != null)
        {
            energyBar.SetMaxEnergy(maxEnergy);
            energyBar.SetEnergy(energy);
        }
    }

    void HandleStatChanged(EquipmentSO.Stats stat, float value)
    {
        if (stat == EquipmentSO.Stats.MaxEnergy)
        {
            SetMaxEnergy(value);
        }
        else if (stat == EquipmentSO.Stats.EnergyRegenRate)
        {
            SetEnergyRegenRate(value);
        }
    }

    void Start()
    {
        energy = maxEnergy;
    }

    void Update()
    {
        //decrease delay timer
        energyRegenDelayTimer -= Time.deltaTime;
        //regen energy if delay timer is 0 or less
        if (energyRegenDelayTimer <= 0)
        {
            float previousEnergy = energy;
            energy += energyRegenRate * Time.deltaTime;
            if (energy > maxEnergy)
            {
                energy = maxEnergy;
            }

            // Only update if energy actually changed
            if (energy != previousEnergy)
            {
                OnEnergyChanged?.Invoke(energy);
                if (energyBar != null)
                {
                    energyBar.UpdateEnergyBar(energy, maxEnergy);
                }
            }
        }
    }

    public bool UseEnergy(float energyCost)
    {
        if (energy >= energyCost)
        {
            energy -= energyCost;
            energyRegenDelayTimer = energyRegenDelay;
            OnEnergyChanged?.Invoke(energy);
            if (energyBar != null)
            {
                energyBar.UpdateEnergyBar(energy, maxEnergy);
            }
            Debug.Log("Used energy: " + energyCost + " Energy: " + energy);
            return true;
        }
        else
        {
            Debug.LogWarning("Not enough energy to use!");
            return false;
        }
    }

    void SetEnergyRegenRate(float newEnergyRegenRate)
    {
        energyRegenRate = newEnergyRegenRate;
        OnEnergyRegenRateChanged?.Invoke(energyRegenRate);
    }

    void SetMaxEnergy(float newMaxEnergy)
    {
        maxEnergy = newMaxEnergy;
        OnMaxEnergyChanged?.Invoke(maxEnergy);

        // Clamp current energy to new max
        if (energy > maxEnergy)
        {
            energy = maxEnergy;
        }

        if (energyBar != null)
        {
            energyBar.UpdateMaxEnergy(maxEnergy);
            energyBar.UpdateEnergyBar(energy, maxEnergy);
        }
    }

    void SetEnergy(float newEnergy)
    {
        energy = Mathf.Clamp(newEnergy, 0f, maxEnergy);
        OnEnergyChanged?.Invoke(energy);
        if (energyBar != null)
        {
            energyBar.UpdateEnergyBar(energy, maxEnergy);
        }
    }
}
