using UnityEngine;
using System.Collections;

public class SkeletonMinerAI : PatrolEnemyAI
{
    [Header("Combat Settings")]
    private int isAttacking = 0; // 0 = no attack, 1 = melee
    [SerializeField] private float meleeRange = 1f; // Range at which miner attacks
    [SerializeField] private float meleeAttackCooldown = 1.5f;
    private float meleeTimer = 0f;

    [Header("Ground Check")]
    // Note: groundCheckBox, groundLayer, and isInAir are now inherited from base class

    [Header("Attack References")]
    [SerializeField] private GenericAttackHitbox attackHitboxScript;

    [Header("Animation References")]
    [SerializeField] Animator anim;
    private string currentAnimationState = "Idle";

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
        // Call base Update for jump timer and ground checking
        base.Update();
        
        AnimationControl();
        if (isDead || isHurt) return;

        // Update attack timer
        meleeTimer -= Time.deltaTime;

        // Use base class state machine
        bool canSee = CanSeePlayer();
        switch (currentState)
        {
            case PatrolState.Idle: HandleIdle(canSee); break;
            case PatrolState.Patrol: HandlePatrol(canSee); break;
            case PatrolState.Alert: HandleAlert(canSee); break;
        }

        // Check for melee attack if in Alert state and close enough (after state machine handles movement)
        if (currentState == PatrolState.Alert && isAttacking == 0 && meleeTimer <= 0f)
        {
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= meleeRange)
                {
                    // Attack!
                    TryMeleeAttack();
                }
            }
        }
    }
    public void ChangeAnimationState(string newState)
    {
        if (newState == currentAnimationState) return;
        anim.Play(newState, 0, 0f);
        currentAnimationState = newState;
    }

    // Override HandleIdle to ensure patrol starts if points are set
    protected override void HandleIdle(bool canSee)
    {
        base.HandleIdle(canSee);
        // Base class handles transition to Patrol if patrol points exist
    }

    // Override HandlePatrol to ensure it works correctly
    protected override void HandlePatrol(bool canSee)
    {
        // If no patrol points, go back to idle
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            currentState = PatrolState.Idle;
            return;
        }

        // Call base HandlePatrol
        base.HandlePatrol(canSee);
    }

    // Override HandleAlert to add melee attack logic
    protected override void HandleAlert(bool canSee)
    {
        // If currently attacking, stop movement
        if (isAttacking != 0)
        {
            StopMoving();
            return;
        }

        // Call base HandleAlert for lose sight logic
        if (!canSee)
        {
            loseSightTimer += Time.deltaTime;

            // While searching, try to turn around to find player
            if (player != null && loseSightTimer < loseSightTime)
            {
                // Check if player is behind us
                Vector2 directionToPlayer = player.position - transform.position;
                bool playerIsBehind = (isFacingRight && directionToPlayer.x < 0) || (!isFacingRight && directionToPlayer.x > 0);

                if (playerIsBehind)
                {
                    // Turn around to look for player
            FlipSprite();
    }
            }

            if (loseSightTimer >= loseSightTime)
            {
                loseSightTimer = 0f;
                StopMoving();
                currentState = PatrolState.Idle;
                return;
            }
        }
        else
        {
            loseSightTimer = 0f;
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Movement logic
        if (distanceToPlayer > meleeRange)
    {
            // Not in melee range - keep chasing
            MoveTowards(player.position, alertSpeed);
        }
        else
        {
            // In melee range - stop and attack
            StopMoving();
        }
    }

    private void TryMeleeAttack()
    {
        if (isAttacking != 0 || meleeTimer > 0f) return;

        FaceTowardsPlayer();
        SetUpAttackHitbox(1);

        // Apply random cooldown multiplier (0.8-1.0x) like GuardAI
        float randomMultiplier = Random.Range(0.8f, 1.0f);
        meleeTimer = meleeAttackCooldown * randomMultiplier;
    }

    public void FaceTowardsPlayer()
    {
        if (player == null) return;
        if (isFacingRight && player.position.x < transform.position.x || !isFacingRight && player.position.x > transform.position.x)
        {
            FlipSprite();
        }
    }
    protected void AnimationControl()
    {
        if (isDead)
        {
            ChangeAnimationState("Death");
            return;
        }
        else if (isAttacking != 0 || isHurt)
        {
            //played outside of animcontrol
            return;
        }
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
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.2f)
            {
                ChangeAnimationState("Walk");
            }
            else
            {
                ChangeAnimationState("Idle");
            }
        }
    }

    protected override void IsGroundedCheck()
    {
        base.IsGroundedCheck(); // Base class handles the actual ground checking
    }

    //attacks
    public void SetUpAttackHitbox(int attackNum)
    {
        switch (attackNum)
        {
            case 1: //melee
                isAttacking = 1;
                attackHitboxScript.CustomizeHitbox(attackHitboxes[0]);
                ChangeAnimationState("MeleeAttack");
                break;
            default:
                break;
        }
    }

    public void EndAttack()
    {
        isAttacking = 0;
    }

    public override void Hurt(int dmg, Vector2 knockbackForce)
    {
        if (isDead) return;

        // Check if this hit will kill the enemy
        bool willDie = (health - dmg) <= 0;

        // Immediately stop current movement, then apply knockback so stun halts momentum
        StopMoving();
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.linearVelocity += knockbackForce;

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
        StopMoving(); // Stop all movement

        if (dropItemsOnDeath != null)
        {
            dropItemsOnDeath.DropItems();
        }

        yield return new WaitForSeconds(2f); // Wait for death animation to finish

        base.Die();
    }

    private void StartHurtAnim(int dmg)
    {
        float damageFlashTime = Mathf.Max(0.2f, (dmg / (float)maxHealth));
        isHurt = true;
        ChangeAnimationState("Hurt");
        StartCoroutine(DamageFlash(damageFlashTime));
    }

    public override void EndHurtState()
    {
        isHurt = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public override void Respawn(Vector2? position = null, bool? facingRight = null)
    {
        base.Respawn(position, facingRight);

        // Reset all AI state variables
        isDead = false;
        isHurt = false;
        isAttacking = 0;

        // Reset movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Stop any active coroutines
        StopAllCoroutines();

        // Reset animation state
        currentAnimationState = "Idle";
        if (anim != null)
        {
            anim.Play("Idle");
        }

        // Reset attack timer
        meleeTimer = 0f;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // Draw base gizmos (detection range, patrol points, raycasts)

        // Draw melee attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
