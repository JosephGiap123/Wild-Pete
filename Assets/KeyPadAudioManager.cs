using UnityEngine;

public class KeyPadAudioManager : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;

    private AudioSource audioSource;

    public AudioClip SuccessClip => successClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void PlayClick() => PlayOneShot(clickClip, "[KeyPadAudioManager] clickClip is not assigned!");
    public void PlaySuccess() => PlayOneShot(successClip, "[KeyPadAudioManager] successClip is not assigned!");
    public void PlayFail() => PlayOneShot(failClip, "[KeyPadAudioManager] failClip is not assigned!");

    // Use when the UI is about to be hidden/disabled; plays on a temporary GameObject so it won't cut off.
    public void PlaySuccessAtPosition(Vector3 position)
    {
        if (!successClip)
        {
            Debug.LogWarning("[KeyPadAudioManager] successClip is not assigned!");
            return;
        }
        AudioSource.PlayClipAtPoint(successClip, position);
    }

    private void PlayOneShot(AudioClip clip, string warnMessage)
    {
        if (!clip)
        {
            Debug.LogWarning(warnMessage);
            return;
        }

        if (audioSource) audioSource.PlayOneShot(clip);
    }
}
