using UnityEngine;

[CreateAssetMenu(fileName = "AttackHitboxInfo", menuName = "Attacks/AttackHitboxInfo")]
public class AttackHitboxInfo : ScriptableObject
{
	public LayerMask player;
	public LayerMask enemy;
	public LayerMask statics;
	public Vector2 knockbackForce;
	public Vector2 hitboxOffset;
	public Vector2 hitboxSize;
	public int damage;
	public float stunTime;
	public bool constantKnockback = true;

	public enum AttackType //CHANGE THIS LATER!
	{
		None,
		Melee,
		Ranged,
		WeaponlessMelee
	}

	public AttackType attackType;

}
