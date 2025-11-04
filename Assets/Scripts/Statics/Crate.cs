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
    }

    public override void Damage(int dmg, Vector2 knockbackForce)
    {
        health -= dmg;
        Debug.Log(health);

        rb.linearVelocity += knockbackForce * 0.8f;
        StartCoroutine(DamageFlash(0.2f));
        if (health <= 0)
        {
            //run some code
            Instantiate(particleEmitter, transform.position, Quaternion.identity);
            dropItemsOnDeath.DropItems();
            Break();
        }
    }

}
