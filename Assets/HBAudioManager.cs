using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class HBAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    private AudioSource loopSource;

    [Header("Attack Sounds")]
    [SerializeField] private AudioClip meleeClip;
    [SerializeField] private AudioClip boosterDashClip;
    [SerializeField] private AudioClip rocketShotClip;
    [SerializeField] private AudioClip rocketExplosiveClip;
    [SerializeField] private AudioClip dynamiteClip;

    [Header("State Sounds")]
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip staggerIndicatorClip;
    [SerializeField] private AudioClip rocketJumpClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip runLoopClip;

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Tuning")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 0.3f)] public float pitchJitter = 0.04f;
    public float max3dDistance = 30f;

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
    }

    // Public API - Attack Sounds
    public void PlayMelee()
    {
        PlayOneShot(meleeClip);
    }

    public void PlayBoosterDash()
    {
        PlayOneShot(boosterDashClip);
    }

    public void PlayRocketShot()
    {
        PlayOneShot(rocketShotClip);
    }

    public void PlayRocketExplosive()
    {
        PlayOneShot(rocketExplosiveClip);
    }

    public void PlayDynamite()
    {
        PlayOneShot(dynamiteClip);
    }

    // State Sounds
    public void PlayHurt()
    {
        PlayOneShot(hurtClip);
    }

    public void PlayStaggerIndicator()
    {
        PlayOneShot(staggerIndicatorClip);
    }

    public void PlayRocketJump()
    {
        PlayOneShot(rocketJumpClip);
    }

    public void PlayDeath()
    {
        PlayOneShot(deathClip);
    }

    // Run Loop Control
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
