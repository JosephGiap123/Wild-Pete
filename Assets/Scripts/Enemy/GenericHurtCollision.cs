// using UnityEngine;

// public class GenericHurtCollision : MonoBehaviour
// {
//     [SerializeField] EnemyBase parentScript;
//     [SerializeField] BoxCollider2D hurtBox;
//     [SerializeField] Vector2 knockbackOnHurt = Vector2.zero;
//     [SerializeField] int damage = 1;
//     [SerializeField] float damageCooldown = 1f;
//     private float damageTimer = 0f;

//     void Update()
//     {
//         if (parentScript.isDead || parentScript.isHurt)
//         {
//             return;
//         }
//         damageTimer -= Time.deltaTime;
//     }

//     void OnTriggerEnter2D(Collider2D collision)
//     {
//         if (damageTimer > 0) return;
//         damageTimer = damageCooldown;
//         BasePlayerMovement2D player = targetRoot.GetComponent<BasePlayerMovement2D>();
//         if (player != null)
//         {
//             // Don't attack if player is dead
//             if (HealthManager.instance != null && HealthManager.instance.IsDead())
//             {
//                 return;
//             }

//             player.HurtPlayer(currentDamage, finalKnockback, null, hitboxCenter);

//             if (disableAfterFirstHit) DisableHitbox();
//             return;
//         }
//     }
// }
