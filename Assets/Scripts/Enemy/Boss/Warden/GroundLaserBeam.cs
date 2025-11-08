using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GroundLaserBeam : MonoBehaviour
{
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask staticMask;
    [SerializeField] private BoxCollider2D laserHitbox;
    [SerializeField] private Animator anim;
    protected int damage = 1;
    public bool facingRight = true;
    protected Vector2 knockbackForce = new Vector2(0f, 0f);
    [Header("Hit Behavior")]
    [SerializeField] private bool disableAfterFirstHit = false; // optional single-hit behavior
    private readonly HashSet<GameObject> alreadyHit = new HashSet<GameObject>();
    public void Initialize(Vector3 position, int damage, Vector2 knockbackForce, float timeToSpawn)
    {
        transform.position = position;
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        StartCoroutine(WaitCoRoutine(timeToSpawn));
    }

    public IEnumerator WaitCoRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        anim.Play("LaserBeamRise", 0, 0f);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerMask) != 0) //bitshifting to find if sometihng is in said layer
        {
            if (collision.gameObject != null)
            {
                GameObject targetRoot = collision.transform.parent.gameObject;
                if (alreadyHit.Contains(targetRoot)) return; // prevent multiple hits on same target during this activation
                alreadyHit.Add(targetRoot);
                Debug.Log("Hit player");
                targetRoot.GetComponent<BasePlayerMovement2D>().HurtPlayer(damage, knockbackForce, facingRight ? 1f : -1f);
                Debug.Log($"{(facingRight ? 1f : -1f)} {knockbackForce}");
                if (disableAfterFirstHit) DisableHitbox();
            }
        }
        else if (((1 << collision.gameObject.layer) & staticMask) != 0)
        {
            if (collision.gameObject != null)
            {
                GameObject targetRoot = collision.transform.parent.gameObject;
                if (alreadyHit.Contains(targetRoot)) return;
                alreadyHit.Add(targetRoot);
                Debug.Log("Hit static");
                targetRoot.GetComponent<BreakableStatics>().Damage(damage, new Vector2((facingRight ? 1f : -1f) * knockbackForce.x, knockbackForce.y));
                if (disableAfterFirstHit) DisableHitbox();
            }
        }
    }


    public void DestroyLaser()
    {
        Debug.Log("Destroyed");
        Destroy(gameObject);
    }
    void DisableHitbox()
    {
        laserHitbox.enabled = false;
    }
}
