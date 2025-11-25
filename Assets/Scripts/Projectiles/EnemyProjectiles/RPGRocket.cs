using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class RPGRocket : MonoBehaviour
{
    [SerializeField] private LayerMask bulletDestroyMask;
    [SerializeField] private LayerMask playerMask;
    private int damage = 2;
    [SerializeField] private LayerMask staticMask;
    private Rigidbody2D rb;

    [SerializeField] private AttackHitboxInfo explosionHitbox;

    [SerializeField] private GameObject explosion;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool shouldDestroy = false;
        if (((1 << collision.gameObject.layer) & bulletDestroyMask) != 0)
        {
            shouldDestroy = true;
        }
        else if (((1 << collision.gameObject.layer) & playerMask) != 0)
        {
            if (collision.gameObject != null)
            {
                collision.gameObject.transform.parent.gameObject.GetComponent<BasePlayerMovement2D>().HurtPlayer(damage, Vector2.zero, 1f);
                shouldDestroy = true;
            }
        }
        else if (((1 << collision.gameObject.layer) & staticMask) != 0)
        {
            if (collision.gameObject != null)
            {
                collision.gameObject.transform.parent.gameObject.GetComponent<BreakableStatics>().Damage(damage, Vector2.zero);
                shouldDestroy = true;
            }
        }
        if (shouldDestroy)
        {
            GetComponentInChildren<CinemachineImpulseSource>()?.GenerateImpulse(0.6f);
            SpawnExplosion();
            DestroyRocket();
        }
    }

    public void DestroyRocket()
    {
        this.gameObject.GetComponentInChildren<ParticleSystem>().Stop();
        this.gameObject.GetComponentInChildren<ParticleSystem>().transform.parent = null;
        Destroy(gameObject);
    }

    public void SpawnExplosion()
    {
        GameObject newExplosion = Instantiate(explosion, transform.position, Quaternion.identity);
        ExplosionCloud explosionCloud = newExplosion.GetComponent<ExplosionCloud>();
        explosionCloud.Initialize(explosionHitbox);
    }

    public void Initialize(int damage, float speed, float angle, float lifeTime)
    {
        this.damage = damage;
        SetVelocity(speed, angle);
        transform.Rotate(0, 0, angle);
        SetDestroyTime(lifeTime);
    }
    private void SetVelocity(float speed, float angle)
    {
        // Convert angle from degrees to radians for Mathf.Cos and Mathf.Sin
        float angleInRadians = angle * Mathf.Deg2Rad;
        rb.linearVelocity = new Vector2(speed * Mathf.Cos(angleInRadians), speed * Mathf.Sin(angleInRadians));
    }
    private void SetDestroyTime(float lifeTime)
    {
        StartCoroutine(DestroyRocketAfterTime(lifeTime));
    }
    private IEnumerator DestroyRocketAfterTime(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        DestroyRocket();
    }
}
