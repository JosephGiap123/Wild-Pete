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

	public enum Stats
	{
		MaxHealth,
		MeleeAttack,
		RangedAttack,
		UniversalAttack,
		MovementSpeed,
		JumpCount,
		DashSpeed,
		SlideSpeed,
		MaxAmmo,
		BulletSpeed,
		BulletCount
	}
	public List<Stats> itemStats;
	public List<float> itemStatAmounts;
	public EquipmentSlot equipmentType;


}
