using UnityEngine;

public class GenericHurtCollision : MonoBehaviour
{
	[SerializeField] EnemyBase parentScript;
	[SerializeField] BoxCollider2D hurtBox;
	[SerializeField] float damageCooldown = 1f;
	[SerializeField] AttackHitboxInfo attackHitboxInfo;
	private float damageTimer = 0f;

	void Update()
	{
		if (parentScript.isDead || parentScript.isHurt || PauseController.IsGamePaused)
		{
			return;
		}
		damageTimer -= Time.deltaTime;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (((1 << collision.gameObject.layer) & attackHitboxInfo.player) == 0)
		{
			return;
		}
		if (damageTimer > 0 || parentScript.isDead || parentScript.isHurt) return;
		damageTimer = damageCooldown;
		BasePlayerMovement2D player = collision.transform.parent.GetComponent<BasePlayerMovement2D>();
		if (player != null)
		{
			// Don't attack if player is dead
			if (HealthManager.instance != null && HealthManager.instance.IsDead())
			{
				return;
			}
			player.HurtPlayer(attackHitboxInfo.damage, attackHitboxInfo.knockbackForce, null, transform.position);
			return;
		}
	}
}
