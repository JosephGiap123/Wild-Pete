using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableStatics : MonoBehaviour
{

    [Header("Static Info")]
    [SerializeField] protected int health = 10;

    [Header("References")]
    // [SerializeField] protected Animator animator;
    [SerializeField] protected Rigidbody2D rb;

    [SerializeField] protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        sr.material = new Material(sr.sharedMaterial); // duplicate the base material
    }

    public virtual void Damage(int dmg, Vector2 knockbackForce)
    {
        health -= dmg;
        Debug.Log(health);
        StartCoroutine(DamageFlash(0.2f));
        if (health <= 0)
        {
            //run some code
            Break();
        }
    }

    protected virtual void Break()
    {
        Destroy(gameObject);
    }

    public virtual IEnumerator DamageFlash(float duration)
    {
        sr.material.SetFloat("_FlashAmount", 1f);
        yield return new WaitForSeconds(duration);
        sr.material.SetFloat("_FlashAmount", 0f);
    }
}