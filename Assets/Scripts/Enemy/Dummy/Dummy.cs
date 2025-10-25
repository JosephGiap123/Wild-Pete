using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Dummy : EnemyBase
{
    [SerializeField] Animator anim;
    private bool hurtStun = false;
    Coroutine hurtCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Hurt(int dmg)
    {
        health -= dmg;
        Debug.Log(health);

        if (hurtCoroutine != null)
            StopCoroutine(hurtCoroutine);

        hurtCoroutine = StartCoroutine(HurtAnim());

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator HurtAnim()
    {
        hurtStun = true;
        anim.Play("Hurt");
        StartCoroutine(DamageFlash(0.2f));
        yield return new WaitWhile(() => hurtStun);
        anim.Play("Idle");
    }

    public override void EndHurtState()
    {
        hurtStun = false;
    }

}