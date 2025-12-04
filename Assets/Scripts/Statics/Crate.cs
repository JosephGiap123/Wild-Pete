using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;
public class Crate : BreakableStatics
{
    [SerializeField] GameObject particleEmitter;
    [SerializeField] DropItemsOnDeath dropItemsOnDeath;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    protected override void Awake()
    {
        base.Awake();
        if (!sfxSource)
        {
            sfxSource = GetComponent<AudioSource>();
        }
        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 1f;
            sfxSource.rolloffMode = AudioRolloffMode.Linear;
        }
    }
    public override void Damage(int dmg, Vector2 knockbackForce)
    {
        if (isInvincible) return;
        health -= dmg;
        Debug.Log(health);
        PlayHitSound();
        rb.linearVelocity += knockbackForce * 0.8f;
        StartCoroutine(DamageFlash(0.2f));
        if (health <= 0)
        {
            //run some code
            PlayBreakSound();
            Instantiate(particleEmitter, transform.position, Quaternion.identity);
            dropItemsOnDeath.DropItems();
            Break();
        }
    }
    protected override void PlayHitSound()
    {
        if (!hitSound) return;
        if (!sfxSource) return;
        sfxSource.PlayOneShot(hitSound, hitVolume);
    }
    protected void PlayBreakSound()
    {
        var temp = new GameObject($"LockPickAudio_{breakSound.name}");
        var tempSource = temp.AddComponent<AudioSource>();
        tempSource.playOnAwake = false;
        tempSource.outputAudioMixerGroup = sfxMixerGroup;
        tempSource.loop = false;
        tempSource.spatialBlend = 0f;
        tempSource.clip = breakSound;
        tempSource.Play();
        Object.Destroy(temp, breakSound.length);
        return;
    }
}