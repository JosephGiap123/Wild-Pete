using UnityEngine;

public class GuardBullet : MonoBehaviour
{
    [SerializeField] private LayerMask bulletDestroyMask;
    [SerializeField] private LayerMask playerMask;
    private int damage = 2;
    [SerializeField] private LayerMask staticMask;
    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & bulletDestroyMask) != 0)
        {
            //potentially spawn fx if not an enemy.
            //damage enemies
            if (((1 << collision.gameObject.layer) & playerMask) != 0)
            {
                if (collision.gameObject != null)
                {
                    Debug.Log("Hit player");
                    collision.gameObject.transform.parent.gameObject.GetComponent<BasePlayerMovement2D>().HurtPlayer(damage, 1f, Vector2.zero);
                }
            }
            else if (((1 << collision.gameObject.layer) & staticMask) != 0)
            {
                if (collision.gameObject != null)
                {
                    Debug.Log("Hit static");
                    collision.gameObject.transform.parent.gameObject.GetComponent<BreakableStatics>().Damage(1, Vector2.zero);
                }
            }
            Destroy(gameObject);
        }
    }

    public void Initialize(int damage, float speed, float lifeTime)
    {
        this.damage = damage;
        SetStraightVelocity(speed);
        SetDestroyTime(lifeTime);
    }
    private void SetStraightVelocity(float speed)
    {
        rb.linearVelocity = transform.right * speed;
    }
    private void SetDestroyTime(float lifeTime)
    {
        Destroy(gameObject, lifeTime);
    }
}