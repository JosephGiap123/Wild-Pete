using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class WardenAudioManager : MonoBehaviour
{
    [Header("Primary Audio Source (auto-created if empty)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Run Loop")]
    [SerializeField] private AudioClip runLoop;

    [Header("Melee Variations")]
    [SerializeField] private AudioClip[] meleeClips = new AudioClip[3];

    [Header("Abilities")]
    [SerializeField] private AudioClip rangedAttack;
    [SerializeField] private AudioClip teleportSlam;
    [SerializeField] private AudioClip groundLasers;
    [SerializeField] private AudioClip laserBeam;

    [Header("Damage States")]
    [SerializeField] private AudioClip hurt;
    [SerializeField] private AudioClip death;

    [Header("Boss Music")]
    [SerializeField] private AudioClip bossMusicClip;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.7f;

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Tuning")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 0.3f)] public float pitchJitter = 0.05f;
    public float max3dDistance = 30f;

    private AudioSource loopSource;
    private AudioSource musicSource;
    private int lastMeleeIndex = -1;
    [Header("Distance Fade")]
    [Tooltip("Enable distance-based fading for the run loop")]
    public bool enableDistanceFade = true;
    [Tooltip("Distance (units) within which the loop is at full run volume")]
    public float fadeFullDistance = 8f;
    [Tooltip("Distance (units) beyond which the loop is silent")]
    public float fadeZeroDistance = 40f;
    [Range(0f,1f)] public float runVolumeMultiplier = 0.5f; // quieter baseline for Warden

    private Transform player;

    private void Awake()
    {
        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        sfxSource.spatialBlend = 1f;
        sfxSource.rolloffMode = AudioRolloffMode.Linear;
        sfxSource.maxDistance = max3dDistance;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = 1f;
        loopSource.rolloffMode = AudioRolloffMode.Linear;
        loopSource.maxDistance = max3dDistance;
        loopSource.outputAudioMixerGroup = sfxMixerGroup;
        loopSource.volume = sfxVolume;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f; // non-spatial for music
        musicSource.outputAudioMixerGroup = sfxMixerGroup;
        musicSource.volume = musicVolume;

        // try to find player
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
    }

    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
    }

    private void HandlePlayerSet(GameObject playerObj)
    {
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (loopSource != null && loopSource.isPlaying && enableDistanceFade && player != null)
        {
            float dist = Vector2.Distance(player.position, transform.position);
            float fade = ComputeFadeMultiplier(dist, fadeFullDistance, fadeZeroDistance);
            loopSource.volume = sfxVolume * runVolumeMultiplier * fade;
        }
    }

    private float ComputeFadeMultiplier(float distance, float fullDist, float zeroDist)
    {
        if (!enableDistanceFade) return 1f;
        if (distance <= fullDist) return 1f;
        if (distance >= zeroDist) return 0f;
        if (Mathf.Approximately(zeroDist, fullDist)) return 0f;
        return 1f - ((distance - fullDist) / (zeroDist - fullDist));
    }

    // ---- Public hooks ----
    public void PlayRangedAttack() => PlayOneShot(rangedAttack);
    public void PlayTeleportSlam() => PlayOneShot(teleportSlam);
    public void PlayGroundLasers() => PlayOneShot(groundLasers);
    public void PlayLaserBeam() => PlayOneShot(laserBeam);
    public void PlayHurt() => PlayOneShot(hurt);
    public void PlayDeath() => PlayOneShot(death);

    public void StartBossMusic()
    {
        if (!bossMusicClip || musicSource == null) return;
        if (musicSource.isPlaying) return;
        musicSource.clip = bossMusicClip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopBossMusic()
    {
        if (musicSource == null) return;
        if (musicSource.isPlaying) musicSource.Stop();
    }

    public void PlayMelee()
    {
        if (meleeClips == null || meleeClips.Length == 0)
        {
            return;
        }

        AudioClip chosen = null;

        if (meleeClips.Length == 1)
        {
            chosen = meleeClips[0];
        }
        else
        {
            int idx;
            int tries = 0;
            do
            {
                idx = Random.Range(0, meleeClips.Length);
                tries++;
            } while (idx == lastMeleeIndex && tries < 5);

            lastMeleeIndex = idx;
            chosen = meleeClips[idx];
        }

        PlayOneShot(chosen);
    }

    public void StartRunLoop()
    {
        if (!runLoop || loopSource.isPlaying) return;
        loopSource.clip = runLoop;
        loopSource.pitch = 1f;
        loopSource.volume = sfxVolume;
        loopSource.Play();
    }

    public void StopRunLoop()
    {
        if (loopSource.isPlaying) loopSource.Stop();
    }

    public void SetSfxVolume(float value01)
    {
        sfxVolume = Mathf.Clamp01(value01);
        if (loopSource.isPlaying) loopSource.volume = sfxVolume;
    }

    // ---- helpers ----
    private void PlayOneShot(AudioClip clip)
    {
        if (!clip) return;
        float oldPitch = sfxSource.pitch;
        sfxSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        sfxSource.PlayOneShot(clip, sfxVolume);
        sfxSource.pitch = oldPitch;
    }
}
