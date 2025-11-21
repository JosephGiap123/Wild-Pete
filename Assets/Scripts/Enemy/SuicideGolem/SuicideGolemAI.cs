using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
public class SuicideGolemAI : PatrolEnemyAI
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRange = 1f; // Range at which golem explodes

    [Header("Ground Check")]
    private bool isInAir = false;
    public LayerMask groundLayer;
    [SerializeField] private BoxCollider2D groundCheckBox;

    [Header("Combat Settings")]
    private int isAttacking = 0; // 0 = no attack, 1 = exploding
    private bool isInvincible = false;
    private bool isExploding = false; // Track if explosion sequence has started
    [Header("Attack References")]
    [SerializeField] private GenericAttackHitbox attackHitboxScript;

    [Header("Animation References")]
    [SerializeField] Animator anim;
    private string currentAnimationState = "Idle";

    [Header("Camera Shake")]
    private CinemachineImpulseSource impulseSource;

    protected override void Awake()
    {
        base.Awake();
        impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // Handles GameManager.OnPlayerSet

        // Try to register with CheckpointManager again (in case it wasn't ready during Awake())
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RegisterEnemy(this);
        }
    }

    public void Update()
    {
        AnimationControl();
        if (isDead || isExploding) return;
        IsGroundedCheck();

        // Check for explosion first (regardless of state)
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= explosionRange && isAttacking == 0)
            {
                // Explode!
                StartExplosion();
                return;
            }
        }

        // Use base class state machine
        bool canSee = CanSeePlayer();
        switch (currentState)
        {
            case PatrolState.Idle: HandleIdle(canSee); break;
            case PatrolState.Patrol: HandlePatrol(canSee); break;
            case PatrolState.Alert: HandleAlert(canSee); break;
        }
    }
    public void ChangeAnimationState(string newState)
    {
        if (newState == currentAnimationState) return;
        anim.Play(newState, 0, 0f);
        currentAnimationState = newState;
    }

    protected virtual void HandleFlip()
    {
        if (isAttacking != 0 || isDead) return;
        if ((rb.linearVelocity.x > 0.25f && !isFacingRight) || (rb.linearVelocity.x < 0.25f && isFacingRight))
            FlipSprite();
    }
    public override void FlipSprite()
    {
        base.FlipSprite(); // Handle isFacingRight and sprite flip
    }
    public void FaceTowardsPlayer()
    {
        if (isFacingRight && player.position.x < transform.position.x || !isFacingRight && player.position.x > transform.position.x)
        {
            FlipSprite();
        }
    }


    public override void Hurt(int dmg, Vector2 knockbackForce)
    {
        if (isInvincible || isDead) return;

        // Check if this hit will kill the enemy
        bool willDie = (health - dmg) <= 0;

        // Immediately stop current movement, this enemy does not take knockback
        StopMoving();
        // Clear ongoing attack intent
        isAttacking = 0;

        StartHurtAnim(dmg);

        // If this will kill the enemy, start death coroutine BEFORE calling base.Hurt
        // Set isDead = true first so base.Hurt() doesn't call Die() (which deactivates GameObject)
        if (willDie)
        {
            isDead = true; // Prevent base.Hurt() from calling Die()
            StartCoroutine(Death());
        }

        // Call base Hurt (handles health, damage text, and sets Alert state)
        base.Hurt(dmg, knockbackForce);
    }

    protected IEnumerator Death()
    {
        isDead = true;
        ChangeAnimationState("Death");
        StopMoving(); // Stop all movement

        if (dropItemsOnDeath != null)
        {
            dropItemsOnDeath.DropItems();
        }

        yield return new WaitForSeconds(3f); // Wait for death animation to finish

        base.Die();
    }

    private void StartHurtAnim(int dmg)
    {
        float damageFlashTime = Mathf.Max(0.2f, (dmg / (float)maxHealth));
        StartCoroutine(DamageFlash(damageFlashTime));
    }
    protected void AnimationControl()
    {
        // Don't change animation if exploding or dead (handled elsewhere)
        if (isAttacking != 0 || isDead || isExploding)
        {
            return;
        }

        // Set animation based on movement
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                ChangeAnimationState("Run");
            }
            else
            {
                ChangeAnimationState("Idle");
        }
    }

    void IsGroundedCheck()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheckBox.bounds.center, groundCheckBox.bounds.size, 0f, groundLayer);
        bool wasInAir = isInAir;
        isInAir = colliders.Length == 0;
    }

    // Legacy method - kept for compatibility but explosion is now triggered automatically
    public void SetUpAttackHitbox(int attackNum)
    {
        if (attackNum == 1 && !isExploding)
        {
            StartExplosion();
        }
    }


    private void StartExplosion()
    {
        if (isExploding || isAttacking != 0 || isDead) return; // Already exploding or dead

        isExploding = true;
        isAttacking = 1;
        StopMoving();
        FaceTowardsPlayer();

        // Set up explosion hitbox
        if (attackHitboxScript != null && attackHitboxes != null && attackHitboxes.Length > 0)
        {
                attackHitboxScript.CustomizeHitbox(attackHitboxes[0]);
        }
                ChangeAnimationState("Explode");
    }

    // Public method called by animation event at the end of Explode animation
    public IEnumerator ExplosionDeathSequence()
    {

        // Wait a bit for visual effect
        yield return new WaitForSeconds(2f);

        // Die after explosion (if not already dead)
        if (!isDead)
        {
            isDead = true;
            //doesnt drop items if it explodes
            base.Die();
        }
    }

    public void EndAttack()
    {
        isAttacking = 0;
    }

    public void SetInvincible()
    {
        isInvincible = true;
    }
    public void EndInvincible()
    {
        isInvincible = false;
    }
    public void ImpulsePlayer()
    {
        impulseSource.GenerateImpulse(1.5f);
    }



    public override void Respawn(Vector2? position = null, bool? facingRight = null)
    {
        base.Respawn(position, facingRight); // Base handles patrol state reset

        isDead = false;
        isAttacking = 0;
        isInvincible = false;
        isExploding = false;

        // Reset movement
        if (rb != null)
    {
            rb.linearVelocity = Vector2.zero;
        }

        StopAllCoroutines();

        currentAnimationState = "Idle";
        if (anim != null)
        {
            anim.Play("Idle");
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= explosionRange)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
    }
}
