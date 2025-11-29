using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance;

    // Base stats (set in inspector or via InitializeStats)
    [SerializeField] private int baseMaxHealth = 20;
    [SerializeField] private int baseMaxAmmo = 5;
    [SerializeField] private float baseMovementSpeed = 3f;
    [SerializeField] private int baseJumpCount = 1;
    [SerializeField] private float baseDashSpeed = 12f;
    [SerializeField] private float baseSlideSpeed = 6f;
    [SerializeField] private float baseBulletSpeed = 10f;
    [SerializeField] private int baseBulletCount = 0;
    [SerializeField] private int baseMeleeAttack = 0;
    [SerializeField] private int baseWeaponlessMeleeAttack = 0;
    [SerializeField] private int baseRangedAttack = 0;
    [SerializeField] private int baseUniversalAttack = 0;
    [SerializeField] private float baseEnergy = 10f;
    [SerializeField] private float baseEnergyRegenRate = 1f;

    // Modifier tracking - stores all active modifiers for each stat
    private Dictionary<EquipmentSO.Stats, List<float>> statModifiers = new Dictionary<EquipmentSO.Stats, List<float>>();

    // Public properties that calculate final stats dynamically
    public int maxHealth => baseMaxHealth + (int)GetTotalModifier(EquipmentSO.Stats.MaxHealth);
    public int maxAmmo => baseMaxAmmo + (int)GetTotalModifier(EquipmentSO.Stats.MaxAmmo);
    public float MovementSpeed => baseMovementSpeed + GetTotalModifier(EquipmentSO.Stats.MovementSpeed);
    public int jumpCount => baseJumpCount + (int)GetTotalModifier(EquipmentSO.Stats.JumpCount);
    public float dashSpeed => baseDashSpeed + GetTotalModifier(EquipmentSO.Stats.DashSpeed);
    public float slideSpeed => baseSlideSpeed + GetTotalModifier(EquipmentSO.Stats.SlideSpeed);
    public float bulletSpeed => baseBulletSpeed + GetTotalModifier(EquipmentSO.Stats.BulletSpeed);
    public int bulletCount => baseBulletCount + (int)GetTotalModifier(EquipmentSO.Stats.BulletCount);
    public int meleeAttack => baseMeleeAttack + (int)GetTotalModifier(EquipmentSO.Stats.MeleeAttack);
    public int weaponlessMeleeAttack => baseWeaponlessMeleeAttack + (int)GetTotalModifier(EquipmentSO.Stats.WeaponlessMeleeAttack);
    public int rangedAttack => baseRangedAttack + (int)GetTotalModifier(EquipmentSO.Stats.RangedAttack);
    public int universalAttack => baseUniversalAttack + (int)GetTotalModifier(EquipmentSO.Stats.UniversalAttack);
    public float maxEnergy => baseEnergy + GetTotalModifier(EquipmentSO.Stats.MaxEnergy);
    public float energyRegenRate => baseEnergyRegenRate + GetTotalModifier(EquipmentSO.Stats.EnergyRegenRate);
    public event Action<EquipmentSO.Stats, float> OnStatChanged;

    void Awake() //singleton!
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize all stats and notify listeners
        NotifyAllStatsChanged();
    }

    public void InitializeStats(int baseMaxHealth, int baseMaxAmmo, float baseMovementSpeed, int baseJumpCount,
        float baseDashSpeed, float baseSlideSpeed, float baseBulletSpeed, int baseBulletCount,
        int baseMeleeAttack, int baseWeaponlessMeleeAttack, int baseRangedAttack, int baseUniversalAttack)
    {
        this.baseMaxHealth = baseMaxHealth;
        this.baseMaxAmmo = baseMaxAmmo;
        this.baseMovementSpeed = baseMovementSpeed;
        this.baseJumpCount = baseJumpCount;
        this.baseDashSpeed = baseDashSpeed;
        this.baseSlideSpeed = baseSlideSpeed;
        this.baseBulletSpeed = baseBulletSpeed;
        this.baseBulletCount = baseBulletCount;
        this.baseMeleeAttack = baseMeleeAttack;
        this.baseWeaponlessMeleeAttack = baseWeaponlessMeleeAttack;
        this.baseRangedAttack = baseRangedAttack;
        this.baseUniversalAttack = baseUniversalAttack;

        NotifyAllStatsChanged();
    }

    public void ResetStats()
    {
        statModifiers.Clear();
        NotifyAllStatsChanged();
    }

    public void AddStatModifier(EquipmentSO.Stats stat, float amount)
    {
        if (!statModifiers.ContainsKey(stat))
        {
            statModifiers[stat] = new List<float>();
        }

        statModifiers[stat].Add(amount);
        NotifyStatChanged(stat);
    }

    public void RemoveStatModifier(EquipmentSO.Stats stat, float amount)
    {
        if (statModifiers.ContainsKey(stat) && statModifiers[stat].Contains(amount))
        {
            statModifiers[stat].Remove(amount);
            NotifyStatChanged(stat);
        }
        else
        {
            Debug.LogWarning($"StatsManager: Could not remove modifier {amount} for stat {stat}. Modifier not found.");
        }
    }

    public void AddEquipmentStats(EquipmentSO equipment)
    {
        if (equipment == null || equipment.itemStats == null || equipment.itemStatAmounts == null)
        {
            return;
        }

        for (int i = 0; i < equipment.itemStats.Count && i < equipment.itemStatAmounts.Count; i++)
        {
            AddStatModifier(equipment.itemStats[i], equipment.itemStatAmounts[i]);
        }
    }


    public void RemoveEquipmentStats(EquipmentSO equipment)
    {
        if (equipment == null || equipment.itemStats == null || equipment.itemStatAmounts == null)
        {
            return;
        }

        for (int i = 0; i < equipment.itemStats.Count && i < equipment.itemStatAmounts.Count; i++)
        {
            RemoveStatModifier(equipment.itemStats[i], equipment.itemStatAmounts[i]);
        }
    }

    public void ChangeStat(EquipmentSO.Stats stat, float amount)
    {
        AddStatModifier(stat, amount);
    }

    private float GetTotalModifier(EquipmentSO.Stats stat)
    {
        if (statModifiers.ContainsKey(stat))
        {
            return statModifiers[stat].Sum();
        }
        return 0f;
    }

    public float GetStatFloat(EquipmentSO.Stats stat)
    {
        switch (stat)
        {
            case EquipmentSO.Stats.MovementSpeed:
                return MovementSpeed;
            case EquipmentSO.Stats.DashSpeed:
                return dashSpeed;
            case EquipmentSO.Stats.SlideSpeed:
                return slideSpeed;
            case EquipmentSO.Stats.BulletSpeed:
                return bulletSpeed;
            case EquipmentSO.Stats.MaxEnergy:
                return maxEnergy;
            case EquipmentSO.Stats.EnergyRegenRate:
                return energyRegenRate;
            default:
                return 0f;
        }
    }

    public int GetStatInt(EquipmentSO.Stats stat)
    {
        switch (stat)
        {
            case EquipmentSO.Stats.MaxHealth:
                return maxHealth;
            case EquipmentSO.Stats.MaxAmmo:
                return maxAmmo;
            case EquipmentSO.Stats.JumpCount:
                return jumpCount;
            case EquipmentSO.Stats.BulletCount:
                return bulletCount;
            case EquipmentSO.Stats.MeleeAttack:
                return meleeAttack;
            case EquipmentSO.Stats.RangedAttack:
                return rangedAttack;
            case EquipmentSO.Stats.UniversalAttack:
                return universalAttack;
            case EquipmentSO.Stats.WeaponlessMeleeAttack:
                return weaponlessMeleeAttack;

            default:
                return 0;
        }
    }

    private void NotifyStatChanged(EquipmentSO.Stats stat)
    {
        // Get the final calculated value
        float finalValue = 0f;
        if (IsIntStat(stat))
        {
            finalValue = GetStatInt(stat);
        }
        else
        {
            finalValue = GetStatFloat(stat);
        }

        OnStatChanged?.Invoke(stat, finalValue);
    }

    private void NotifyAllStatsChanged()
    {
        foreach (EquipmentSO.Stats stat in Enum.GetValues(typeof(EquipmentSO.Stats)))
        {
            NotifyStatChanged(stat);
        }
    }

    private bool IsIntStat(EquipmentSO.Stats stat)
    {
        return stat == EquipmentSO.Stats.MaxHealth ||
               stat == EquipmentSO.Stats.MaxAmmo ||
               stat == EquipmentSO.Stats.JumpCount ||

               stat == EquipmentSO.Stats.BulletCount ||
               stat == EquipmentSO.Stats.WeaponlessMeleeAttack ||
               stat == EquipmentSO.Stats.MeleeAttack ||
               stat == EquipmentSO.Stats.RangedAttack ||
               stat == EquipmentSO.Stats.UniversalAttack;
    }
}
