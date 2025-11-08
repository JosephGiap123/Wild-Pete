using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class GuardAudioManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Audio Source (auto-created if empty)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip shot;
    [SerializeField] private AudioClip meleeSwing1;
    [SerializeField] private AudioClip meleeSwing2;
    [SerializeField] private AudioClip dash;
    [SerializeField] private AudioClip hurt;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip runLoop;
    [SerializeField] private AudioClip hit;
  

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Tuning")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 0.3f)] public float pitchJitter = 0.05f;
    public float max3dDistance = 20f;

    private AudioSource loopSource;
    private Coroutine pitchResetCo;
    private int lastMeleeIndex = -1;

    void Awake()
    {
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 1f; // 3D
        sfxSource.rolloffMode = AudioRolloffMode.Linear;
        sfxSource.maxDistance = max3dDistance;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.spatialBlend = 1f;
        loopSource.rolloffMode = AudioRolloffMode.Linear;
        loopSource.maxDistance = max3dDistance;
        loopSource.outputAudioMixerGroup = sfxMixerGroup;
        loopSource.playOnAwake = false;
        loopSource.volume = sfxVolume;
    }
    // -------- public hooks ----------
    public void PlayShot() => PlayOneShot(shot);
    public void PlayMelee() => PlayRandomMelee();  // randomized between two swings
    public void PlayDash() => PlayOneShot(dash);
    public void PlayHurt() => PlayOneShot(hurt);
    public void PlayDeath() => PlayOneShot(death);
    public void PlayHit() => PlayOneShot(hit);

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

    // -------- helpers ----------
    private void PlayOneShot(AudioClip clip)
    {
        if (!clip) return;
        float oldPitch = sfxSource.pitch;
        sfxSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        sfxSource.PlayOneShot(clip, sfxVolume);
        sfxSource.pitch = oldPitch;
    }

    private void PlayRandomMelee()
    {
        // Select one of the two swings, avoiding immediate repeats if both exist
        AudioClip chosen = null;

        if (meleeSwing1 && meleeSwing2)
        {
            int index;
            int tries = 0;
            do
            {
                index = Random.Range(0, 2); // 0 or 1
                tries++;
            } while (index == lastMeleeIndex && tries < 4);

            lastMeleeIndex = index;
            chosen = (index == 0) ? meleeSwing1 : meleeSwing2;
        }
        else
        {
            // Fallback if only one is assigned
            chosen = meleeSwing1 ? meleeSwing1 : meleeSwing2;
        }

        PlayOneShot(chosen);
    }
}
