using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class RespawnAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip interactSound;

    private void Awake()
    {
        if (!sfxSource) sfxSource = GetComponent<AudioSource>();
    }

    public void PlayInteractSound()
    {
        if (sfxSource && interactSound)
        {
            sfxSource.PlayOneShot(interactSound);
        }
    }
}
