using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 14f;
    [SerializeField] private float bulletLifeTime = 3f;
    [SerializeField] private LayerMask bulletDestroyMask;
    [SerializeField] private LayerMask enemyMask;

    [SerializeField] private LayerMask staticMask;
    private Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        SetStraightVelocity();
        SetDestroyTime();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & bulletDestroyMask) != 0)
        {
            //potentially spawn fx if not an enemy.
            //damage enemies
            if (((1 << collision.gameObject.layer) & enemyMask) != 0)
            {
                if (collision.gameObject != null)
                {
                    Debug.Log("Hit enemy");
                    collision.gameObject.transform.parent.gameObject.GetComponent<EnemyBase>().Hurt(3, Vector2.zero);
                }
            }
            else if (((1 << collision.gameObject.layer) & staticMask) != 0)
            {
                if (collision.gameObject != null)
                {
                    Debug.Log("Hit enemy");
                    collision.gameObject.transform.parent.gameObject.GetComponent<BreakableStatics>().Damage(3, Vector2.zero);
                }
            }
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    private void SetStraightVelocity()
    {
        rb.linearVelocity = transform.right * bulletSpeed;
    }

    private void SetDestroyTime()
    {
        Destroy(gameObject, bulletLifeTime);
    }
}
