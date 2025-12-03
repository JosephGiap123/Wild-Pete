using UnityEngine;

public class Bridge : MonoBehaviour
{
    public bool isUp = false;
    public Animator anim;
    [Header("Audio")]
    [SerializeField] private AudioSource bridgeAudioSource;
    [SerializeField] private AudioClip raiseClip;
    [SerializeField] private AudioClip lowerClip;
    [SerializeField, Range(0f, 2f)] private float raiseVolume = 1f;
    [SerializeField, Range(0f, 2f)] private float lowerVolume = 1f;
    public void Awake()
    {
        anim.Play(isUp ? "BridgeUp" : "BridgeDown");
        if (!bridgeAudioSource) bridgeAudioSource = GetComponent<AudioSource>();
        if (!bridgeAudioSource) bridgeAudioSource = gameObject.AddComponent<AudioSource>();
        bridgeAudioSource.playOnAwake = false;
        bridgeAudioSource.spatialBlend = 1f;
        bridgeAudioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    public void RaiseBridge()
    {
        isUp = true;
        anim.Play("BridgeRise");
        PlayRaiseSound();
    }

    public void LowerBridge()
    {
        isUp = false;
        anim.Play("BridgeDown");
        PlayLowerSound();
    }

    private void PlayRaiseSound()
    {
        if (raiseClip == null || bridgeAudioSource == null) return;
        bridgeAudioSource.PlayOneShot(raiseClip, raiseVolume);
    }

    private void PlayLowerSound()
    {
        if (lowerClip == null || bridgeAudioSource == null) return;
        bridgeAudioSource.PlayOneShot(lowerClip, lowerVolume);
    }
}
