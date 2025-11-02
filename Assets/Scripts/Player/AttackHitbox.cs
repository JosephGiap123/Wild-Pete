using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] LayerMask enemyMask;
    [SerializeField] LayerMask staticMask;
    [SerializeField] CircleCollider2D circleCol;
    [SerializeField] BoxCollider2D boxCol;
    private Vector2 knockbackForce = Vector2.zero;
    private bool active = false;
    private int damage = 1;
    [Header("Hit Behavior")]
    [SerializeField] private bool disableAfterFirstHit = false; // optional single-hit behavior
    private readonly HashSet<GameObject> alreadyHit = new HashSet<GameObject>();

    private BasePlayerMovement2D parent;

    public void DisableAll()
    {
        if (circleCol != null) circleCol.enabled = false;
        if (boxCol != null) boxCol.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;

        if (((1 << other.gameObject.layer) & enemyMask) != 0) //bitshifting to find if sometihng is in said layer
        {
            //guaranteed to be an enemy.
            if (other.gameObject != null)
            {
                GameObject targetRoot = other.transform.parent.gameObject;
                if (alreadyHit.Contains(targetRoot)) return; // prevent multiple hits on same target during this activation
                alreadyHit.Add(targetRoot);
                Debug.Log("Hit enemy");
                targetRoot.GetComponent<EnemyBase>().Hurt(damage, knockbackForce);
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
                targetRoot.GetComponent<BreakableStatics>().Damage(damage, knockbackForce);
                if (disableAfterFirstHit) DisableHitbox();
            }
        }
    }

    public void ChangeHitboxCircle(Vector2 localOffset, float radius, Vector2 knockback, int dmg)
    {
        knockbackForce = new Vector2(knockback.x * (parent.isFacingRight ? 1f : -1f), knockback.y);
        DisableAll();
        circleCol.offset = new Vector2(localOffset.x, localOffset.y);
        circleCol.radius = radius;
        damage = dmg;
    }

    public void ChangeHitboxBox(Vector2 localOffset, Vector2 size, Vector2 knockback, int dmg)
    {
        knockbackForce = new Vector2(knockback.x * (parent.isFacingRight ? 1f : -1f), knockback.y);
        DisableAll();
        boxCol.offset = new Vector2(localOffset.x, localOffset.y);
        boxCol.size = size;
        damage = dmg;
    }

    public void ActivateCircle()
    {
        active = true;
        alreadyHit.Clear();
        circleCol.enabled = true;
    }

    public void ActivateBox()
    {
        active = true;
        alreadyHit.Clear();
        boxCol.enabled = true;
    }

    public void DisableHitbox()
    {
        DisableAll();
        active = false;
        alreadyHit.Clear();
    }

    private void Awake()
    {
        parent = transform.parent.GetComponent<BasePlayerMovement2D>();
    }
}
