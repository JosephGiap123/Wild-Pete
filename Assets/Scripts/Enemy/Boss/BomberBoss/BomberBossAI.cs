using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BomberBoss : EnemyBase, IHasFacing
{

    [Header("Animations")]
    [SerializeField] private Animator anim;
    private string currentState = "Idle";

    [Header("Movement Settings")]

    private bool isStaggered = false;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] private BoxCollider2D groundCheckBox;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat Setting")]
    public int phaseNum = 1;
    protected bool isInAir = false;
    protected bool inAttackState = true;
    protected int isAttacking = 0;
    protected bool isInvincible = false;

    [SerializeField] private float meleeDistance = 1f;
    [SerializeField] private float rangedDistance = 15f;
    [Header("Combat Stats")]
    [SerializeField] private int originalMaxAmmo = 10;
    private int maxAmmo = 10;
    private int currentAmmo = 10;
    [SerializeField] private int originalMaxAerialShotsConsecutively = 3;
    private int maxAerialShotsConsecutively = 3;
    [SerializeField] private float meleeCooldown = 2f;
    private float meleeTimer = 0f;
    [SerializeField] private float rangedCooldown = 6f;
    [SerializeField] private float aerialRangedCooldown = 0.5f;
    private float rangedTimer = 0f;
    [SerializeField] private float rocketJumpCooldown = 8f;
    private float rocketJumpTimer = 0f;

    [SerializeField] private float originalUltCooldown = 20f;
    private float ultCooldown = 20f;
    private float ultTimer = 0f;

    [SerializeField] private int originalMaxConsecutiveUltimates = 1;
    private int consecutiveUltimates = 0;
    private int maxConsecutiveUltimates = 1;

    // Track consecutive aerial shots
    private int consecutiveAerialShots = 0;

    // Track ultimate usage for weighting (lower = used more recently, higher priority)
    private int ult1UsageWeight = 0; // Dynamite
    private int ult2UsageWeight = 0; // Landmines
    private int ult3UsageWeight = 0; // Nuke

    [SerializeField] private float staggerTime = 3f;

    [SerializeField] int damageToStagger = 40;
    private int currentStaggerDamage = 0;
    private float staggerTimer = 0f;


    [Header("Combat References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject rocketJumpExplosionPrefab;
    [SerializeField] private GenericAttackHitbox attackHitboxScript;
    [SerializeField] private GameObject phaseChangeParticles;
    [SerializeField] private WardenAudioManager audioManager;
    [Header("Rocket Spawn Points")]
    [SerializeField] private Transform standRocketProjSpawnPt;
    [SerializeField] private Transform airDiagonalRocketProjSpawnPt;
    [SerializeField] private Transform airStraightRocketProjSpawnPt;

    [SerializeField] private GameObject dynamitePrefab;
    [SerializeField] private GameObject landminePrefab;

    [SerializeField] private GameObject nukePrefab;

    [Header("Ultimate Attack - Dynamite Spawn")]
    [Tooltip("BoxCollider2D that defines the area where dynamite can spawn. Set as trigger.")]
    [SerializeField] private BoxCollider2D SpawnBounds;
    [Tooltip("Number of dynamite to spawn in the ultimate attack")]
    [SerializeField] private int dynamiteCount = 5;
    [Tooltip("Initial downward velocity for spawned dynamite")]

    [SerializeField] private float dynamiteFallSpeed = 2f;
    [Tooltip("Random horizontal velocity range for dynamite")]
    [SerializeField] private Vector2 horizontalVelocityRange = new Vector2(-1f, 1f);
    [Tooltip("Y offset from top of bounds for spawn position (positive = slightly below top)")]

    [Header("Ultimate Attack - Land Mine Spawn")]
    [SerializeField] private int landMineCount = 5;
    [Tooltip("Y offset from top of bounds for spawn position (positive = slightly below top)")]
    [SerializeField] private float topSpawnOffset = 0.5f;


    protected float distanceToPlayer = 100f;


    [Header("Particles")]
    [SerializeField] private ParticleSystem dashParticle;
    [SerializeField] private ParticleSystem shootStandingParticle;
    [SerializeField] private ParticleSystem shootAirDiagonalParticle;
    [SerializeField] private ParticleSystem shootAirStraightParticle;
    [SerializeField] BossHPBarInteractor hpBarInteractor;
    private Transform player;

    [SerializeField] private Transform centerOfArena;

    protected override void Awake()
    {
        base.Awake();
        isInvincible = true; // Start invincible until entrance completes
        hpBarInteractor.ShowHealthBar(false); // Hide health bar initially
    }

    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void HandlePlayerSet(GameObject playerObj)
    {
        if (playerObj != null) this.player = playerObj.transform;
    }

    public void Start()
    {
        StartCoroutine(WaitForNearbyPlayer());
    }

    public IEnumerator WaitForNearbyPlayer()
    {
        ChangeAnimationState("EntranceIdle");
        yield return new WaitForSeconds(0.5f);

        // Wait for player reference to be set
        while (player == null)
        {
            // Try to get player from GameManager
            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                player = GameManager.Instance.player.transform;
            }
            ChangeAnimationState("EntranceIdle");
            yield return null;
        }

        // Keep animation state updated while waiting (since AnimationControl won't run when inAttackState is true)
        while (distanceToPlayer > 3f)
        {
            ChangeAnimationState("EntranceIdle");
            yield return null;
        }

        // Play entrance animation when player is nearby
        ChangeAnimationState("Entrance");
        isInvincible = false;
        hpBarInteractor.ShowHealthBar(true);
        // Update health bar visual when showing it (to reflect current health)
        hpBarInteractor.UpdateHealthVisual();

        // The entrance animation will call EndAttackState() via animation event
        // which will set inAttackState = false when the animation completes
        // No need to manually set it here
    }

    public void Update()
    {
        if (PauseController.IsGamePaused) return;

        meleeTimer -= Time.deltaTime;
        ultTimer -= Time.deltaTime;
        staggerTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;
        rocketJumpTimer -= Time.deltaTime;
        if (isStaggered && staggerTimer <= 0)
        {
            CallEndStaggerAnimation();
        }

        if (player != null)
        {
            distanceToPlayer = Vector2.Distance(player.position, transform.position);
        }

        AnimationControl();
        IsGroundedCheck();
        if (inAttackState || isDead || isStaggered) return;

        // Increment ultimate usage weights (makes them more available over time)
        if (ult1UsageWeight > 0) ult1UsageWeight--;
        if (ult2UsageWeight > 0) ult2UsageWeight--;
        if (ult3UsageWeight > 0) ult3UsageWeight--;

        DecideAttack();
    }

    public void SetUpAndSpawnParticle(int particleNum)
    {
        ParticleSystem newParticle = null;
        if (particleNum == 1)
        {
            newParticle = dashParticle;
        }
        else if (particleNum == 2)
        {
            newParticle = shootStandingParticle;
        }
        else if (particleNum == 3)
        {
            newParticle = shootAirDiagonalParticle;
        }
        else if (particleNum == 4)
        {
            newParticle = shootAirStraightParticle;
            newParticle.Emit(1);
            return;
        }
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.rotation3D = new(0f, isFacingRight ? 0f : 180f);
        newParticle.Emit(emitParams, 1);
        Debug.Log("Spawned particle");
    }

    protected void AnimationControl()
    {
        if (inAttackState || isDead || isStaggered) return;
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
            if (Mathf.Abs(rb.linearVelocity.x) > 2.5f)
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
        bool wasInAir = isInAir;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheckBox.bounds.center, groundCheckBox.bounds.size, 0f, groundLayer);
        isInAir = colliders.Length == 0;

        // Reset consecutive aerial shots when landing
        if (wasInAir && !isInAir)
        {
            consecutiveAerialShots = 0;
        }
    }

    private void MoveTowards(Vector2 target, float speed = -1f)
    {
        if (speed < 0) speed = moveSpeed;
        float deltaX = target.x - transform.position.x;
        if (Mathf.Abs(deltaX) > 0.5f)
        {
            float direction = Mathf.Sign(deltaX);
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

            if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
                FlipSprite();
        }
        else
        {
            ZeroVelocity();
        }
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
        health -= dmg;
        if (damageText != null)
        {
            GameObject dmgText = Instantiate(damageText, transform.position, transform.rotation);
            dmgText.GetComponentInChildren<DamageText>().Initialize(new(knockbackForce.x, 5f), dmg, new Color(0.8862745f, 0.3660145f, 0.0980392f, 1f), Color.red);
        }
        hpBarInteractor.UpdateHealthVisual();
        StartCoroutine(base.DamageFlash(0.2f));
        // audioManager?.PlayHurt();
        if (!isStaggered) currentStaggerDamage += dmg;
        if (currentStaggerDamage >= damageToStagger)
        {
            StartStagger();
            currentStaggerDamage = 0;
        }
        if (health <= 0)
        {
            isInvincible = true;
            StartCoroutine(Death());
        }
        else if (health <= maxHealth * 0.66f && phaseNum < 2) //swap phases
        {
            phaseNum = 2;
            moveSpeed = 3f;
            maxAmmo = originalMaxAmmo + 2;
            maxAerialShotsConsecutively = originalMaxAerialShotsConsecutively + 1;
            maxConsecutiveUltimates = originalMaxConsecutiveUltimates + 1;
            ultCooldown = originalUltCooldown - 1f;
            // Instantiate(phaseChangeParticles, transform.position, Quaternion.identity);
        }
        else if (health <= maxHealth * 0.33f && phaseNum < 3)
        {
            phaseNum = 3;
            moveSpeed = 4f;
            maxAmmo = originalMaxAmmo + 4;
            maxAerialShotsConsecutively = originalMaxAerialShotsConsecutively + 2;
            maxConsecutiveUltimates = originalMaxConsecutiveUltimates + 2;
            ultCooldown = originalUltCooldown - 2f;
            // Instantiate(phaseChangeParticles, transform.position, Quaternion.identity);
        }
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
        ChangeAnimationState("Death");
        // audioManager?.StopRunLoop();
        // audioManager?.PlayDeath();
        ZeroVelocity(); // Stop all movement
        isStaggered = false;
        if (dropItemsOnDeath != null)
        {
            dropItemsOnDeath.DropItems();
        }
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
        // Boss always returns to spawn point, ignoring checkpoint position
        base.Respawn(null, facingRight);

        // Reset all AI state variables
        isDead = false;
        isAttacking = 0;
        inAttackState = true; // Keep in attack state until entrance animation completes
        currentState = "Idle";
        isStaggered = false;
        isInvincible = true; // Will be set to false by WaitForNearbyPlayer coroutine
        currentStaggerDamage = 0;
        consecutiveUltimates = 0;

        // Reset phase to 1 (phase should not persist across respawns)
        phaseNum = 1;
        maxAmmo = originalMaxAmmo;
        maxAerialShotsConsecutively = originalMaxAerialShotsConsecutively;
        maxConsecutiveUltimates = originalMaxConsecutiveUltimates;
        ultCooldown = originalUltCooldown;
        // Reset all timers
        meleeTimer = 0f;
        rangedTimer = 0f;
        rocketJumpTimer = 0f;
        ultTimer = 0f;
        staggerTimer = 0f;

        // Reset ammo
        currentAmmo = maxAmmo;

        // Reset aerial shot tracking
        consecutiveAerialShots = 0;

        // Reset ultimate usage weights
        ult1UsageWeight = 0;
        ult2UsageWeight = 0;
        ult3UsageWeight = 0;

        // Reset movement and physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Stop any active coroutines
        StopAllCoroutines();

        // Reset animation state
        if (anim != null)
        {
            anim.Play("Idle");
        }

        // Hide health bar initially
        if (hpBarInteractor != null)
        {
            hpBarInteractor.ShowHealthBar(false);
            // Update health bar visual to reflect full health (will be shown when entrance completes)
            hpBarInteractor.UpdateHealthVisual();
        }

        // Restart the entrance sequence
        StartCoroutine(WaitForNearbyPlayer());
    }

    public override void FlipSprite()
    {
        base.FlipSprite();
    }

    public void EndAttack()
    {
        Debug.Log("End attack");
        inAttackState = false;
        isAttacking = 0;
    }

    public void ChangeAttackNumber(int newAttackNum)
    {
        isAttacking = newAttackNum;
    }

    public void EndUlt()
    {
        ChangeAnimationState("UltRecovery");
    }

    public void SpawnDynamite()
    {
        if (dynamitePrefab == null)
        {
            Debug.LogError("BomberBoss: Dynamite prefab is not assigned!");
            return;
        }

        if (SpawnBounds == null)
        {
            Debug.LogError("BomberBoss: Dynamite spawn bounds (BoxCollider2D) is not assigned!");
            return;
        }

        Bounds spawnBounds = SpawnBounds.bounds;

        for (int i = 0; i < dynamiteCount; i++)
        {
            // Generate random X position within bounds, but spawn at top Y
            float randomX = Random.Range(spawnBounds.min.x, spawnBounds.max.x);
            float spawnY = spawnBounds.max.y - topSpawnOffset; // Spawn near the top
            Vector2 spawnPosition = new Vector2(randomX, spawnY);

            // Spawn dynamite
            GameObject newDynamite = Instantiate(dynamitePrefab, spawnPosition, Quaternion.identity);

            // Calculate initial velocity (downward with slight random horizontal)
            float randomHorizontal = Random.Range(horizontalVelocityRange.x, horizontalVelocityRange.y);
            Vector2 initVelocity = new Vector2(randomHorizontal, -dynamiteFallSpeed);

            // Initialize the dynamite
            Dynamite dynamiteScript = newDynamite.GetComponent<Dynamite>();
            if (dynamiteScript != null)
            {
                dynamiteScript.Initialize(initVelocity);
            }
            else
            {
                Debug.LogWarning("BomberBoss: Dynamite prefab doesn't have Dynamite component!");
            }
        }
    }

    public void EndAttackState()
    {
        inAttackState = false;
    }

    public void AddVelocity(float addedVelocity)
    {
        rb.linearVelocity += new Vector2(addedVelocity * (isFacingRight ? 1 : -1), 0f);
    }

    public void AddYVelocity(float addedVelocity)
    {
        rb.linearVelocity += new Vector2(0f, addedVelocity);
    }

    public void ZeroVelocity()
    {
        // If in air, preserve vertical velocity to allow natural falling

        rb.linearVelocity = Vector2.zero;
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
    }

    public void UseAmmo()
    {
        currentAmmo--;
    }


    public void StartReload()
    {
        currentAmmo = 0;
        inAttackState = true;
        isAttacking = -1;
        ChangeAnimationState("Reload");
    }

    public void StartStagger()
    {
        staggerTimer = staggerTime;
        isStaggered = true;
        inAttackState = false;
        isAttacking = 0;
        ChangeAnimationState("Stagger");
    }

    public void CallEndStaggerAnimation()
    {
        ChangeAnimationState("StaggerRecovery");
    }
    public void EndStagger()
    {
        isStaggered = false;
    }
    public void SetUpAttackHitboxes(int attackNum)
    {
        FaceTowardsPlayer();
        ZeroVelocity();
        isAttacking = attackNum;
        inAttackState = true;
        if (attackNum == 1)
        {
            attackHitboxScript.CustomizeHitbox(attackHitboxes[0]);
            ChangeAnimationState("Melee1");
        }
        else if (attackNum == 2)
        {
            attackHitboxScript.CustomizeHitbox(attackHitboxes[1]);
            ChangeAnimationState("Dash");
        }
    }

    public void EndMelee1Chain()
    {
        if (phaseNum == 1)
        {
            ChangeAnimationState("Melee1Recovery");
        }
        else
        {
            SetUpAttackHitboxes(2); //chain into dash attack
        }
        meleeTimer = meleeCooldown;
    }

    public void DecideAttack()
    {
        if (player == null) return;

        List<int> availableAttacks = new List<int>();

        // 1. Reload (highest priority if no ammo and grounded)
        if (currentAmmo <= 0 && !isInAir)
        {
            StartReload();
            return;
        }

        // 2. Melee attack (if within melee distance and grounded and cooldown ready)
        if (distanceToPlayer <= meleeDistance && !isInAir && meleeTimer <= 0)
        {
            availableAttacks.Add(1); // Melee
        }

        // 3. Ranged attack (if within ranged distance but NOT melee distance, has ammo, and cooldown ready)
        if (distanceToPlayer > meleeDistance && distanceToPlayer <= rangedDistance && currentAmmo > 0 && rangedTimer <= 0)
        {
            availableAttacks.Add(3); // Ranged
        }

        // 4. Rocket jump (if within ranged distance AND ammo >= 5 AND cooldown ready)
        if (distanceToPlayer <= rangedDistance && currentAmmo >= 5 && !isInAir && rocketJumpTimer <= 0)
        {
            availableAttacks.Add(4); // Rocket jump
        }

        // 5. Aerial rocket attacks (if in the air, above player, consecutive shots < max, has ammo, and cooldown ready)
        if (isInAir && transform.position.y > player.position.y + 1f &&
            consecutiveAerialShots < maxAerialShotsConsecutively &&
            currentAmmo > 0 && rangedTimer <= 0)
        {
            availableAttacks.Add(5); // Aerial rocket
        }

        // 6. Ultimates (only in phase 2+, off cooldown, and not actively using ultimate)
        if (phaseNum >= 2 && ultTimer <= 0 && isAttacking < 5)
        {
            // Add available ultimates with weights
            List<int> availableUlts = new List<int>();
            List<float> ultWeights = new List<float>();

            // All ultimates are available, but weighted by usage
            // Use 10, 11, 12 to avoid conflict with aerial rocket (5)
            availableUlts.Add(10); // Ult 1 (Dynamite)
            ultWeights.Add(ult1UsageWeight + 1f); // +1 to avoid zero weight

            availableUlts.Add(11); // Ult 2 (Landmines)
            ultWeights.Add(ult2UsageWeight + 1f);

            availableUlts.Add(12); // Ult 3 (Nuke)
            ultWeights.Add(ult3UsageWeight + 1f);

            // Choose ultimate based on weighted random
            if (availableUlts.Count > 0)
            {
                float totalWeight = 0f;
                foreach (float weight in ultWeights)
                {
                    totalWeight += weight;
                }

                float randomValue = Random.Range(0f, totalWeight);
                float currentWeight = 0f;
                int chosenUlt = availableUlts[0];

                for (int i = 0; i < availableUlts.Count; i++)
                {
                    currentWeight += ultWeights[i];
                    if (randomValue <= currentWeight)
                    {
                        chosenUlt = availableUlts[i];
                        break;
                    }
                }

                availableAttacks.Add(chosenUlt);
            }
        }

        // Choose an attack from available options
        if (availableAttacks.Count > 0)
        {
            // Priority order: Melee > Ranged > Rocket Jump > Aerial Rocket > Ultimates
            int chosenAttack = availableAttacks[0]; // Default to first

            // Check for melee first (highest priority)
            if (availableAttacks.Contains(1))
            {
                chosenAttack = 1;
            }
            // Then choose between ranged and rocket jump (80% chance for ranged)
            else if (availableAttacks.Contains(3) && availableAttacks.Contains(4))
            {
                // Both ranged and rocket jump are available - 80% chance for ranged
                float random = Random.Range(0f, 1f);
                if (random < 0.8f)
                {
                    chosenAttack = 3; // Ranged (80% chance)
                }
                else
                {
                    chosenAttack = 4; // Rocket jump (20% chance)
                }
            }
            // Only ranged available
            else if (availableAttacks.Contains(3))
            {
                chosenAttack = 3;
            }
            // Only rocket jump available
            else if (availableAttacks.Contains(4))
            {
                chosenAttack = 4;
            }
            // Then aerial rocket
            else if (availableAttacks.Contains(5))
            {
                chosenAttack = 5;
            }
            // Finally ultimates (randomly choose from available)
            else if (availableAttacks.Count > 0)
            {
                // Filter to only ultimates (10, 11, 12)
                List<int> ultOptions = availableAttacks.FindAll(a => a >= 10 && a <= 12);
                if (ultOptions.Count > 0)
                {
                    chosenAttack = ultOptions[Random.Range(0, ultOptions.Count)];
                }
            }

            // Execute chosen attack
            switch (chosenAttack)
            {
                case 1: // Melee
                    SetUpAttackHitboxes(1);
                    break;
                case 3: // Ranged
                    ShootRocket();
                    break;
                case 4: // Rocket jump
                    RocketJump();
                    break;
                case 5: // Aerial rocket (boss is grounded but above player)
                    consecutiveAerialShots++;
                    ShootRocket(); // ShootRocket() now handles this case automatically
                    break;
                case 10: // Ult 1 (Dynamite) - isAttacking = 5
                    StartUlt(1);
                    break;
                case 11: // Ult 2 (Landmines) - isAttacking = 6
                    StartUlt(2);
                    break;
                case 12: // Ult 3 (Nuke) - isAttacking = 7
                    StartUlt(3);
                    break;
            }
        }
        else
        {
            // No attacks available - move towards player until within melee range
            if (distanceToPlayer > meleeDistance && !isInAir)
            {
                MoveTowards(player.position);
            }
            else
            {
                // Only zero velocity if grounded (preserve fall velocity when in air)
                if (!isInAir)
                {
                    ZeroVelocity();
                }
                else
                {
                    // Only zero horizontal velocity when in air
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                }
            }
        }
    }

    public float DetermineXDistanceToPlayer()
    {
        float xDistanceToPlayer = player.position.x - transform.position.x;
        return Mathf.Abs(xDistanceToPlayer);
    }

    public void ShootRocket()
    {
        if (currentAmmo <= 0 || rangedTimer > 0) return;

        // Use aerial animations only if boss is actually in the air
        if (isInAir)
        {
            //if the enemy is in the air or above player can do 2 things: either shoot straight down or shoot diagonally.
            //if the player is to the left of the enemy, shoot diagonally to the right(using the block back to go towards the player)
            //if the enemy is to the right, shoot to the left.
            //if it is relatively below the player in x value, shoot straight down.
            rangedTimer = aerialRangedCooldown;
            // Only zero horizontal velocity, preserve vertical velocity so boss continues falling
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            FaceTowardsPlayer();
            isAttacking = 3;
            inAttackState = true;

            float xDistanceToPlayer = DetermineXDistanceToPlayer();
            if (xDistanceToPlayer > 1.2f)
            {  //do diagonal.
                FaceTowardsPlayer();
                FlipSprite(); //face away from player.
                ChangeAnimationState("AerialDiagonalShot");
            }
            else
            {
                ChangeAnimationState("AerialDownShot");
            }
        }
        else
        {
            rangedTimer = rangedCooldown;
            UseAmmo();
            ZeroVelocity();
            FaceTowardsPlayer();
            isAttacking = 3;
            inAttackState = true;
            attackHitboxScript.CustomizeHitbox(attackHitboxes[2]);
            ChangeAnimationState("GroundedRanged");
        }
    }

    public void RocketJump()
    {
        rangedTimer = 1f;
        rocketJumpTimer = rocketJumpCooldown;
        ZeroVelocity();
        FaceTowardsPlayer();
        isAttacking = 4;
        inAttackState = true;
        ChangeAnimationState("RocketJump");
    }
    public void DoUlt()
    {
        switch (isAttacking)
        {
            case 5:
                SpawnDynamite();
                ult1UsageWeight = 10; // Increase weight (make it less likely to be chosen next time)
                break;
            case 6:
                SpawnLandMines();
                ult2UsageWeight = 10;
                break;
            case 7:
                SpawnNuke();
                ult3UsageWeight = 50;
                break;
            default:
                Debug.LogError("BomberBoss: Invalid ultimate attack number: " + isAttacking);
                break;
        }
        consecutiveUltimates++;
        if (consecutiveUltimates >= maxConsecutiveUltimates)
        {
            consecutiveUltimates = 0;
            ultTimer = ultCooldown;
        }
        else
        {
            ultTimer = 3f;
        }
    }
    public void StartUlt(int ultNum)
    {
        if (ultTimer > 0) return;
        FaceTowardsPlayer();
        ZeroVelocity();
        isAttacking = 4 + ultNum;
        inAttackState = true;
        ChangeAnimationState("Ultcall");
    }

    public void SpawnLandMines()
    {
        if (landminePrefab == null)
        {
            Debug.LogError("BomberBoss: Landmine prefab is not assigned!");
            return;
        }

        if (SpawnBounds == null)
        {
            Debug.LogError("BomberBoss: Landmine spawn bounds (BoxCollider2D) is not assigned!");
            return;
        }

        Bounds spawnBounds = SpawnBounds.bounds;

        for (int i = 0; i < landMineCount; i++)
        {
            // Generate random X position within bounds
            float randomX = Random.Range(spawnBounds.min.x, spawnBounds.max.x);
            // Spawn at the bottom of the bounds (ground level) instead of top
            float spawnY = spawnBounds.min.y;
            Vector2 spawnPosition = new Vector2(randomX, spawnY);

            // Spawn landmine
            GameObject newLandMine = Instantiate(landminePrefab, spawnPosition, Quaternion.identity);
            Landmine landmineScript = newLandMine.GetComponent<Landmine>();
            if (landmineScript != null)
            {
                landmineScript.InitializeLandMine();
            }
            else
            {
                Debug.LogWarning("BomberBoss: Landmine prefab doesn't have Landmine component!");
            }
        }
    }

    public void SpawnNuke()
    {
        GameObject newNuke = Instantiate(nukePrefab, transform.position, Quaternion.identity);
    }

    public void SpawnRocket(float rawAngle)
    {
        Transform spawnPoint = null;
        float angle = rawAngle;
        if (rawAngle == 315f)
        {
            Debug.Log("Spawn diagonal rocket");
            spawnPoint = airDiagonalRocketProjSpawnPt;
            angle = isFacingRight ? 315f : 225f;
        }
        else if (rawAngle == 0f)
        {
            Debug.Log("Spawn standing rocket");
            spawnPoint = standRocketProjSpawnPt;
            angle = isFacingRight ? 0f : 180f;
        }
        else
        {
            Debug.Log("Spawn straight down rocket");
            spawnPoint = airStraightRocketProjSpawnPt;
            // For straight down, use -90 degrees (or 270, both point straight down)
            angle = -90f; // Straight down
        }

        if (spawnPoint == null)
        {
            Debug.LogError($"BomberBoss: Spawn point is null for angle {rawAngle}! Check spawn point assignments.");
            return;
        }

        Vector3 spawnPosition = spawnPoint.position;

        GameObject newRocket = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        newRocket.GetComponent<RPGRocket>().Initialize(10, 10, angle, 3f);
    }

    public void SpawnRocketJumpExplosion()
    {
        GameObject newRocketJumpExplosion = Instantiate(rocketJumpExplosionPrefab, transform.position, Quaternion.identity);
        newRocketJumpExplosion.GetComponent<ExplosionCloud>().Initialize(null);
    }
}
