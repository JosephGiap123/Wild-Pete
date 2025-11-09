using UnityEngine;

public class LockPickAudioManager : MonoBehaviour
{
    [Header("Audio Source (optional)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private AudioClip completedSound;

    [Header("Settings")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Awake()
    {
        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f; // UI/2D
        }
    }

    public void PlaySuccessSound(bool detach = false) => PlayClip(successSound, detach);
    public void PlayFailSound(bool detach = false) => PlayClip(failSound, detach);
    public void PlayCompletedSound(bool detach = false) => PlayClip(completedSound, detach);

    private void PlayClip(AudioClip clip, bool detach)
    {
        if (!clip) return;

        if (detach)
        {
            var temp = new GameObject($"LockPickAudio_{clip.name}");
            var tempSource = temp.AddComponent<AudioSource>();
            tempSource.playOnAwake = false;
            tempSource.loop = false;
            tempSource.spatialBlend = 0f;
            tempSource.volume = sfxVolume;
            tempSource.clip = clip;
            tempSource.Play();
            Object.Destroy(temp, clip.length);
            return;
        }

        if (!sfxSource) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
