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

    public override void Hurt(int dmg, Vector2 knockback)
    {
        health -= dmg;
        Debug.Log(health);

        if (hurtCoroutine != null)
            StopCoroutine(hurtCoroutine);

        if (damageText != null)
        {
            GameObject dmgText = Instantiate(damageText, transform.position, transform.rotation);
            dmgText.GetComponentInChildren<DamageText>().Initialize(new(knockback.x, 5f), dmg, new Color(0.8862745f, 0.3660145f, 0.0980392f, 1f), Color.red);
        }
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

    public override void Respawn(Vector2? position = null, bool? facingRight = null)
    {
        base.Respawn(position, facingRight);
        
        // Reset state variables
        hurtStun = false;
        
        // Stop any active coroutines
        if (hurtCoroutine != null)
        {
            StopCoroutine(hurtCoroutine);
            hurtCoroutine = null;
        }
        StopAllCoroutines();
        
        // Reset animation state
        if (anim != null)
        {
            anim.Play("Idle");
        }
        
        // Reset movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

}