using UnityEngine;

public class Landmine : MonoBehaviour
{
    [SerializeField] private AttackHitboxInfo attackHitboxInfo;

    [SerializeField] private BoxCollider2D boxCol;
    [SerializeField] private GameObject explosionPrefab;

    [SerializeField] private float destroyTimer = 15f;
    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip beepClip;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float beepVolume = 0.7f;
    [SerializeField] private bool loopBeep = true;


    public void Awake()
    {
        boxCol.enabled = false;
        if (!sfxSource) sfxSource = GetComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 1f;
        sfxSource.rolloffMode = AudioRolloffMode.Linear;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        LayerMask explodeLayers = attackHitboxInfo.player | attackHitboxInfo.enemy | attackHitboxInfo.statics;
        if (((1 << other.gameObject.layer) & explodeLayers) != 0)
        {
            GameObject newExplosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            newExplosion.GetComponent<ExplosionCloud>().Initialize(attackHitboxInfo);
            PlayExplosionSound();
            Destroy(gameObject);
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        LayerMask explodeLayers = attackHitboxInfo.player | attackHitboxInfo.enemy | attackHitboxInfo.statics;
        if (((1 << other.gameObject.layer) & explodeLayers) != 0)
        {
            GameObject newExplosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            newExplosion.GetComponent<ExplosionCloud>().Initialize(attackHitboxInfo);
            PlayExplosionSound();
            Destroy(gameObject);
        }
    }

    public void SwitchToNotifyAnim()
    {
        GetComponent<Animator>().Play("Notify");
        PlayBeepSound();
    }

    public void SwitchToLandMineAnim()
    {
        GetComponent<Animator>().Play("LandMine");
        boxCol.enabled = true; 
        Destroy(gameObject, destroyTimer);
    }

    public void InitializeLandMine()
    {
        SwitchToNotifyAnim();
    }

    private void PlayExplosionSound()
    {
        if (explosionClip == null || sfxSource == null) return;
        if (loopBeep && sfxSource.isPlaying && sfxSource.clip == beepClip) sfxSource.Stop();
        sfxSource.PlayOneShot(explosionClip, explosionVolume);
    }

    private void PlayBeepSound()
    {
        if (beepClip == null || sfxSource == null) return;
        sfxSource.clip = beepClip;
        sfxSource.volume = beepVolume;
        sfxSource.loop = loopBeep;
        sfxSource.Play();
    }
}
