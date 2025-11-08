using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
public class Dynamite : MonoBehaviour
{
    [SerializeField] GameObject explosionCloud;
    [SerializeField] GameObject explosionParticles;
    [SerializeField] float explosionTime = 3f;
    private Rigidbody2D rb;
    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void Initialize(Vector2 initVelocity)
    {
        rb.linearVelocity = initVelocity;
        rb.angularVelocity = 360f * (rb.rotation + 360) / Mathf.Abs(rb.rotation + 360);
        StartCoroutine(Explode());
    }

    public IEnumerator Explode()
    {
        yield return new WaitForSeconds(explosionTime);
        Instantiate(explosionCloud, transform.position, Quaternion.identity);
        Instantiate(explosionParticles, transform.position, Quaternion.identity);
        GetComponentInChildren<CinemachineImpulseSource>()?.GenerateImpulse(1.0f);
        Destroy(gameObject);
    }
}
