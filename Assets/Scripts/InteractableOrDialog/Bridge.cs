using UnityEngine;
using System.Collections;

public class Bridge : MonoBehaviour
{
    public bool isUp = false;
    public Animator anim;
    [Header("Audio")]
    [SerializeField] private AudioSource bridgeAudioSource;
    [SerializeField] private AudioClip raiseClip;
    [SerializeField, Range(0f, 2f)] private float raiseVolume = 1f;
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
        PlayRaiseSoundLoop();
        StartCoroutine(EndRaiseBridge());
    }

    public void LowerBridge()
    {
        isUp = false;
        anim.Play("BridgeDown");
    }

    private void PlayRaiseSoundLoop()
    {
        if (raiseClip == null || bridgeAudioSource == null) return;
        bridgeAudioSource.clip = raiseClip;
        bridgeAudioSource.PlayOneShot(raiseClip, raiseVolume);
    }

    private IEnumerator EndRaiseBridge()
    {
        yield return null;
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        StopBridgeRaiseSoundLoop();
    }
    public void StartBridgeRaiseSoundLoop()
    {
        if (!raiseClip || bridgeAudioSource == null || bridgeAudioSource.isPlaying) return;
        bridgeAudioSource.Play();
    }

    public void StopBridgeRaiseSoundLoop()
    {
        if (bridgeAudioSource != null && bridgeAudioSource.isPlaying) bridgeAudioSource.Stop();
    }
}
