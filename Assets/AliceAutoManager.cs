using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AliceAudioManager : MonoBehaviour
{
    public static AliceAudioManager instance;

    [Header("Audio Source (auto-created if empty)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip shotgun;
    [SerializeField] private AudioClip reload;
    [SerializeField] private AudioClip[] hammerClips;
    [SerializeField] private AudioClip sweep;
    [SerializeField] private AudioClip dash;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip hurt;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip runLoop;

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Tuning")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 0.3f)] public float pitchJitter = 0.05f;
    public float max3dDistance = 20f;

    private AudioSource loopSource;
    private Coroutine pitchResetCo;
    private int lastHammerIndex = -1;

    private void Awake()
    {
        // ✅ Singleton setup to prevent duplicates & ensure persistence
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // ✅ Ensure Audio Sources exist
        if (!sfxSource)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.spatialBlend = 1f;
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

    // ---------- Public SFX Calls ----------
    public void PlayShotgun() => PlayOneShot(shotgun);
    public void PlayDash() => PlayOneShot(dash);
    public void PlaySweep() => PlayOneShot(sweep);
    public void PlayJump() => PlayOneShot(jump);
    public void PlayHurt() => PlayOneShot(hurt);
    public void PlayDeath() => PlayOneShot(death);

    public void PlayHammer()
    {
        if (hammerClips == null || hammerClips.Length == 0) return;

        AudioClip clip;
        if (hammerClips.Length == 1)
        {
            clip = hammerClips[0];
        }
        else
        {
            int idx;
            int tries = 0;
            do
            {
                idx = Random.Range(0, hammerClips.Length);
                tries++;
            } while (idx == lastHammerIndex && tries < 5);

            lastHammerIndex = idx;
            clip = hammerClips[idx];
        }

        PlayOneShot(clip);
    }

    // ---------- Reload (Pitch + Speed) ----------
    public void PlayReload()
    {
        if (!reload || sfxSource == null) return;

        if (pitchResetCo != null) StopCoroutine(pitchResetCo);

        sfxSource.Stop();
        float oldPitch = sfxSource.pitch;
        var oldClip = sfxSource.clip;

        sfxSource.pitch = 2f;
        sfxSource.clip = reload;
        sfxSource.volume = sfxVolume;
        sfxSource.Play();

        float playTime = reload.length / 2f;
        pitchResetCo = StartCoroutine(ResetPitchAfter(playTime, oldPitch, oldClip));
    }

    // ---------- Run Loop ----------
    public void StartRunLoop()
    {
        if (loopSource == null || runLoop == null || loopSource.isPlaying) return;
        loopSource.clip = runLoop;
        loopSource.pitch = 1f;
        loopSource.volume = sfxVolume;
        loopSource.Play();
    }

    public void StopRunLoop()
    {
        if (loopSource == null) return;         // ✅ prevents MissingReferenceException
        if (loopSource.isPlaying) loopSource.Stop();
    }

    // ---------- Helpers ----------
    private void PlayOneShot(AudioClip clip)
    {
        if (!clip || sfxSource == null) return;

        float oldPitch = sfxSource.pitch;
        sfxSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        sfxSource.PlayOneShot(clip, sfxVolume);
        sfxSource.pitch = oldPitch;
    }

    private IEnumerator ResetPitchAfter(float seconds, float oldPitch, AudioClip oldClip)
    {
        yield return new WaitForSeconds(seconds);
        if (sfxSource == null) yield break; // ✅ safety check
        sfxSource.pitch = oldPitch;
        if (sfxSource.clip == reload && !sfxSource.isPlaying)
            sfxSource.clip = oldClip;
        pitchResetCo = null;
    }
}
