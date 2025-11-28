using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class SpiderAudioManager : MonoBehaviour
{
    [Header("Audio Source (auto-created if empty)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip lungeClip;
    [SerializeField] private AudioClip swipeClip;
    [SerializeField] private AudioClip webShotClip;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip crawlLoopClip;

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Tuning")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 0.3f)] public float pitchJitter = 0.04f;
    public float max3dDistance = 20f;

    private AudioSource loopSource;

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

    // Public API
    public void PlayLunge()
    {
        PlayOneShot(lungeClip);
    }

    public void PlaySwipe()
    {
        PlayOneShot(swipeClip);
    }

    public void PlayWebShot()
    {
        PlayOneShot(webShotClip);
    }

    public void PlayHurt()
    {
        PlayOneShot(hurtClip);
    }

    public void PlayDeath()
    {
        PlayOneShot(deathClip);
    }

    public void StartCrawlLoop()
    {
        if (crawlLoopClip == null || loopSource == null) return;
        if (loopSource.isPlaying) return;
        loopSource.clip = crawlLoopClip;
        loopSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        loopSource.volume = sfxVolume;
        loopSource.Play();
    }

    public void StopCrawlLoop()
    {
        if (loopSource == null) return;
        if (loopSource.isPlaying) loopSource.Stop();
    }

    public void SetSfxVolume(float value01)
    {
        sfxVolume = Mathf.Clamp01(value01);
        if (loopSource != null && loopSource.isPlaying) loopSource.volume = sfxVolume;
    }

    // Helpers
    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        float oldPitch = sfxSource.pitch;
        sfxSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        sfxSource.PlayOneShot(clip, sfxVolume);
        sfxSource.pitch = oldPitch;
    }
}
