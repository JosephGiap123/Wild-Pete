using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EquipmentSO", menuName = "Items/EquipmentSO")]
public class EquipmentSO : ItemSO
{

	public enum EquipmentSlot
	{
		Body,
		Boots,
		Melee,
		Ranged
	}
	public enum EquipableBy
	{
		Alice,
		Pete,
		Both
	}

	public enum Stats
	{
		MaxHealth,
		MeleeAttack,
		WeaponlessMeleeAttack,
		RangedAttack,
		UniversalAttack,
		MovementSpeed,
		JumpCount,
		DashSpeed,
		SlideSpeed,
		MaxAmmo,
		BulletSpeed,
		BulletCount,
		MaxEnergy,
		EnergyRegenRate
	}
	public List<Stats> itemStats;
	public List<float> itemStatAmounts;
	public EquipmentSlot equipmentType;

	public EquipableBy equipableBy;

	[Header("Weapon Behavior (Optional)")]
	[Tooltip("If true, this melee weapon disables the held weapon but increases weaponless damage")]
	public bool disablesHeldWeapon = false;

	[Header("Ranged Weapon Projectile (Optional)")]
	[Tooltip("Custom projectile prefab for this ranged weapon. If null, uses default bullet.")]
	public GameObject customProjectilePrefab;

}
