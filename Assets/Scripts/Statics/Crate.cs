using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class Crate : BreakableStatics
{
    [SerializeField] GameObject particleEmitter;
    [SerializeField] DropItemsOnDeath dropItemsOnDeath;
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
        if (!breakSound) return;
        if (!sfxSource) return;
        sfxSource.PlayOneShot(breakSound, breakVolume);
    }
}