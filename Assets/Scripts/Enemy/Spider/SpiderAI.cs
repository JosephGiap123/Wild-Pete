using UnityEngine;
using System.Collections;

public class SpiderAI : PatrolEnemyAI
{
    [Header("Combat Settings")]
    private int isAttacking = 0; // 0 = no attack, 1 = melee, 2 = lunge/dash, 3 = web shot
    [SerializeField] private float meleeRange = 1.2f; // Very close range - melee or lunge
    [SerializeField] private float dashRange = 4f; // Dash range - lunge or ranged
    [SerializeField] private float rangedRange = 8f; // Ranged attack range

    [Header("Attack Settings")]
    [SerializeField] private int rangedDamage = 3;
    [SerializeField] private float rangedAttackCooldown = 5f;
    [SerializeField] private float meleeAttackCooldown = 1.5f;
    [SerializeField] private float dashAttackCooldown = 3f;
    [SerializeField] private float bulletSpeed = 14f;
    [SerializeField] private float bulletLifeTime = 3f;
    [SerializeField] private float dashAttackSpeed = 8f;

    [Header("Attack Selection Weights")]
    [SerializeField] private float meleeWeight = 1.0f;
    [SerializeField] private float lungeWeight = 1.0f;
    [SerializeField] private float rangedWeight = 1.0f;
    [SerializeField] private float selectionInterval = 0.2f;
    [SerializeField] private float repeatAttackPenalty = 0.35f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 11f; // Increased from 7f for higher jump
    // Note: jumpCooldown and jumpTimer are now inherited from base class
    [SerializeField] private float jumpHorizontalSpeed = 6f; // Horizontal speed while jumping
    private bool isJumping = false; // Track if we're in a jump

    [Header("Ground Check")]
    // Note: groundCheckBox, groundLayer, and isInAir are now inherited from base class

