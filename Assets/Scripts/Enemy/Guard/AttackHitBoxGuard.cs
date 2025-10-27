using UnityEngine;

public class AttackHitBoxGuard : MonoBehaviour
{
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask staticMask;
    [SerializeField] private BoxCollider2D boxCol;
    [SerializeField] private GuardAI parent;
    private bool active = false;
    protected int damage = 1;
    protected Vector2 knockbackForce = new Vector2(0f, 0f);

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
                Debug.Log("Hit player");
                other.gameObject.transform.parent.gameObject.GetComponent<BasePlayerMovement2D>().HurtPlayer(damage, parent.isFacingRight ? 1f : -1f, knockbackForce);
                Debug.Log($"{(parent.isFacingRight ? 1f : -1f)} {knockbackForce}");
            }
        }
        else if (((1 << other.gameObject.layer) & staticMask) != 0)
        {
            if (other.gameObject != null)
            {
                Debug.Log("Hit static");
                other.gameObject.transform.parent.gameObject.GetComponent<BreakableStatics>().Damage(damage, new Vector2((parent.isFacingRight ? 1f : -1f) * knockbackForce.x, knockbackForce.y));
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
        boxCol.enabled = true;
    }

    public void DisableHitbox()
    {
        Disable();
        active = false;
    }
}
