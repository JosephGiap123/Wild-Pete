using UnityEngine;
using System.Collections.Generic;

public class AttackHitBoxGuard : MonoBehaviour
{
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask staticMask;
    [SerializeField] private BoxCollider2D boxCol;
    [SerializeField] private GuardAI parent;
    private bool active = false;
    protected int damage = 1;
    protected Vector2 knockbackForce = new Vector2(0f, 0f);
    [Header("Hit Behavior")]
    [SerializeField] private bool disableAfterFirstHit = false; // optional single-hit behavior
    private readonly HashSet<GameObject> alreadyHit = new HashSet<GameObject>();

    public void Disable()
    {
        if (boxCol != null) boxCol.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;

        if (((1 << other.gameObject.layer) & playerMask) != 0) //bitshifting to find if sometihng is in said layer
        {
            //guaranteed to be an enemy.
            if (other.gameObject != null)
            {
                GameObject targetRoot = other.transform.parent.gameObject;
                if (alreadyHit.Contains(targetRoot)) return; // prevent multiple hits on same target during this activation
                alreadyHit.Add(targetRoot);
                Debug.Log("Hit player");
                targetRoot.GetComponent<BasePlayerMovement2D>().HurtPlayer(damage, knockbackForce, parent.isFacingRight ? 1f : -1f);
                Debug.Log($"{(parent.isFacingRight ? 1f : -1f)} {knockbackForce}");
                if (disableAfterFirstHit) DisableHitbox();
            }
        }
        else if (((1 << other.gameObject.layer) & staticMask) != 0)
        {
            if (other.gameObject != null)
            {
                GameObject targetRoot = other.transform.parent.gameObject;
                if (alreadyHit.Contains(targetRoot)) return;
                alreadyHit.Add(targetRoot);
                Debug.Log("Hit static");
                targetRoot.GetComponent<BreakableStatics>().Damage(damage, new Vector2((parent.isFacingRight ? 1f : -1f) * knockbackForce.x, knockbackForce.y));
                if (disableAfterFirstHit) DisableHitbox();
            }
        }
    }

    public void ChangeHitboxBox(Vector2 localOffset, Vector2 size, int damageAmount, Vector2 knockback)
    {
        Disable();
        damage = damageAmount;
        boxCol.offset = new Vector2(localOffset.x, localOffset.y);
        boxCol.size = size;
        knockbackForce = knockback;
    }

    public void ActivateBox()
    {
        active = true;
        alreadyHit.Clear();
        boxCol.enabled = true;
    }

    public void DisableHitbox()
    {
        Disable();
        active = false;
        alreadyHit.Clear();
    }
}
