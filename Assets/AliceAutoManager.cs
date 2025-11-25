using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AliceAudioManager : MonoBehaviour
{
    [Header("Audio Source (auto-created if empty)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip shotgun;      // primary fire
    [SerializeField] private AudioClip reload;       // will play at 2x speed
    [SerializeField] private AudioClip[] hammerClips; // <-- assign 2+ swing sounds here
    [SerializeField] private AudioClip sweep;
    [SerializeField] private AudioClip dash;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip hurt;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip runLoop;      // continuous footsteps loop
    [SerializeField] private AudioClip punch;
    [SerializeField] private AudioClip slide;

    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Tuning")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Tooltip("Random pitch +/- around 1.0 for variation on one-shots (not used for Reload)")]
    [Range(0f, 0.3f)] public float pitchJitter = 0.05f;
    [Tooltip("3D distance where sound fully fades out")]
    public float max3dDistance = 20f;

    // Separate source for continuous loop (run/engine hum etc.)
    private AudioSource loopSource;

    // For 2x reload reset
    private Coroutine pitchResetCo;

    // To avoid repeating the same hammer clip twice
    private int lastHammerIndex = -1;

    void Awake()
    {
        if (!sfxSource)
            sfxSource = gameObject.AddComponent<AudioSource>();

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

    // ---------- Public hooks (call from scripts or Animation Events) ----------
    public void PlayShotgun() => PlayOneShot(shotgun);

    // Randomized hammer: pick from hammerClips[], avoid immediate repeat
    public void PlayHammer()
    {
        AudioClip clip = null;

        if (hammerClips != null && hammerClips.Length > 0)
        {
            if (hammerClips.Length == 1)
            {
                clip = hammerClips[0];
            }
            else
            {
                int idx;
                // pick until different from last (max a few tries to be safe)
                int tries = 0;
                do
                {
                    idx = Random.Range(0, hammerClips.Length);
                    tries++;
                } while (idx == lastHammerIndex && tries < 5);

                lastHammerIndex = idx;
                clip = hammerClips[idx];
            }
        }

        // If nothing assigned in the array, nothing plays (safe no-op)
        PlayOneShot(clip);
    }

    public void PlayDash() => PlayOneShot(dash);
    public void PlaySweep() => PlayOneShot(sweep);
    public void PlayJump() => PlayOneShot(jump);
    public void PlayHurt() => PlayOneShot(hurt);
    public void PlayDeath() => PlayOneShot(death);
    public void PlaySlide() => PlayOneShot(slide);

    // Reload at exactly 2x speed (pitch up). Robust against OneShot pitch quirks.
    public void PlayReload()
    {
        if (!reload || sfxSource == null) return;

        if (pitchResetCo != null) StopCoroutine(pitchResetCo);

        sfxSource.Stop();                 // clean start
        float oldPitch = sfxSource.pitch;
        var oldClip = sfxSource.clip;

        sfxSource.pitch = 2f;            // 2× speed (and pitch)
        sfxSource.clip = reload;
        sfxSource.volume = sfxVolume;
        sfxSource.Play();

        float playTime = reload.length / 2f; // half the original duration at 2×
        pitchResetCo = StartCoroutine(ResetPitchAfter(playTime, oldPitch, oldClip));
    }

    // Continuous run loop control
    public void StartRunLoop()
    {
        if (!runLoop || loopSource == null || loopSource.isPlaying) return;
        loopSource.clip = runLoop;
        loopSource.pitch = 1f;
        loopSource.volume = sfxVolume;
        loopSource.Play();
    }

    public void StopRunLoop()
    {
        if (loopSource != null && loopSource.isPlaying) loopSource.Stop();
    }

    public void SetSfxVolume(float value01)
    {
        sfxVolume = Mathf.Clamp01(value01);
        if (loopSource != null && loopSource.isPlaying) loopSource.volume = sfxVolume;
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
        if (sfxSource != null)
        {
            sfxSource.pitch = oldPitch;
            if (sfxSource.clip == reload && !sfxSource.isPlaying)
                sfxSource.clip = oldClip;
        }
        pitchResetCo = null;
    }
}
