using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAI : PatrolEnemyAI
{
    private enum GuardAttackType { None = 0, Melee = 1, Melee2 = 2, Ranged = 3, Dash = 4 }
    private GuardAudioManager audioManager;

    // Guard-specific states beyond base class states
    private enum GuardState
    {
        Attack,
        Return
    }

    [SerializeField] private GuardState guardCurrentState = GuardState.Attack;
    private bool isInGuardSpecificState = false; // Track if we're in Attack or Return state

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false; // Toggle this to see debug logs

    [Header("Animations")]
    [SerializeField] private Animator anim;
    private string currentAnimationState = "Idle";

    [Header("Guard Movement Settings")]
    [SerializeField] private float jumpForce = 5f;
    // Note: groundCheckBox, groundLayer, and isInAir are now inherited from base class

    [Header("Combat Settings")]
    private int isAttacking = 0; // 0 = no attack, 1 = melee1, 2 = melee2, 3 = ranged, 4 = dash attack
    private int attackChain = 0;
    [SerializeField] private float meleeRange = 1f;
    [SerializeField] private float dashRange = 5f;

    [Header("Attack Settings")]
    [SerializeField] private int rangedDamage = 2;
    [SerializeField] private float rangedAttackCooldown = 5f;
    [SerializeField] private float bulletSpeed = 14f;
    [SerializeField] private float bulletLifeTime = 3f;
    [SerializeField] private float dashAttackSpeed = 8f;

    [Header("Attack Cooldowns (individual)")]
    [SerializeField] private float melee1AttackCooldown = 1.0f;
    [SerializeField] private float melee2AttackCooldown = 1.2f;
    [SerializeField] private float dashAttackCooldown = 3.0f;

    [Header("Attack Selection (weights/ranges)")]
    [SerializeField] private float meleeWeight = 1.0f;
    [SerializeField] private float dashWeight = 0.9f;
    [SerializeField] private float rangedWeight = 1.1f;
    [SerializeField] private float selectionInterval = 0.2f;
    [SerializeField] private float repeatAttackPenalty = 0.35f; // penalty multiplier if repeating last attack
    [SerializeField] private float idealRangedMultiplier = 1.0f; // extra boost when at ideal ranged distance

    [Header("Combat References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GenericAttackHitbox attackHitboxScript; // Uses GenericAttackHitbox system

    [Header("Detection Settings")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Settings (timers)")]
    [SerializeField] private float attackCooldown = 1f; // seconds between attacks
    private float attackTimer = 0f;
    private float rangedTimer = 0f;
    private float melee1Timer = 0f;
    private float melee2Timer = 0f;
    private float dashTimer = 0f;
    private float selectTimer = 0f;

    private GuardAttackType lastAttack = GuardAttackType.None;
    private bool chainMeleePending = false; // after melee1, force chaining into melee2



    protected override void Awake()
    {
        base.Awake(); // Calls EnemyBase.Awake()
        // Additional initialization if needed
        audioManager = GetComponent<GuardAudioManager>() ?? GetComponentInChildren<GuardAudioManager>();
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // Handles GameManager.OnPlayerSet
    }

    protected override void OnDisable()
    {
        base.OnDisable(); // Unsubscribes from GameManager.OnPlayerSet
    }

    // Override to enable facing direction checks
    protected override bool ShouldCheckFacingDirection()
    {
        return true;
    }

    private void Update()
    {
        // Call base Update for jump timer and ground checking
        base.Update();
        
        AnimationControl();

        // Stop processing AI logic if dead or hurt
        if (isDead || isHurt) return;

        // While performing non-dash attacks, ensure no horizontal drift
        if (isAttacking == 1 || isAttacking == 2 || isAttacking == 3)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        rangedTimer -= Time.deltaTime;
        melee1Timer -= Time.deltaTime;
        melee2Timer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        selectTimer -= Time.deltaTime;

        bool canSee = CanSeePlayer();

        // Debug current state (only if debug mode enabled)
        if (debugMode)
        {
            string stateName = isInGuardSpecificState
                ? (guardCurrentState == GuardState.Attack ? "Attack" : "Return")
                : currentState.ToString();
            Debug.Log($"GuardAI State: {stateName}, CanSeePlayer: {canSee}, Player: {(player != null ? player.name : "NULL")}, Attacking: {isAttacking}");
        }

        // Handle Guard-specific states first
        if (isInGuardSpecificState)
        {
            switch (guardCurrentState)
            {
            case GuardState.Attack: HandleAttack(); break;
            case GuardState.Return: HandleReturn(); break;
        }
    }
        else
        {
            // Use base class state machine
            switch (currentState)
        {
                case PatrolState.Idle: HandleIdle(canSee); break;
                case PatrolState.Patrol: HandlePatrol(canSee); break;
                case PatrolState.Alert: HandleAlert(canSee); break;
        }
        }
    }


    // Override base HandleIdle to add debug logging
    protected override void HandleIdle(bool canSee)
    {
        base.HandleIdle(canSee);
        if (debugMode && canSee)
        {
            Debug.Log("GuardAI Idle: Spotted player! Entering Alert state.");
        }
    }

    // Override base HandlePatrol to add debug logging
    protected override void HandlePatrol(bool canSee)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("GuardAI: No patrol points set! Switching to Idle.");
            currentState = PatrolState.Idle;
            return;
        }

        base.HandlePatrol(canSee);
        if (debugMode && canSee)
        {
            Debug.Log("GuardAI: Spotted player during patrol! Switching to Alert.");
        }
    }

    // Override base HandleAlert to add attack selection logic
    protected override void HandleAlert(bool canSee)
    {
        // If currently attacking, only allow dash to control velocity; otherwise halt movement
        if (isAttacking != 0)
        {
            if (isAttacking != 4) // not dash
            {
                StopMoving();
            }
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
                    if (debugMode) Debug.Log("GuardAI Alert: Player might be behind, turning around!");
                }
            }

            if (loseSightTimer >= loseSightTime)
            {
                loseSightTimer = 0f;
                StopMoving();
                isInGuardSpecificState = true;
                guardCurrentState = GuardState.Return;
                if (debugMode) Debug.Log("GuardAI Alert: Giving up search, returning to patrol.");
                return;
            }
        }
        else
        {
            loseSightTimer = 0f;
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(player.position, transform.position);

        // Use selector to decide if we should attack now (including ranged while chasing)
        if (isAttacking == 0 && attackTimer <= 0f)
        {
            TrySelectAndStartAttack(distanceToPlayer);
        }

        // Movement logic
        if (distanceToPlayer > meleeRange * 1.2f)
        {
            // Not in melee range - keep chasing
            MoveTowards(player.position, alertSpeed);
        }
        else
        {
            // In melee/dash range - stop and enter Attack state for close combat
            StopMoving();
            isInGuardSpecificState = true;
            guardCurrentState = GuardState.Attack;
            if (debugMode) Debug.Log($"GuardAI Alert: In melee/dash range ({distanceToPlayer:F2} units)! Switching to Attack state.");
        }
    }

    private void HandleAttack()
    {
        // Always stop movement in attack state
        if (isAttacking == 0)
        {
            StopMoving();
        }

        // Only decide to attack if not currently attacking
        if (isAttacking == 0 && attackTimer <= 0f)
        {
            float distanceToPlayer = (player != null) ? Vector2.Distance(player.position, transform.position) : Mathf.Infinity;
            TrySelectAndStartAttack(distanceToPlayer);
        }

        // Don't change states while actively attacking
        if (isAttacking != 0)
        {
            return;
        }

        // After attacking finishes, always return to Alert state to search for player
        // Don't immediately give up if we can't see them (they might be behind us)
        isInGuardSpecificState = false;
        currentState = PatrolState.Alert;
        loseSightTimer = 0f; // Reset timer so guard searches for a while
        if (debugMode) Debug.Log("GuardAI Attack: Attack finished, entering Alert state to search for player.");
    }


    private void HandleReturn()
    {
        // Check if we can see player again while returning
        if (CanSeePlayer())
        {
            if (debugMode) Debug.Log("GuardAI Return: Spotted player again! Re-entering Alert state.");
            isInGuardSpecificState = false;
            currentState = PatrolState.Alert;
            loseSightTimer = 0f;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            if (debugMode) Debug.LogWarning("GuardAI Return: No patrol points, switching to Idle.");
            isInGuardSpecificState = false;
            currentState = PatrolState.Idle;
            return;
        }

        Vector2 patrolTarget = patrolPoints[currentPatrolIndex];
        float distanceToTarget = Vector2.Distance(transform.position, patrolTarget);

        MoveTowards(patrolTarget); // Use normal patrol speed

        if (distanceToTarget < 1.0f) // Same tolerance as patrol for consistency
        {
            if (debugMode) Debug.Log("GuardAI Return: Reached patrol point, idling before resuming patrol.");
            isInGuardSpecificState = false;
            currentState = PatrolState.Idle;
            patrolWaitTimer = 0f; // Reset wait timer for idle period
        }
    }


    public override void Respawn(Vector2? position = null, bool? facingRight = null)
    {
        base.Respawn(position, facingRight); // Base handles patrol state reset

        // Reset Guard-specific state
        isDead = false;
        isHurt = false;
        isAttacking = 0;
        attackChain = 0;
        chainMeleePending = false;
        isInGuardSpecificState = false;
        guardCurrentState = GuardState.Attack;

        // Reset all timers
        attackTimer = 0f;
        rangedTimer = 0f;
        melee1Timer = 0f;
        melee2Timer = 0f;
        dashTimer = 0f;
        selectTimer = 0f;

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

        // Reset last attack
        lastAttack = GuardAttackType.None;

        if (debugMode) Debug.Log("GuardAI: Respawned and state reset");
    }

    protected IEnumerator Death()
    {
        isDead = true;
        StopMoving(); // Stop all movement
        if (debugMode) Debug.Log("GuardAI: Death coroutine started, playing death animation...");
        audioManager?.PlayDeath();
        dropItemsOnDeath.DropItems();
        yield return new WaitForSeconds(2f); //wait for death animation to finish

        if (debugMode) Debug.Log("GuardAI: Death animation finished, destroying object.");
        base.Die();
    }

    protected void Jump()
    {
        if (!isInAir && !isDead && !isHurt && isAttacking == 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isInAir = true;
        }
    }
    public void EndAttack()
    {
        isAttacking = 0;
        StopMoving();

        // If a melee chain is pending, slightly reduce selector delay to immediately pick melee2
        if (chainMeleePending)
        {
            attackTimer = 0f; // allow immediate selection
            selectTimer = 0f;
        }
    }

    public void InstBullet()
    {
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        GuardBullet projScript = projectile.GetComponent<GuardBullet>();
        projScript.Initialize(rangedDamage, bulletSpeed, bulletLifeTime);
        return;
    }
    public override void Hurt(int dmg, Vector2 knockbackForce)
    {
        if (isDead) return;

        // Immediately stop current movement, then apply knockback so stun halts momentum
        StopMoving();
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.linearVelocity += knockbackForce;

        if (audioManager != null)
        {
            if (health - dmg <= 0)
                audioManager?.PlayDeath();
            else
                audioManager?.PlayHurt();
        }

        // Clear ongoing/combo attack intent so we don't spam the same attack after stun
        isAttacking = 0;
        attackChain = 0;
        attackTimer = attackCooldown / 3; // small delay before next decision

        StartHurtAnim(dmg);

        // Exit guard-specific states when hurt
        isInGuardSpecificState = false;

        // Check if this hit will kill the enemy
        bool willDie = (health - dmg) <= 0;

        // If this will kill the enemy, start death coroutine BEFORE calling base.Hurt
        // Set isDead = true first so base.Hurt() doesn't call Die() (which deactivates GameObject)
        if (willDie)
                {
            isDead = true; // Prevent base.Hurt() from calling Die()
            StartCoroutine(Death());
        }

        // Call base Hurt (handles health, damage text, and sets Alert state)
        base.Hurt(dmg, knockbackForce);

        // Guard-specific logic after base.Hurt
        if (IsAlive())
        {
            if (debugMode) Debug.Log($"GuardAI: Hurt! Health: {health}, Entering Alert state!");
        }
    }
    public override void EndHurtState()
    {
        isHurt = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void StartHurtAnim(int dmg)
    {
        float damageFlashTime = Mathf.Max(0.2f, (dmg / maxHealth));
        isHurt = true;
        ChangeAnimationState("Hurt");
        StartCoroutine(DamageFlash(damageFlashTime));

    }

    protected override void IsGroundedCheck()
    {
        bool wasInAir = isInAir;
        base.IsGroundedCheck(); // Base class handles the actual ground checking
        
        // Play run loop sound when landing and moving
        if (wasInAir && !isInAir && Mathf.Abs(rb.linearVelocity.x) > 0.2f && audioManager != null)
        {
            audioManager?.StartRunLoop();
        }
    }

    public void ChangeAnimationState(string newState)
    {
        if (newState == currentAnimationState) return;
        anim.Play(newState, 0, 0f);
        currentAnimationState = newState;
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
            return;
        }
        else if (isInAir)
        {
            if (audioManager != null) audioManager?.StopRunLoop();
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
            if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed + 0.2f)
            {
                ChangeAnimationState("Run");
                if (audioManager != null) audioManager?.StartRunLoop();
            }
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.2f)
            {
                ChangeAnimationState("Walk");
                if (audioManager != null) audioManager?.StartRunLoop();
            }
            else
            {
                ChangeAnimationState("Idle");
                if (audioManager != null) audioManager?.StopRunLoop();
            }
        }
    }

    protected virtual void HandleFlip()
    {
        if (isAttacking != 0 || isHurt || isDead) return;
        if ((rb.linearVelocity.x > 0.25f && !isFacingRight) || (rb.linearVelocity.x < 0.25f && isFacingRight))
            FlipSprite();
    }

    public override void FlipSprite()
    {
        base.FlipSprite(); // Handle isFacingRight and sprite flip
        // Rotate projectile spawn point to match facing direction
        if (projectileSpawnPoint != null)
        {
            projectileSpawnPoint.localRotation = Quaternion.Euler(0, 0, isFacingRight ? 0 : 180);
        }
    }

    private void TrySelectAndStartAttack(float distanceToPlayer)
    {
        if (player == null) return;
        if (selectTimer > 0f) return;

        // Turn toward player before deciding
        if ((player.position.x > transform.position.x && !isFacingRight) || (player.position.x < transform.position.x && isFacingRight))
        {
            FlipSprite();
        }

        // Score each attack
        GuardAttackType best = GuardAttackType.None;
        float bestScore = 0f;

        // Melee selection (close). If a chain is pending, force Melee2 selection.
        if (distanceToPlayer <= meleeRange * 1.25f)
        {
            if (chainMeleePending)
            {
                // Force melee2 next, ignoring cooldowns to guarantee chain feel
                best = GuardAttackType.Melee2;
                bestScore = float.MaxValue;
            }
            else
            {
                // Consider melee1 or melee2 depending on chain and individual cooldowns
                float closeFactor = Mathf.InverseLerp(meleeRange * 1.5f, 0f, distanceToPlayer); // higher when very close
                float score = meleeWeight * Mathf.Clamp01(closeFactor);
                if (lastAttack == GuardAttackType.Melee || lastAttack == GuardAttackType.Melee2) score *= (1f - repeatAttackPenalty);
                if (score > bestScore)
                {
                    if (attackChain == 0 && melee1Timer <= 0f)
                    {
                        bestScore = score;
                        best = GuardAttackType.Melee;
                    }
                    else if (attackChain != 0 && melee2Timer <= 0f)
                    {
                        bestScore = score;
                        best = GuardAttackType.Melee2;
                    }
                }
            }
        }

        // Dash (mid)
        if (dashTimer <= 0f)
        {
            float midCenter = dashRange * 0.75f;
            float midSpan = dashRange;
            float midFactor = 1f - Mathf.Clamp01(Mathf.Abs(distanceToPlayer - midCenter) / midSpan);
            float score = dashWeight * Mathf.Clamp01(midFactor);
            if (lastAttack == GuardAttackType.Dash) score *= (1f - repeatAttackPenalty);
            if (distanceToPlayer <= dashRange * 1.1f && distanceToPlayer > meleeRange * 0.8f && score > bestScore)
            {
                bestScore = score;
                best = GuardAttackType.Dash;
            }
        }

        // Ranged (far) - requires LOS
        if (rangedTimer <= 0f && CanSeePlayer())
        {
            float rangedMin = dashRange; // prefer beyond dash range
            float rangedMax = dashRange * 3.5f;
            float ideal = (rangedMin + rangedMax) * 0.5f;
            if (distanceToPlayer >= rangedMin && distanceToPlayer <= rangedMax)
            {
                float farFactor = 1f - Mathf.Clamp01(Mathf.Abs(distanceToPlayer - ideal) / (rangedMax - rangedMin));
                float score = rangedWeight * farFactor * idealRangedMultiplier;
                if (lastAttack == GuardAttackType.Ranged) score *= (1f - repeatAttackPenalty);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = GuardAttackType.Ranged;
                }
            }
        }

        if (best == GuardAttackType.None)
        {
            // No viable attack; chase logic remains in state handlers
            selectTimer = selectionInterval;
            return;
        }

        // Start the chosen attack and apply cooldowns
        // Randomize attack cooldown between 0.8-1x the regular time
        float randomMultiplier = Random.Range(0.8f, 1.0f);

        switch (best)
        {
            case GuardAttackType.Melee:
                MeleeAttack();
                melee1Timer = melee1AttackCooldown;
                attackTimer = attackCooldown * randomMultiplier;
                lastAttack = GuardAttackType.Melee;
                guardCurrentState = GuardState.Attack;
                break;
            case GuardAttackType.Melee2:
                MeleeAttack();
                melee2Timer = melee2AttackCooldown;
                attackTimer = attackCooldown * randomMultiplier;
                lastAttack = GuardAttackType.Melee2;
                guardCurrentState = GuardState.Attack;
                break;
            case GuardAttackType.Dash:
                SetUpAttackHitbox(4);
                dashTimer = dashAttackCooldown;
                attackTimer = attackCooldown * randomMultiplier;
                lastAttack = GuardAttackType.Dash;
                guardCurrentState = GuardState.Attack;
                break;
            case GuardAttackType.Ranged:
                RangedAttack();
                rangedTimer = rangedAttackCooldown;
                attackTimer = attackCooldown * 0.75f * randomMultiplier;
                lastAttack = GuardAttackType.Ranged;
                // remain in Alert state for ranged while chasing
                break;
        }

        selectTimer = selectionInterval;
    }

    public void MeleeAttack()
    {
        if (attackChain == 0) //first melee Attack
        {
            StopMoving();
            isAttacking = 1;
            SetUpAttackHitbox(1);
            attackChain++;
            chainMeleePending = true; // ensure we chain into the second attack
        }
        else
        { //second melee attack
            StopMoving();
            isAttacking = 2;
            SetUpAttackHitbox(2);
            attackChain = 0;
            // Randomize attack cooldown between 0.8-1x the regular time
            float randomMultiplier = Random.Range(0.8f, 1.0f);
            attackTimer = (attackCooldown / 2) * randomMultiplier;
            chainMeleePending = false; // chain complete
        }
    }

    public void RangedAttack()
    {
        StopMoving();
        isAttacking = 3;
        ChangeAnimationState("RangedAttack");
    }

    public void SetUpAttackHitbox(int attackNum)
    {
        switch (attackNum)
        {
            case 1:
                isAttacking = 1;
                attackHitboxScript.CustomizeHitbox(attackHitboxes[0]);
                ChangeAnimationState("Attack1");
                break;
            case 2:
                isAttacking = 2;
                attackHitboxScript.CustomizeHitbox(attackHitboxes[1]);
                ChangeAnimationState("Attack2");
                break;
            case 4:
                isAttacking = 4;
                attackHitboxScript.CustomizeHitbox(attackHitboxes[2]);
                ChangeAnimationState("AttackDash");
                break;
            default:
                break;
        }
    }

    public void DashVelocityIncrease()
    {
        float dashSpeed = isFacingRight ? dashAttackSpeed : -1 * dashAttackSpeed;
        rb.linearVelocity = new Vector2(dashSpeed, rb.linearVelocity.y);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // Draw base gizmos (detection range, patrol points, raycasts)

        // Draw facing direction indicator
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (isFacingRight ? Vector3.right : Vector3.left) * detectionRange);
    }

}
