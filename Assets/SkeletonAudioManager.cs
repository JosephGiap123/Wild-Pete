using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class SkeletonAudioManager : MonoBehaviour
{
    [Header("Audio Source (auto-created if empty)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip runLoopClip;

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Tuning")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 0.3f)] public float pitchJitter = 0.04f;
    public float max3dDistance = 20f;

    private AudioSource loopSource;
    [Header("Distance Fade")]
    [Tooltip("Enable distance-based fading for the run loop")]
    public bool enableDistanceFade = true;
    [Tooltip("Distance (units) within which the loop is at full run volume")]
    public float fadeFullDistance = 6f;
    [Tooltip("Distance (units) beyond which the loop is silent")]
    public float fadeZeroDistance = 20f;
    [Range(0f,1f)] public float runVolumeMultiplier = 0.6f; // make loop quieter by default

    private Transform player;

    private void Awake()
    {
        if (!sfxSource) sfxSource = GetComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.spatialBlend = 1f;
        sfxSource.rolloffMode = AudioRolloffMode.Linear;
        sfxSource.maxDistance = max3dDistance;
        sfxSource.playOnAwake = false;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.spatialBlend = 1f;
        loopSource.rolloffMode = AudioRolloffMode.Linear;
        loopSource.maxDistance = max3dDistance;
        loopSource.playOnAwake = false;
        loopSource.outputAudioMixerGroup = sfxMixerGroup;
        loopSource.volume = sfxVolume;
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

    // Public API
    public void PlayAttack()
    {
        PlayOneShot(attackClip);
    }

    public void PlayHurt()
    {
        PlayOneShot(hurtClip);
    }

    public void StartRunLoop()
    {
        if (runLoopClip == null || loopSource == null) return;
        if (loopSource.isPlaying) return;
        loopSource.clip = runLoopClip;
        loopSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        loopSource.volume = sfxVolume;
        loopSource.Play();
    }

    public void StopRunLoop()
    {
        if (loopSource == null) return;
        if (loopSource.isPlaying) loopSource.Stop();
    }

    public void SetSfxVolume(float value01)
    {
        sfxVolume = Mathf.Clamp01(value01);
        if (loopSource != null && loopSource.isPlaying) loopSource.volume = sfxVolume;
    }

    // Helper
    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        float oldPitch = sfxSource.pitch;
        sfxSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        sfxSource.PlayOneShot(clip, sfxVolume);
        sfxSource.pitch = oldPitch;
    }
}
