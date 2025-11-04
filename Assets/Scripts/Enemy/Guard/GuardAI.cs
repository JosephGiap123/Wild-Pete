using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAI : EnemyBase
{

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
    public bool isInAir = false;

    [Header("Combat Settings")]
    private int isAttacking = 0; // 0 = no attack, 1 = melee1, 2 = melee2, 3 = ranged, 4 = dash attack
    private int attackChain = 0;
    private bool isHurt = false;
    private bool isDead = false;
    [SerializeField] private float meleeRange = 1f;
    [SerializeField] private float dashRange = 5f;

    [Header("Attack Settings")]
    [SerializeField] private int melee1Damage = 2;
    [SerializeField] private Vector2 melee1HitboxSize = new(1f, 1f);
    [SerializeField] private Vector2 melee1HitboxOffset = new(0.5f, 0f);
    [SerializeField] private Vector2 melee1Knockback = new(2f, 1f);
    [SerializeField] private int melee2Damage = 3;
    [SerializeField] private Vector2 melee2HitboxSize = new(1.2f, 1f);
    [SerializeField] private Vector2 melee2HitboxOffset = new(0.6f, 0f);
    [SerializeField] private Vector2 melee2Knockback = new(3f, 1f);
    [SerializeField] private int rangedDamage = 2;
    [SerializeField] private float rangedAttackCooldown = 5f;
    [SerializeField] private float bulletSpeed = 14f;
    [SerializeField] private float bulletLifeTime = 3f;
    [SerializeField] private int dashAttackDamage = 4;
    [SerializeField] private Vector2 dashAttackHitboxSize = new(1.5f, 1f);
    [SerializeField] private Vector2 dashAttackHitboxOffset = new(0.75f, 0f);
    [SerializeField] private Vector2 dashAttackKnockback = new(4f, 1f);
    [SerializeField] private float dashAttackSpeed = 8f;

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
    [SerializeField] private float loseSightTime = 3f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstructionLayer;
    private Transform player;
    private float loseSightTimer = 0f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1f; // seconds between attacks
    private float attackTimer = 0f;
    private float rangedTimer = 0f;



    protected override void Awake()
    {
        base.Awake();
        // Additional initialization if needed
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

        rangedTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

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
        // Don't move if currently attacking!
        if (isAttacking != 0)
        {
            StopMoving();
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

        // Try ranged attack while chasing (if in range and off cooldown)
        float rangedAttackMinRange = dashRange; // Start ranged attacks beyond dash range
        float rangedAttackMaxRange = dashRange * 3.5f; // Maximum range for ranged attacks

        if (distanceToPlayer >= rangedAttackMinRange && distanceToPlayer <= rangedAttackMaxRange && rangedTimer <= 0f)
        {
            // In ranged attack range and ready to fire - shoot while chasing!
            if (debugMode) Debug.Log("GuardAI Alert: Firing ranged attack while chasing!");
            RangedAttack();
            rangedTimer = rangedAttackCooldown;
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
            DoAttack();
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

        health -= dmg;
        if (debugMode) Debug.Log($"GuardAI: Hurt! Health: {health}");
        // Immediately stop current movement, then apply knockback so stun halts momentum
        StopMoving();
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.linearVelocity += knockbackForce;

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
        isInAir = colliders.Length == 0;
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
        else if (isHurt)
        {
            //played outside of this controller.
            return;
        }
        else if (isAttacking != 0)
        {
            switch (isAttacking)
            {
                case 1:
                    ChangeAnimationState("Attack1");
                    break;
                case 2:
                    ChangeAnimationState("Attack2");
                    break;
                case 3:
                    ChangeAnimationState("RangedAttack");
                    break;
                case 4:
                    ChangeAnimationState("AttackDash");
                    break;
            }
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
            if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed + 0.2f)
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

    //Attacks - Only called in Attack state for close-range combat (melee/dash)
    public void DoAttack()
    {
        //Choose an attack based on what is done.
        //1 = melee1, 2 =melee2, 3 = ranged, 4 = dash
        //melee2 MUST be called after melee1.
        // NOTE: Ranged attacks are now handled in Alert state while chasing

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (!playerTransform || isDead || isHurt) return;
        float distanceToPlayer = Vector2.Distance(playerTransform.position, transform.position);

        //flip towards player if not facing them.
        if ((playerTransform.position.x > transform.position.x && !isFacingRight) || (playerTransform.position.x < transform.position.x && isFacingRight))
        {
            FlipSprite();
        }

        // Only handle melee/dash attacks here (ranged is handled in Alert state)
        if (distanceToPlayer <= meleeRange)
        {
            // Close range: Melee attacks (with chance of dash)
            int meleeChoice = Random.Range(1, 5); //1-3 = melee, 4 = dash
            if (meleeChoice == 4)
            {
                if (debugMode) Debug.Log("GuardAI DoAttack: Random Dash from melee range");
                SetUpAttackHitbox(4);
                attackTimer = attackCooldown;
            }
            else
            {
                if (debugMode) Debug.Log("GuardAI DoAttack: Melee attack");
                MeleeAttack();
            }
        }
        else if (distanceToPlayer <= dashRange)
        {
            // Medium range: Dash attack
            if (debugMode) Debug.Log("GuardAI DoAttack: Dash range - performing dash attack");
            SetUpAttackHitbox(4);
            attackTimer = attackCooldown;
        }
        else
        {
            // Player moved out of close combat range - return to Alert to chase
            if (debugMode) Debug.Log("GuardAI DoAttack: Player out of close combat range, returning to Alert.");
        }
    }

    public void MeleeAttack()
    {
        if (attackChain == 0) //first melee Attack
        {
            isAttacking = 1;
            SetUpAttackHitbox(1);
            attackChain++;
        }
        else
        { //second melee attack
            isAttacking = 2;
            SetUpAttackHitbox(2);
            attackChain = 0;
            attackTimer = attackCooldown / 2;
        }
    }

    public void RangedAttack()
    {
        isAttacking = 3;
        ChangeAnimationState("RangedAttack");
    }

    public void SetUpAttackHitbox(int attackNum)
    {
        switch (attackNum)
        {
            case 1:
                isAttacking = 1;
                attackHitboxScript.ChangeHitboxBox(melee1HitboxOffset, melee1HitboxSize, melee1Damage, melee1Knockback);
                ChangeAnimationState("Attack1");
                break;
            case 2:
                isAttacking = 2;
                attackHitboxScript.ChangeHitboxBox(melee2HitboxOffset, melee2HitboxSize, melee2Damage, melee2Knockback);
                ChangeAnimationState("Attack2");
                break;
            case 4:
                isAttacking = 4;
                attackHitboxScript.ChangeHitboxBox(dashAttackHitboxOffset, dashAttackHitboxSize, dashAttackDamage, dashAttackKnockback);
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
