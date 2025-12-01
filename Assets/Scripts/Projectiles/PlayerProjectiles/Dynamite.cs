using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
public class Dynamite : MonoBehaviour
{
    [SerializeField] GameObject explosionCloud;
    [SerializeField] GameObject explosionParticles;
    [SerializeField] AttackHitboxInfo explosionHitbox;
    [SerializeField] float explosionTime = 3f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip explosionSound;
    [Range(0f, 1f)][SerializeField] private float explosionVolume = 1f;
    
    private Rigidbody2D rb;
    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (!sfxSource) sfxSource = GetComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 1f;
        sfxSource.rolloffMode = AudioRolloffMode.Linear;
        sfxSource.playOnAwake = false;
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
        
        // Play explosion sound
        if (explosionSound && sfxSource)
        {
            sfxSource.PlayOneShot(explosionSound, explosionVolume);
        }
        
        GameObject newExplosionCloud = Instantiate(explosionCloud, transform.position, Quaternion.identity);
        newExplosionCloud.GetComponent<ExplosionCloud>().Initialize(explosionHitbox);
        Instantiate(explosionParticles, transform.position, Quaternion.identity);
        GetComponentInChildren<CinemachineImpulseSource>()?.GenerateImpulse(1.0f);
        
        // Wait for sound to finish before destroying (if desired), or destroy immediately
        if (explosionSound && sfxSource && sfxSource.isPlaying)
        {
            yield return new WaitForSeconds(explosionSound.length);
        }
        
        Destroy(gameObject);
    }
}
