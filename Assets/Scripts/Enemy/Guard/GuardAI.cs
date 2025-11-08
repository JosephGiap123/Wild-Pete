using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAI : EnemyBase, IHasFacing
{
    private enum GuardAttackType { None = 0, Melee = 1, Melee2 = 2, Ranged = 3, Dash = 4 }
    private GuardAudioManager audioManager;

    private enum GuardState
    {
        Idle,
        Patrol,
        Alert,
        Attack,
        Return
    }

    [SerializeField] private GuardState guardCurrentState = GuardState.Idle;

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false; // Toggle this to see debug logs

    [Header("Animations")]
    [SerializeField] private Animator anim;
    private string currentState = "Idle";

    [Header("Guard Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float alertSpeed = 4f; // Faster movement when chasing player
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private BoxCollider2D groundCheckBox;
    [SerializeField] private LayerMask groundLayer;
    public bool isFacingRight = true;
    public bool IsFacingRight => isFacingRight; // IHasFacing implementation
    public bool isInAir = false;

    [Header("Combat Settings")]
    private int isAttacking = 0; // 0 = no attack, 1 = melee1, 2 = melee2, 3 = ranged, 4 = dash attack
    private int attackChain = 0;
    private bool isHurt = false;
    private bool isDead = false;
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
    [SerializeField] private BoxCollider2D boxAttackHitbox;
    [SerializeField] private AttackHitBoxGuard attackHitboxScript;

    [Header("Patrol Settings")]
    [SerializeField] private Vector2[] patrolPoints;
    private int currentPatrolIndex = 0;
    [SerializeField] private float patrolWaitTime = 2f;
    private float patrolWaitTimer = 0f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float nearDetectRadius = 1f; // detect player if extremely close, regardless of facing/LOS
    [SerializeField] private float loseSightTime = 3f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstructionLayer;
    private Transform player;
    private float loseSightTimer = 0f;

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
        base.Awake();
        // Additional initialization if needed
        audioManager = GetComponent<GuardAudioManager>() ?? GetComponentInChildren<GuardAudioManager>();
    }

    private void Start()
    {
        // Try to find player immediately if it exists
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            if (debugMode) Debug.Log($"GuardAI: Player found at start: {player.name}");
        }
    }

    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
    }

    private void HandlePlayerSet(GameObject playerObj)
    {
        if (playerObj != null)
        {
            this.player = playerObj.transform;
            if (debugMode) Debug.Log($"GuardAI: Player set via GameManager: {player.name}");
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null)
        {
            // Try to find player again
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                return false;
            }
        }

        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Immediate proximity detection (anti-hugging): if very close, detect regardless of facing/LOS
        if (distanceToPlayer <= nearDetectRadius)
            return true;

        // Check if player is in detection range
        if (distanceToPlayer > detectionRange)
            return false;

        // Only check in facing direction
        if ((isFacingRight && directionToPlayer.x < 0) || (!isFacingRight && directionToPlayer.x > 0))
            return false;

        // Raycast ONLY for obstructions - if we hit something that's NOT the player, line of sight is blocked
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, obstructionLayer);

        // Draw debug ray to visualize detection
        Debug.DrawRay(transform.position, directionToPlayer.normalized * distanceToPlayer, hit.collider != null ? Color.red : Color.green);

        // If we hit an obstruction before reaching the player, we can't see them
        if (hit.collider != null)
        {
            if (debugMode) Debug.Log($"GuardAI: Line of sight blocked by {hit.collider.name}");
            return false;
        }

        // No obstruction and player is in range and direction - we can see them!
        if (debugMode) Debug.Log("GuardAI: Can see player!");
        return true;
    }

    private void MoveTowards(Vector2 target, float speed = -1f)
    {
        // Use default moveSpeed if no speed specified
        if (speed < 0) speed = moveSpeed;

        float deltaX = target.x - transform.position.x;

        // Higher tolerance for movement to prevent micro-adjustments
        if (Mathf.Abs(deltaX) > 0.5f)
        {
            float direction = Mathf.Sign(deltaX);
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

            if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
                FlipSprite();
        }
        else
        {
            StopMoving();
        }
    }



    public void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void Update()
    {
        // Always call AnimationControl, even when dead or hurt (for death/hurt animations)
        AnimationControl();

        // Stop processing AI logic if dead or hurt
        if (isDead || isHurt) return;

        IsGroundedCheck();

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
            Debug.Log($"GuardAI State: {guardCurrentState}, CanSeePlayer: {canSee}, Player: {(player != null ? player.name : "NULL")}, Attacking: {isAttacking}");
        }

        switch (guardCurrentState)
        {
            case GuardState.Idle: HandleIdle(canSee); break;
            case GuardState.Patrol: HandlePatrol(canSee); break;
            case GuardState.Alert: HandleAlert(canSee); break;
            case GuardState.Attack: HandleAttack(); break;
            case GuardState.Return: HandleReturn(); break;
        }
    }


    private void HandleIdle(bool canSee)
    {
        StopMoving();

        if (canSee)
        {
            if (debugMode) Debug.Log("GuardAI Idle: Spotted player! Entering Alert state.");
            guardCurrentState = GuardState.Alert;
            loseSightTimer = 0f;
            return;
        }

        patrolWaitTimer += Time.deltaTime;

        if (patrolWaitTimer >= patrolWaitTime)
        {
            patrolWaitTimer = 0f;
            guardCurrentState = GuardState.Patrol;
            if (debugMode) Debug.Log("GuardAI Idle: Finished waiting, starting patrol.");
        }
    }

    private void HandlePatrol(bool canSee)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("GuardAI: No patrol points set! Switching to Idle.");
            guardCurrentState = GuardState.Idle;
            return;
        }

        if (canSee)
        {
            if (debugMode) Debug.Log("GuardAI: Spotted player during patrol! Switching to Alert.");
            guardCurrentState = GuardState.Alert;
            loseSightTimer = 0f;
            return;
        }

        Vector2 patrolTarget = patrolPoints[currentPatrolIndex];
        float distanceToTarget = Vector2.Distance(transform.position, patrolTarget);

        MoveTowards(patrolTarget);

        // Much higher tolerance to prevent getting stuck
        if (distanceToTarget < 1.0f)
        {
            StopMoving();
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                patrolWaitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                if (debugMode) Debug.Log($"GuardAI Patrol: Moving to next patrol point: {currentPatrolIndex}");
            }
        }
    }

    private void HandleAlert(bool canSee)
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
        guardCurrentState = GuardState.Alert;
        loseSightTimer = 0f; // Reset timer so guard searches for a while
        if (debugMode) Debug.Log("GuardAI Attack: Attack finished, entering Alert state to search for player.");
    }


    private void HandleReturn()
    {
        // Check if we can see player again while returning
        if (CanSeePlayer())
        {
            if (debugMode) Debug.Log("GuardAI Return: Spotted player again! Re-entering Alert state.");
            guardCurrentState = GuardState.Alert;
            loseSightTimer = 0f;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            if (debugMode) Debug.LogWarning("GuardAI Return: No patrol points, switching to Idle.");
            guardCurrentState = GuardState.Idle;
            return;
        }

        Vector2 patrolTarget = patrolPoints[currentPatrolIndex];
        float distanceToTarget = Vector2.Distance(transform.position, patrolTarget);

        MoveTowards(patrolTarget); // Use normal patrol speed

        if (distanceToTarget < 1.0f) // Same tolerance as patrol for consistency
        {
            if (debugMode) Debug.Log("GuardAI Return: Reached patrol point, idling before resuming patrol.");
            guardCurrentState = GuardState.Idle;
            patrolWaitTimer = 0f; // Reset wait timer for idle period
        }
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
        StopMoving(); // Ensure movement is stopped when attack ends
        if (debugMode) Debug.Log("GuardAI: Attack ended, returning to normal behavior.");

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
        audioManager?.PlayShot();
        return;
    }
    public override void Hurt(int dmg, Vector2 knockbackForce)
    {
        if (isDead) return;

        health -= dmg;
        if (debugMode) Debug.Log($"GuardAI: Hurt! Health: {health}");
        // Immediately stop current movement, then apply knockback so stun halts momentum
        StopMoving();
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.linearVelocity += knockbackForce;
        
        if (audioManager != null)
        {
            if (health <= 0)
                audioManager?.PlayDeath();
            else
                audioManager?.PlayHurt();
        }

        // Clear ongoing/combo attack intent so we don't spam the same attack after stun
        isAttacking = 0;
        attackChain = 0;
        attackTimer = attackCooldown / 2; // small delay before next decision

        if (damageText != null)
        {
            GameObject dmgText = Instantiate(damageText, transform.position, transform.rotation);
            dmgText.GetComponentInChildren<DamageText>().Initialize(new(knockbackForce.x, 5f), dmg, new Color(0.8862745f, 0.3660145f, 0.0980392f, 1f), Color.red);
        }
        StartHurtAnim(dmg);

        // Enter Alert state and try to find attacker
        if (IsAlive())
        {
            guardCurrentState = GuardState.Alert;
            loseSightTimer = 0f;

            // Try to find player if we don't have reference
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    if (debugMode) Debug.Log("GuardAI: Found player after being attacked!");
                }
            }

            if (debugMode) Debug.Log("GuardAI: Hurt! Entering Alert state!");
        }
        else
        {
            StartCoroutine(Death());
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

    void IsGroundedCheck()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheckBox.bounds.center, groundCheckBox.bounds.size, 0f, groundLayer);
        bool wasInAir = isInAir;
        isInAir = colliders.Length == 0;
        
        // Play run loop sound when landing and moving
        if (wasInAir && !isInAir && Mathf.Abs(rb.linearVelocity.x) > 0.2f && audioManager != null)
        {
            audioManager?.StartRunLoop();
        }
    }

    public void ChangeAnimationState(string newState)
    {
        if (newState == currentState) return;
        anim.Play(newState, 0, 0f);
        currentState = newState;
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

    public virtual void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        projectileSpawnPoint.localRotation = Quaternion.Euler(0, 0, isFacingRight ? 0 : 180);
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
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
        switch (best)
        {
            case GuardAttackType.Melee:
                MeleeAttack();
                melee1Timer = melee1AttackCooldown;
                attackTimer = attackCooldown;
                lastAttack = GuardAttackType.Melee;
                guardCurrentState = GuardState.Attack;
                break;
            case GuardAttackType.Melee2:
                MeleeAttack();
                melee2Timer = melee2AttackCooldown;
                attackTimer = attackCooldown;
                lastAttack = GuardAttackType.Melee2;
                guardCurrentState = GuardState.Attack;
                break;
            case GuardAttackType.Dash:
                SetUpAttackHitbox(4);
                dashTimer = dashAttackCooldown;
                attackTimer = attackCooldown;
                lastAttack = GuardAttackType.Dash;
                guardCurrentState = GuardState.Attack;
                break;
            case GuardAttackType.Ranged:
                RangedAttack();
                rangedTimer = rangedAttackCooldown;
                attackTimer = attackCooldown * 0.75f;
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
            attackTimer = attackCooldown / 2;
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
                if (audioManager != null) audioManager?.PlayMelee();
                break;
            case 2:
                isAttacking = 2;
                attackHitboxScript.CustomizeHitbox(attackHitboxes[1]);
                ChangeAnimationState("Attack2");
                if (audioManager != null) audioManager?.PlayMelee();
                break;
            case 4:
                isAttacking = 4;
                attackHitboxScript.CustomizeHitbox(attackHitboxes[2]);
                ChangeAnimationState("AttackDash");
                if (audioManager != null) audioManager?.PlayDash();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (isFacingRight ? Vector3.right : Vector3.left) * detectionRange);

        Gizmos.color = Color.yellow;
        if (patrolPoints != null)
        {
            foreach (var p in patrolPoints)
            {
                if (p != null)
                    Gizmos.DrawWireSphere(p, 0.2f);
            }
        }
    }

}
