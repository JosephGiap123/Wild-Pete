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

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Tuning")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 0.3f)] public float pitchJitter = 0.05f;
    public float max3dDistance = 30f;

    private AudioSource loopSource;
    private int lastMeleeIndex = -1;

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
    }

    // ---- Public hooks ----
    public void PlayRangedAttack() => PlayOneShot(rangedAttack);
    public void PlayTeleportSlam() => PlayOneShot(teleportSlam);
    public void PlayGroundLasers() => PlayOneShot(groundLasers);
    public void PlayLaserBeam() => PlayOneShot(laserBeam);
    public void PlayHurt() => PlayOneShot(hurt);
    public void PlayDeath() => PlayOneShot(death);

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
