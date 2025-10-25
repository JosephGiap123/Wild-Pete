using UnityEngine;

public class ShotgunBullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float bulletLifeTime = 3f;
    [SerializeField] private LayerMask bulletDestroyMask;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask staticMask;
    [SerializeField] private float shotgunCone = 20f;
    [SerializeField] private float maxSpeedVariation = 3f;
    private float randomizeAngle;
    private Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        randomizeAngle = Random.Range(-shotgunCone, shotgunCone);
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
                    collision.gameObject.transform.parent.gameObject.GetComponent<EnemyBase>().Hurt(1);
                }
            }
            else if (((1 << collision.gameObject.layer) & staticMask) != 0)
            {
                if (collision.gameObject != null)
                {
                    Debug.Log("Hit enemy");
                    collision.gameObject.transform.parent.gameObject.GetComponent<BreakableStatics>().Damage(1);
                }
            }
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    private void SetStraightVelocity()
    {
        transform.Rotate(0, 0, randomizeAngle);
        rb.linearVelocity = transform.right * Random.Range(bulletSpeed - maxSpeedVariation, bulletSpeed + maxSpeedVariation);
    }

    private void SetDestroyTime()
    {
        Destroy(gameObject, bulletLifeTime);
    }
}
