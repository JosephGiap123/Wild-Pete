using UnityEngine;
using System.Collections;

public class BomberBoss : EnemyBase, IHasFacing
{

    [Header("Animations")]
    [SerializeField] private Animator anim;
    private string currentState = "Idle";

    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] private BoxCollider2D groundCheckBox;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat Setting")]
    public int phaseNum = 1;
    protected bool isInAir = false;
    protected bool inAttackState = true;
    protected int isAttacking = 0;
    protected bool isInvincible = true;

    [Header("Combat Stats")]


    [Header("Combat References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private BoxCollider2D boxAttackHitbox;
    [SerializeField] private GenericAttackHitbox attackHitboxScript;
    [SerializeField] private GameObject phaseChangeParticles;
    protected float distanceToPlayer = 100f;


    [SerializeField] BossHPBarInteractor hpBarInteractor;
    private Transform player;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void HandlePlayerSet(GameObject playerObj)
    {
        if (playerObj != null) this.player = playerObj.transform;
    }

    public void Update()
    {
        if (player != null)
        {
            distanceToPlayer = Vector2.Distance(player.position, transform.position);
        }
    }

    protected void AnimationControl()
    {
        if (inAttackState || isDead) return;
        else if (isInAir)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                ChangeAnimationState("Rising");
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                ChangeAnimationState("Falling");
            }
            // UpdateRunLoopAudio(false);
            return;
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.25f)
            {
                ChangeAnimationState("Run");
            }
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.2f)
            {
                ChangeAnimationState("Walk");
            }
            else
            {
                ChangeAnimationState("Idle");
            }
        }
    }

    protected void ChangeAnimationState(string newState)
    {
        if (newState == currentState) return;
        anim.Play(newState, 0, 0f);
        currentState = newState;
    }

    void IsGroundedCheck()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheckBox.bounds.center, groundCheckBox.bounds.size, 0f, groundLayer);
        isInAir = colliders.Length == 0;
    }

    private void MoveTowards(Vector2 target, float speed = -1f)
    {
        if (speed < 0) speed = moveSpeed;
    }

    // private void UpdateRunLoopAudio(bool shouldRun)
    // {
    //     if (audioManager != null)
    //     {
    //         if (shouldRun) audioManager.StartRunLoop();
    //         else audioManager.StopRunLoop();
    //     }
    // }

    public override void Hurt(int dmg, Vector2 knockbackForce)
    {
        if (isInvincible) return;
        // audioManager?.PlayHurt();
        StartCoroutine(base.DamageFlash(0.2f));
        health -= dmg;
        hpBarInteractor.UpdateHealthVisual();
    }

    public void FaceTowardsPlayer()
    {
        if (isFacingRight && player.position.x < transform.position.x || !isFacingRight && player.position.x > transform.position.x)
        {
            FlipSprite();
        }
    }

    protected IEnumerator Death()
    {
        isDead = true;
        // audioManager?.StopRunLoop();
        // audioManager?.PlayDeath();
        ZeroVelocity(); // Stop all movement
        dropItemsOnDeath.DropItems();
        yield return new WaitForSeconds(4f); //wait for death animation to finish
        hpBarInteractor.ShowHealthBar(false);
        Die();
    }

    protected override void Die()
    {
        base.Die();
    }

    public override void Respawn(Vector2? position = null, bool? facingRight = null)
    {
        base.Respawn(position, facingRight);
    }

    public override void FlipSprite()
    {
        base.FlipSprite();
    }

    public void EndAttack()
    {
        inAttackState = false;
        isAttacking = 0;
    }

    public void ChangeAttackNumber(int newAttackNum)
    {
        isAttacking = newAttackNum;
    }

    public void EndAttackState()
    {
        inAttackState = false;
    }

    public void ActivateHitbox()
    {
        boxAttackHitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        boxAttackHitbox.enabled = false;
    }

    public void AddVelocity(float addedVelocity)
    {
        rb.linearVelocity += new Vector2(addedVelocity, 0f);
    }

    public void ZeroVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    // public void DecideAttack()
    // {
    //     if (distanceToPlayer <= 1.5f)
    // }

    public void SetUpAttackHitboxes(int attackNum)
    {
        attackHitboxScript.CustomizeHitbox(attackHitboxes[attackNum]);
    }


}