    [Header("Attack References")]
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] private GenericAttackHitbox attackHitboxScript;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Animation References")]
    [SerializeField] Animator anim;
    private string currentAnimationState = "SpiderIdle";

    // Attack timers
    private float rangedTimer = 0f;
    private float meleeTimer = 0f;
    private float dashTimer = 0f;
    private float attackTimer = 0f;
    private float selectTimer = 0f;

    private enum SpiderAttackType { None = 0, Melee = 1, Lunge = 2, Ranged = 3 }
    private SpiderAttackType lastAttack = SpiderAttackType.None;

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

        // Update timers (jumpTimer is now handled by base class)
        rangedTimer -= Time.deltaTime;
        meleeTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        selectTimer -= Time.deltaTime;

        // Stop horizontal movement during non-dash attacks
        if (isAttacking != 0 && isAttacking != 2) // Not lunge/dash
        {
            StopMoving();
        }

        // Use base class state machine
        bool canSee = CanSeePlayer();
        switch (currentState)
        {
            case PatrolState.Idle: HandleIdle(canSee); break;
            case PatrolState.Patrol: HandlePatrol(canSee); break;
            case PatrolState.Alert: HandleAlert(canSee); break;
        }

        // Check for jump opportunity (player Y is higher, in dash range, not attacking)
        if (isAttacking == 0 && !isInAir && jumpTimer <= 0f && player != null && !isJumping)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= rangedRange)
            {
                float playerY = player.position.y;
                float spiderY = transform.position.y;
                if (playerY > spiderY + 0.5f) // Player is significantly higher
                {
                    TryJump();
                }
            }
        }

        // Move towards player while jumping
        if (isJumping && isInAir && player != null)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * jumpHorizontalSpeed, rb.linearVelocity.y);

            // Face the direction we're moving
            if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
            {
                FlipSprite();
            }
        }
    }
    public void ChangeAnimationState(string newState)
    {
        if (newState == currentAnimationState) return;
        anim.Play(newState, 0, 0f);
        currentAnimationState = newState;
    }

    // Override HandleAlert to maintain dash range instead of chasing to melee
    protected override void HandleAlert(bool canSee)
    {
        // If currently attacking, stop movement (except for lunge)
        if (isAttacking != 0)
        {
            if (isAttacking != 2) // Not lunge/dash
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
                Vector2 directionToPlayer = player.position - transform.position;
                bool playerIsBehind = (isFacingRight && directionToPlayer.x < 0) || (!isFacingRight && directionToPlayer.x > 0);

                if (playerIsBehind)
                {
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

        // Try to select and start attack first
        if (attackTimer <= 0f && selectTimer <= 0f)
        {
            TrySelectAndStartAttack(distanceToPlayer);
        }

        // Apply movement based on distance and attack state
        // Don't move if we're jumping (jump movement is handled separately)
        if (isJumping && isInAir)
        {
            return; // Let jump movement handle it
        }

        if (isAttacking != 0)
        {
            // Currently attacking - stop movement (except for lunge)
            if (isAttacking != 2) // Not lunge/dash
            {
                StopMoving();
            }
        }
        else if (distanceToPlayer > dashRange)
        {
            // Too far - move closer to dash range
            MoveTowards(player.position, alertSpeed);
        }
        else if (distanceToPlayer < meleeRange)
        {
            // Very close - sometimes back away, sometimes stay for melee
            // If we just selected a melee attack, stay and let it happen
            // Otherwise, back away with some probability (to allow melee attacks sometimes)
            if (isAttacking == 1)
            {
                // Melee attack selected - stay and attack
                StopMoving();
            }
            else
            {
                // Not melee attacking - sometimes back away, sometimes stay
                // Give a chance to stay close for potential melee attack
                // If attack timer is ready, stay close to allow melee selection
                if (attackTimer <= 0f && Random.value < 0.75f) // 75% chance to stay
                {
                    // Stay close - might melee attack next frame
                    StopMoving();
                }
                else
                {
                    // Back away to dash range
                    Vector2 awayFromPlayer = (transform.position - player.position).normalized;
                    Vector2 targetPosition = (Vector2)player.position + awayFromPlayer * dashRange;
                    MoveTowards(targetPosition, alertSpeed);
                }
            }
        }
        else
        {
            // In ideal dash range - stop and prepare for attacks
            StopMoving();
        }
    }

    private void TrySelectAndStartAttack(float distanceToPlayer)
    {
        if (isAttacking != 0) return; // Already attacking

        SpiderAttackType selectedAttack = SpiderAttackType.None;

        // Attack selection logic based on distance
        if (distanceToPlayer <= meleeRange)
        {
            // Very close: choose between melee and lunge
            selectedAttack = SelectAttack(new[] { SpiderAttackType.Melee, SpiderAttackType.Lunge });
        }
        else if (distanceToPlayer <= dashRange)
        {
            // In dash range: choose between lunge and ranged
            selectedAttack = SelectAttack(new[] { SpiderAttackType.Lunge, SpiderAttackType.Ranged });
        }
        else if (distanceToPlayer <= rangedRange)
        {
            // In ranged range: always ranged attack
            selectedAttack = SpiderAttackType.Ranged;
        }

        // Execute selected attack
        if (selectedAttack != SpiderAttackType.None)
        {
            StartAttack(selectedAttack);
        }
    }

    private SpiderAttackType SelectAttack(SpiderAttackType[] availableAttacks)
    {
        if (availableAttacks.Length == 0) return SpiderAttackType.None;
        if (availableAttacks.Length == 1) return availableAttacks[0];

        // Weighted random selection with repeat penalty
        float[] weights = new float[availableAttacks.Length];
        float totalWeight = 0f;

        for (int i = 0; i < availableAttacks.Length; i++)
        {
            float weight = GetAttackWeight(availableAttacks[i]);
            if (availableAttacks[i] == lastAttack)
            {
                weight *= repeatAttackPenalty; // Penalize repeating last attack
            }
            weights[i] = weight;
            totalWeight += weight;
        }

        // Random selection
        float random = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < availableAttacks.Length; i++)
        {
            cumulative += weights[i];
            if (random <= cumulative)
            {
                return availableAttacks[i];
            }
        }

        return availableAttacks[availableAttacks.Length - 1]; // Fallback
    }

    private float GetAttackWeight(SpiderAttackType attackType)
    {
        return attackType switch
        {
            SpiderAttackType.Melee => meleeWeight,
            SpiderAttackType.Lunge => lungeWeight,
            SpiderAttackType.Ranged => rangedWeight,
            _ => 0f
        };
    }

    private void StartAttack(SpiderAttackType attackType)
    {
        if (isAttacking != 0) return;

        FaceTowardsPlayer();
        lastAttack = attackType;

        switch (attackType)
        {
            case SpiderAttackType.Melee:
                if (meleeTimer <= 0f)
                {
                    isAttacking = 1;
                    SetUpAttackHitbox(1);
                    float randomMultiplier = Random.Range(0.8f, 1.0f);
                    meleeTimer = meleeAttackCooldown * randomMultiplier;
                    attackTimer = meleeAttackCooldown * randomMultiplier;
                    selectTimer = selectionInterval;
                }
                break;

            case SpiderAttackType.Lunge:
                if (dashTimer <= 0f)
                {
                    isAttacking = 2;
                    SetUpAttackHitbox(2);
                    float randomMultiplier = Random.Range(0.8f, 1.0f);
                    dashTimer = dashAttackCooldown * randomMultiplier;
                    attackTimer = dashAttackCooldown * randomMultiplier;
                    selectTimer = selectionInterval;
                }
                break;

            case SpiderAttackType.Ranged:
                if (rangedTimer <= 0f)
                {
                    isAttacking = 3;
                    ChangeAnimationState("SpiderWebShot");
                    float randomMultiplier = Random.Range(0.8f, 1.0f);
                    rangedTimer = rangedAttackCooldown * randomMultiplier;
                    attackTimer = rangedAttackCooldown * randomMultiplier;
                    selectTimer = selectionInterval;
                }
                break;
        }
    }

    private void TryJump()
    {
        if (isInAir || jumpTimer > 0f || isJumping) return;

        isJumping = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpTimer = jumpCooldown; // Use base class jumpCooldown
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
    public void FaceTowardsPlayer()
    {
        // Debug.Log(player.position.x + " " + this.gameObject.transform.position.x);
        if (isFacingRight && player.position.x < transform.position.x)
        {
            FlipSprite();
        }
        else if (!isFacingRight && player.position.x > transform.position.x)
        {
            FlipSprite();
        }
    }
    protected void AnimationControl()
    {
        if (isDead)
        {
            ChangeAnimationState("SpiderDeath");
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
                ChangeAnimationState("SpiderRising");
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                ChangeAnimationState("SpiderFalling");
            }
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.2f)
            {
                ChangeAnimationState("SpiderWalk");
            }
            else
            {
                ChangeAnimationState("SpiderIdle");
            }
        }
    }

    protected override void IsGroundedCheck()
    {
        bool wasInAir = isInAir;
        base.IsGroundedCheck(); // Base class handles the actual ground checking

        // Reset jumping flag when we land
        if (wasInAir && !isInAir)
        {
            isJumping = false;
        }
    }

    //attacks
    public void SetUpAttackHitbox(int attackNum)
    {
        switch (attackNum)
        {
            case 1: //melee
                attackHitboxScript.CustomizeHitbox(attackHitboxes[0]);
                ChangeAnimationState("SpiderMelee");
                break;
            case 2: //lunge
                attackHitboxScript.CustomizeHitbox(attackHitboxes[1]);
                ChangeAnimationState("SpiderLunge");
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

    private Coroutine hurtWatchdogCoroutine;

    private void StartHurtAnim(int dmg)
    {
        float damageFlashTime = Mathf.Max(0.2f, (dmg / (float)maxHealth));
        isHurt = true;
        ChangeAnimationState("Hurt");
        StartCoroutine(DamageFlash(damageFlashTime));
        
        // Start hurt watchdog
        if (hurtWatchdogCoroutine != null)
        {
            StopCoroutine(hurtWatchdogCoroutine);
        }
        hurtWatchdogCoroutine = StartCoroutine(HurtWatchdog());
    }

    private IEnumerator HurtWatchdog()
    {
        yield return new WaitForSeconds(0.3f);
        
        // If still hurt after 0.3s, force end the hurt state
        if (isHurt)
        {
            EndHurtState();
        }
        
        hurtWatchdogCoroutine = null;
    }

    public override void EndHurtState()
    {
        isHurt = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        // Stop watchdog if it's running
        if (hurtWatchdogCoroutine != null)
        {
            StopCoroutine(hurtWatchdogCoroutine);
            hurtWatchdogCoroutine = null;
        }
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
        currentAnimationState = "SpiderIdle";
        if (anim != null)
        {
            anim.Play("SpiderIdle");
        }

        // Reset attack timers
        rangedTimer = 0f;
        meleeTimer = 0f;
        dashTimer = 0f;
        attackTimer = 0f;
        selectTimer = 0f;
        jumpTimer = 0f;
        isJumping = false;
        lastAttack = SpiderAttackType.None;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // Draw base gizmos (detection range, patrol points, raycasts)

        // Draw attack ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }

    public void InstBullet()
    {
        // Calculate direction based on enemy facing, not spawn point rotation
        // This ensures bullet always goes the correct direction even if enemy flips during attack
        // For 2D: 0 degrees = right, 180 degrees = left
        float angle = isFacingRight ? 0f : 180f;
        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);
        
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, bulletRotation);
        GuardBullet projScript = projectile.GetComponent<GuardBullet>();
        if (projScript != null)
        {
            projScript.Initialize(rangedDamage, bulletSpeed, bulletLifeTime);
        }
        return;
    }
}
