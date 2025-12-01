using UnityEngine;

public class ScrewAudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip screwLoopClip;
    [SerializeField] private AudioClip wireConnectClip;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayWireConnect()
    {
        if (!wireConnectClip)
        {
            Debug.LogWarning("[ScrewAudioManager] wireConnectClip is not assigned!");
            return;
        }

        if (audioSource)
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(wireConnectClip);
            Debug.Log("[ScrewAudioManager] Wire connect sound played");
        }
    }

    public void StartScrewLoop()
    {
        if (!screwLoopClip)
        {
            Debug.LogError("[ScrewAudioManager] screwLoopClip is not assigned!");
            return;
        }

        if (audioSource)
        {
            audioSource.clip = screwLoopClip;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("[ScrewAudioManager] Screw loop started");
        }
    }

    public void StopScrewLoop()
    {
        if (audioSource && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("[ScrewAudioManager] Screw loop stopped");
        }
    }
}
