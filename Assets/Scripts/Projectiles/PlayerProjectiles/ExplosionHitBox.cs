using System.Collections.Generic;
using UnityEngine;

public class ExplosionHitBox : MonoBehaviour
{

    [SerializeField] CircleCollider2D hitbox;
    [SerializeField] LayerMask enemy;
    [SerializeField] LayerMask player;
    [SerializeField] LayerMask statics;
    protected int damage = 10;
    protected float maxKnockback = 15f;

    private readonly HashSet<GameObject> alreadyHit = new HashSet<GameObject>();
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemy) != 0) //bitshifting to find if sometihng is in said layer
        {
            //guaranteed to be an enemy.
            if (collision.gameObject != null)
            {
                GameObject targetRoot = collision.transform.parent.gameObject;
                if (alreadyHit.Contains(targetRoot)) return; // prevent multiple hits on same target during this activation
                alreadyHit.Add(targetRoot);
                Debug.Log("Hit enemy");
                targetRoot.GetComponent<EnemyBase>().Hurt(damage, calculateKnockback(collision.transform.position));
            }
        }
        else if (((1 << collision.gameObject.layer) & statics) != 0)
        {
            if (collision.gameObject != null)
            {
                GameObject targetRoot = collision.transform.parent.gameObject;
                if (alreadyHit.Contains(targetRoot)) return;
                alreadyHit.Add(targetRoot);
                Debug.Log("Hit static");
                targetRoot.GetComponent<BreakableStatics>().Damage(damage, calculateKnockback(collision.transform.position));
            }
        }
        else if (((1 << collision.gameObject.layer) & player) != 0)
        {
            if (collision.gameObject != null)
            {
                GameObject targetRoot = collision.transform.parent.gameObject;
                if (alreadyHit.Contains(targetRoot)) return;
                alreadyHit.Add(targetRoot);
                Debug.Log("Hit static");
                targetRoot.GetComponent<BasePlayerMovement2D>().HurtPlayer(damage, transform.position, calculateKnockback(collision.transform.position));
            }
        }
    }

    public Vector2 calculateKnockback(Vector3 position)
    {
        Vector2 explosionPos = this.gameObject.transform.position;
        Vector2 targetPos = position;
        // Direction should be FROM explosion TO target (away from explosion)
        Vector2 knockbackDirection = targetPos - explosionPos;
        Vector2 normalizedDirection = knockbackDirection.normalized;
        float distToTarget = Vector2.Distance(explosionPos, targetPos);
        const float minimumDistance = 0.5f;
        float effectiveDistance = Mathf.Max(distToTarget, minimumDistance);

        float knockbackMagnitude = maxKnockback / effectiveDistance;

        return normalizedDirection * knockbackMagnitude;

    }
}
