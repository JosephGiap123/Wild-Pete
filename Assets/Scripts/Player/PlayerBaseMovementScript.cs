using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Cinemachine;

public abstract class BasePlayerMovement2D : MonoBehaviour, IHasFacing
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpPower = 6f;
    public float minJumpPower = 3f; // Minimum jump height if button is tapped
    protected float horizontalInput;
    public bool isFacingRight = true;
    public bool IsFacingRight => isFacingRight; // IHasFacing implementation
    protected bool weaponEquipped = false;
    protected bool isGrounded;
    protected bool isAttacking = false;
    protected bool isCrouching = false;
    protected bool isJumping = false;
    protected float jumpTimeCounter;
    protected int jumpsRemaining = 1; // Track remaining jumps (starts at 1)

    [Header("Combat Settings")]
    public int maxHealth = 20;
    [SerializeField] public int ammoCount = 5;
    [SerializeField] public int maxAmmo = 5;
    public int maxAttackChain = 3;
    protected int attackCount = 0;
    public float comboResetTime = 3f;
    protected Coroutine attackResetCoroutine;
    protected bool isReloading = false;
    protected Coroutine reloadCoroutine;
    bool hyperArmor = false;
    // [SerializeField] protected float attackCooldown = 1.2f;
    // protected float attackTimer = 0f;

    [Header("Hurt Settings")] //clauded code here
    [SerializeField] protected float invincibilityTime = 0.1f; // I-frames after hurt
    [SerializeField] protected float hurtStateTimeout = 1.0f; // Max time to stay in hurt state (safety fallback)
    protected bool isHurt = false;
    public bool IsHurt => isHurt;
    protected bool isInvincible = false;
    protected bool isDead = false;
    public bool IsDead => isDead;
    [SerializeField] protected float deathAnimationDuration = 4f; // How long death animation plays
    [SerializeField] protected float deathYThreshold = -50f; // Y position below which player dies (falling into pit)

    [Header("Dash Settings")]
    public bool isDashing { get; protected set; } = false;
    public float dashingPower = 12f;
    public float dashingTime = 0.3f;
    public float slidePower = 6f;
    // public float dashingCooldown = 3f;
    // public float slidingCooldown = 0.75f;
    protected Coroutine slideCoroutine;
    // protected Coroutine dashCooldownCoroutine;
    protected Coroutine attackCoroutine;
    protected Coroutine attackTimeoutCoroutine;
    protected Coroutine hurtTimeoutCoroutine;

    [Header("Energy Settings")]
    [SerializeField] protected float dashingEnergyCost = 5f;
    [SerializeField] protected float slidingEnergyCost = 2f;
    [SerializeField] protected float weaponlessMeleeEnergyCost = 1f;
    [SerializeField] protected float groundAttackEnergyCost = 1f;
    [SerializeField] protected float aerialAttackEnergyCost = 1f;
    [SerializeField] protected float crouchingAttackEnergyCost = 1f;
    [SerializeField] protected float jumpEnergyCost = 1f;

    [Header("Aerial Settings")]
    // [SerializeField] protected float aerialCooldown = 1f;
    // protected float aerialTimer = 0f;

    [Header("Attack Safety")]
    [SerializeField] protected float maxAttackDuration = 0.9f; // watchdog timeout for attacks

    [Header("Wall Slide Settings")]
    [SerializeField] protected Transform wallRay;
    [SerializeField] protected LayerMask wallMask;
    [SerializeField] protected float wallSlideSpeed = 0.9f;
    protected bool isTouchingWall;
    protected bool isWallSliding = false;
    protected float castDistance = 0.3f;

    [Header("References")]
    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D RB => rb;
    [SerializeField] protected BoxCollider2D boxCol;
    [SerializeField] protected BoxCollider2D groundCheck;
    [SerializeField] protected BoxCollider2D roofCheck; //check if u can uncrouch
    [SerializeField] protected LayerMask groundMask;
    // [SerializeField] protected TrailRenderer trail;
    [SerializeField] protected Transform bulletOrigin;
    [SerializeField] protected GameObject bullet; // Default bullet (fallback if no custom projectile)
    [SerializeField] protected PlayerAttackHitbox hitboxManager;
    [SerializeField] protected AnimScript animatorScript;
    [SerializeField] protected InteractionDetection interactor;
    [SerializeField] protected GameObject damageText;
    [SerializeField] protected GameObject dynamitePrefab;
    public AttackHitboxInfo[] attackHitboxes;

    protected GameObject bulletInstance;

    // Abstract properties for character-specific values
    protected abstract float CharWidth { get; }
    protected abstract Vector2 CrouchOffset { get; }
    protected abstract Vector2 CrouchSize { get; }
    protected abstract Vector2 StandOffset { get; }
    protected abstract Vector2 StandSize { get; }

    [Header("Particle References")]
    [SerializeField] protected ParticleSystem slideParticle;
    [SerializeField] protected ParticleSystem jumpParticle;
    [SerializeField] protected ParticleSystem dashParticle;
    //events
    public event Action<int, int> OnAmmoChanged;
    public event Action PlayerDied;

    [SerializeField] protected InputBroadcaster inputBroadcaster;

    protected virtual void Awake()
    {
        // Make ground check slightly smaller than character width
        if (groundCheck != null)
        {
            groundCheck.size = new(CharWidth * 0.9f, groundCheck.size.y);
        }

        // Subscribe to respawn event
        GameRestartManager.CharacterRespawned += OnRespawn;

        // Subscribe to stat changes
        if (StatsManager.instance != null)
        {
            StatsManager.instance.OnStatChanged += HandleStatChanged;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from death event when object is disabled/destroyed
        if (HealthManager.instance != null)
        {
            HealthManager.instance.OnPlayerDeath -= Die;
        }
    }

    protected virtual void OnDestroy()
    {
        // Unsubscribe from respawn event
        GameRestartManager.CharacterRespawned -= OnRespawn;

        // Unsubscribe from stat changes
        if (StatsManager.instance != null)
        {
            StatsManager.instance.OnStatChanged -= HandleStatChanged;
        }

        // Also unsubscribe from death event in OnDestroy as a safety net
        if (HealthManager.instance != null)
        {
            HealthManager.instance.OnPlayerDeath -= Die;
        }
    }

    protected virtual void Start()
    {
        // Initialize StatsManager with player's base stats
        // Use a coroutine to ensure StatsManager is ready
        StartCoroutine(InitializeStatsManager());
    }

    private IEnumerator InitializeStatsManager()
    {
        // Wait a frame to ensure StatsManager is initialized
        yield return null;

        if (StatsManager.instance != null)
        {
            StatsManager.instance.InitializeStats(
                maxHealth,
                maxAmmo,
                moveSpeed,
                1, // jumpCount (base is 1)
                dashingPower,
                slidePower,
                10f, // bulletSpeed (default, adjust if needed)
                0, // bulletCount (default)
                0, // weaponless melee attack (default)
                0, // melee attack (default)
                0, // rangedAttack (default)
                0  // universalAttack (default)
            );

            // Initialize jumps remaining from StatsManager
            jumpsRemaining = StatsManager.instance.jumpCount;
        }
    }

    private void HandleStatChanged(EquipmentSO.Stats stat, float value)
    {
        if (StatsManager.instance == null) return;

        switch (stat)
        {
            case EquipmentSO.Stats.MaxHealth:
                maxHealth = StatsManager.instance.maxHealth;
                // Update HealthManager if it exists
                if (HealthManager.instance != null)
                {
                    HealthManager.instance.SetMaxHealth(maxHealth);
                    // If current health exceeds new max, cap it
                    int currentHealth = HealthManager.instance.GetCurrentHealth();
                    if (currentHealth > maxHealth)
                    {
                        HealthManager.instance.SetHealth(maxHealth);
                    }
                }
                break;

            case EquipmentSO.Stats.MovementSpeed:
                moveSpeed = StatsManager.instance.MovementSpeed;
                break;

            case EquipmentSO.Stats.DashSpeed:
                dashingPower = StatsManager.instance.dashSpeed;
                break;

            case EquipmentSO.Stats.SlideSpeed:
                slidePower = StatsManager.instance.slideSpeed;
                break;

            case EquipmentSO.Stats.MaxAmmo:
                maxAmmo = StatsManager.instance.maxAmmo;
                // If current ammo exceeds new max, cap it
                if (ammoCount > maxAmmo)
                {
                    ammoCount = maxAmmo;
                }
                OnAmmoChanged?.Invoke(ammoCount, maxAmmo);
                break;

            case EquipmentSO.Stats.JumpCount:
                // Update max jump count from StatsManager
                // Always update jumpsRemaining to the new max when jump count changes
                // This ensures boots work whether equipped on ground or in air
                jumpsRemaining = StatsManager.instance.jumpCount;
                break;

            case EquipmentSO.Stats.BulletSpeed:
            case EquipmentSO.Stats.BulletCount:
            case EquipmentSO.Stats.MeleeAttack:
            case EquipmentSO.Stats.RangedAttack:
            case EquipmentSO.Stats.UniversalAttack:
                // These stats would be used in attack calculations
                // You can add fields to store them if needed
                break;
        }
    }

    // Handles respawning the player at the checkpoint location.
    // This is public so GameRestartManager can call it directly on the current player
    public virtual void OnRespawn(Vector2 checkpointLocation)
    {
        // Check if this object has been destroyed
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("OnRespawn called on destroyed player object, ignoring");
            return;
        }

        // Restore checkpoint state first
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RestoreCheckpoint();
        }

        // Reset player state
        isDead = false;
        isHurt = false;
        isInvincible = false;
        isAttacking = false;
        isDashing = false;
        isWallSliding = false;
        isCrouching = false;
        isJumping = false;
        isGrounded = false;
        isReloading = false;
        attackCount = 0;

        // Stop all movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = checkpointLocation;
        }

        // Check again before accessing transform
        if (this != null && gameObject != null)
        {
            transform.position = checkpointLocation;
        }
        else
        {
            Debug.LogError("Player object destroyed during OnRespawn, cannot set position");
            return;
        }

        // Restore health and ammo from checkpoint
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint())
        {
            var checkpointData = CheckpointManager.Instance.GetCheckpointData();
            if (checkpointData != null)
            {
                HealthManager.instance.SetHealth(checkpointData.playerHealth);
                ammoCount = checkpointData.playerAmmo;
                maxAmmo = checkpointData.playerMaxAmmo;
                OnAmmoChanged?.Invoke(ammoCount, maxAmmo);
            }
        }
        else
        {
            HealthManager.instance.SetHealth(maxHealth);
            ammoCount = maxAmmo;
            OnAmmoChanged?.Invoke(ammoCount, maxAmmo);
        }

        // Disable hitbox immediately on respawn
        if (hitboxManager != null)
        {
            hitboxManager.DisableHitbox();
        }

        // Re-enable player
        gameObject.SetActive(true);

        // Cancel any active coroutines
        CancelAllActions();

        // Unpause game
        PauseController.SetPause(false);

        Debug.Log($"Player respawned at checkpoint: {checkpointLocation}");
    }

    // Abstract methods for character-specific behavior
    protected abstract void SetupGroundAttack(int attackIndex);
    protected abstract void SetupCrouchAttack();
    protected abstract void SetupAerialAttack();

    private void OnEnable()
    {
        HealthManager.instance.OnPlayerDeath += Die;
    }
    protected virtual void Update()
    {
        if (PauseController.IsGamePaused)
        {
            return;
        }

        // Check if player has fallen below death threshold
        if (!isDead && transform.position.y < deathYThreshold)
        {
            Debug.Log($"Player fell below death threshold ({deathYThreshold}). Player Y: {transform.position.y}");
            // Kill player through HealthManager event system for consistency
            HealthManager.instance.KillPlayer();
            return;
        }

        // attackTimer -= Time.deltaTime;
        // aerialTimer -= Time.deltaTime;

        if (isAttacking) return;
        AnimationControl();
        if (isHurt || isReloading) return;
        HandleMovement();
        if (!isDashing && !isDead)
        {
            HandleInput();
            HandleFlip();
        }
    }

    protected virtual void HandleInput()
    {
        if (isDead || isAttacking || isReloading) return;
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Interact]) && !isDashing && isGrounded)
        {
            interactor.OnInteract();
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Melee]) && !isWallSliding)
        {
            Attack();
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Throw]) && isGrounded && PlayerInventory.instance.HasItem("Dynamite") > 0)
        {
            attackCoroutine = StartCoroutine(ThrowAttack());
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Hotkey1]) && isGrounded && PlayerInventory.instance.HasItem("Bandaid") > 0)
        {
            PlayerInventory.instance.UseItem("Bandaid", 1);
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Hotkey2]) && isGrounded && PlayerInventory.instance.HasItem("Medkit") > 0)
        {
            PlayerInventory.instance.UseItem("Medkit", 1);
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Ranged]) && isGrounded && !isAttacking && ammoCount > 0 && PlayerInventory.instance.equipmentSlots[3] != null && !PlayerInventory.instance.equipmentSlots[3].IsEmpty())
        {
            attackCoroutine = StartCoroutine(RangedAttack());
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Reload]) && isGrounded && !isCrouching && !isDashing && !isAttacking && !isReloading && ammoCount < maxAmmo && PlayerInventory.instance.HasItem("Ammo") > 0)
        {
            reloadCoroutine = StartCoroutine(Reload());
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Dash]) && !isAttacking && !isWallSliding)
        {
            if (!isCrouching && EnergyManager.instance.UseEnergy(dashingEnergyCost))
            {
                isDashing = true;
                slideCoroutine = StartCoroutine(Dash());
                // dashCooldownCoroutine = StartCoroutine(DashCooldown(dashingCooldown));
            }
            else if (isCrouching && EnergyManager.instance.UseEnergy(slidingEnergyCost))
            {
                isDashing = true;
                StartCoroutine(Slide());
                // dashCooldownCoroutine = StartCoroutine(DashCooldown(slidingCooldown));
            }
            else
            {
                Debug.Log("Not enough energy to dash or slide");
            }

        }
        if (!isAttacking && isGrounded && Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Unequip]) && PlayerInventory.instance.equipmentSlots[2] != null && !PlayerInventory.instance.equipmentSlots[2].IsEmpty())
        {
            Debug.Log(PlayerInventory.instance.equipmentSlots[2].GetEquippedItem().itemName);
            PlayerInventory.instance.equipmentSlots[2].UnequipItem();
        }
    }
    protected virtual void HandleMovement()
    {
        if (isDead) return;
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) && jumpsRemaining > 0 && EnergyManager.instance.UseEnergy(jumpEnergyCost))
        {
            isJumping = true;
            isGrounded = false;
            jumpsRemaining--;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            jumpParticle.Emit(1);
            if (isDashing && isCrouching)
            { //dash cancel
                if (slideCoroutine != null)
                    StopCoroutine(slideCoroutine);
                // if (dashCooldownCoroutine != null)
                //     StopCoroutine(dashCooldownCoroutine);
                isDashing = false;
                // StartCoroutine(DashCooldown(slidingCooldown));
            }
        }

        if (isDashing) return;

        // Release jump button early for shorter jump
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
            // Apply minimum jump power if released early
            if (rb.linearVelocity.y > minJumpPower)
            {
                rb.linearVelocity = new(rb.linearVelocity.x, minJumpPower);
            }
        }

        if (Input.GetAxisRaw("Vertical") == -1 && isGrounded)
        {
            isCrouching = true;
            boxCol.offset = CrouchOffset;
            boxCol.size = CrouchSize;
        }
        else if (Input.GetAxisRaw("Vertical") != -1 && isCrouching)
        {
            //check if there is roof above.
            if ((Physics2D.OverlapAreaAll(roofCheck.bounds.min, roofCheck.bounds.max, groundMask).Length + Physics2D.OverlapAreaAll(roofCheck.bounds.min, roofCheck.bounds.max, wallMask).Length) == 0)
            { //essentially, check if either wall or floor above player.
                isCrouching = false;
                boxCol.offset = StandOffset;
                boxCol.size = StandSize;
            }
        }
    }

    protected virtual void AnimationControl()
    {
        if (isDead)
        {
            animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.DeathWep : playerStates.Death);
            return;
        }
        if (isHurt)
        {
            animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.HurtWep : playerStates.Hurt);
            return;
        }
        if (isReloading)
        {
            animatorScript.ChangeAnimationState(playerStates.Reload);
            return;
        }
        if (isWallSliding)
        {
            animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.WallSlideWep : playerStates.WallSlide);
            return;
        }
        if (isDashing)
        {
            if (isCrouching)
            {
                animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.SlideWep : playerStates.Slide);
                return;
            }
            else
            {
                animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.DashWep : playerStates.Dash);
                return;
            }
        }
        if (!isAttacking)
        {
            if (!isGrounded)
            {
                if (rb.linearVelocity.y > 0.1f)
                {
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.RisingWep : playerStates.Rising);
                    return;
                }
                else if (rb.linearVelocity.y < -0.1f)
                {
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.FallingWep : playerStates.Falling);
                    return;
                }
            }
            else
            {
                if (isCrouching)
                {
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.CrouchWep : playerStates.Crouch);
                    return;
                }
                else if (Mathf.Abs(rb.linearVelocity.x) > 0.2f && horizontalInput != 0)
                {
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.RunWep : playerStates.Run);
                    return;
                }
                else
                {
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.IdleWep : playerStates.Idle);
                    return;
                }

            }
        }
    }

    protected virtual void Attack()
    {
        if (!weaponEquipped)
        { //punching
            if (isCrouching || isJumping || !isGrounded || !EnergyManager.instance.UseEnergy(weaponlessMeleeEnergyCost)) return;
            if (attackCount >= 3 || attackCount < 0)
                attackCount = 0;
            SetUpPunchAttack(attackCount);
            attackCount++;
            isAttacking = true;
            if (attackResetCoroutine != null)
                StopCoroutine(attackResetCoroutine);
            attackResetCoroutine = StartCoroutine(ResetAttackCountAfterDelay());
            StartAttackWatchdog(maxAttackDuration);
            return;
        }
        if (isGrounded)
        {
            if (isJumping) return; //frame perfect check.
            if (isCrouching)
            {
                if (!EnergyManager.instance.UseEnergy(crouchingAttackEnergyCost)) return;
                SetupCrouchAttack();
                isAttacking = true;
                StartAttackWatchdog(maxAttackDuration);
                return;
            }

            if (!EnergyManager.instance.UseEnergy(groundAttackEnergyCost)) return;
            if (attackCount >= maxAttackChain || attackCount < 0)
                attackCount = 0;
            SetupGroundAttack(attackCount);
            attackCount++;
            isAttacking = true;

            if (attackResetCoroutine != null)
                StopCoroutine(attackResetCoroutine);
            attackResetCoroutine = StartCoroutine(ResetAttackCountAfterDelay());
            StartAttackWatchdog(maxAttackDuration);
        }
        else
        {
            if (EnergyManager.instance.UseEnergy(aerialAttackEnergyCost)) //to replace with energy later
            {
                attackCoroutine = StartCoroutine(AerialAttack());
            }
        }
    }

    public virtual void EndAttack()
    {
        isAttacking = false;
        if (attackTimeoutCoroutine != null)
        {
            StopCoroutine(attackTimeoutCoroutine);
            attackTimeoutCoroutine = null;
        }
        if (hitboxManager != null)
            hitboxManager.DisableHitbox();
    }

    public virtual void EndReload()
    {
        isReloading = false;
    }

    protected virtual void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = new(0, rb.linearVelocity.y);
            return;
        }
        if (isHurt)
        {
            rb.linearVelocity = new(
                Mathf.Lerp(rb.linearVelocity.x, 0, 0.1f),
                rb.linearVelocity.y
            );
            return;
        }
        ;
        // Dashes override normal movement

        CheckWall();
        CheckGround();

        if (isDashing)
        {
            if (isCrouching)
            {
                rb.linearVelocity = new(
                Mathf.Lerp(rb.linearVelocity.x, 0, 0.05f),
                rb.linearVelocity.y);
            }
            return;
        }

        // Wall slide behavior
        if (isWallSliding)
        {
            rb.linearVelocity = new(
                rb.linearVelocity.x,
                Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue)
            );
            return;
        }

        // Grounded attack locks horizontal movement
        if (isAttacking && isGrounded || isReloading)
        {
            rb.linearVelocity = new(0, rb.linearVelocity.y);
            return;
        }

        // Aerial attack dampens horizontal movement gradually
        if (isAttacking && !isGrounded)
        {
            HandleAerialAttackMovement();
            return;
        }

        // Read horizontal input (updated in Update())
        float targetSpeed = horizontalInput * moveSpeed;

        if (isCrouching)
            targetSpeed *= 0.4f;

        if (!isDashing)
        {
            float speedDiff = targetSpeed - rb.linearVelocity.x;
            float accelRate = isGrounded ? 15f : 8f;
            float movementForce = speedDiff * accelRate;

            rb.AddForce(Vector2.right * movementForce);
        }


        // Smooth deceleration when no input
        if (horizontalInput == 0 && isGrounded || isDashing)
        {
            rb.linearVelocity = new(Mathf.Lerp(rb.linearVelocity.x, 0, 0.1f), rb.linearVelocity.y);
        }
    }


    protected virtual void HandleAerialAttackMovement()
    {
        rb.linearVelocity = new(Mathf.Lerp(rb.linearVelocity.x, 0, 0.1f), rb.linearVelocity.y);
    }

    protected virtual void HandleFlip()
    {
        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
            FlipSprite();
    }

    public virtual void FlipSprite()
    {
        bulletOrigin.localRotation = Quaternion.Euler(0, 0, isFacingRight ? 180 : 0);
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    protected virtual void CheckGround()
    {
        if (isJumping && rb.linearVelocity.y > 0.1f)
        {
            isGrounded = false;
            return;  // Don't run the physics check
        }
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;

        // Reset jump state and restore jumps when landing
        if (!wasGrounded && isGrounded)
        {
            isJumping = false;
            // Restore all jumps when landing
            if (StatsManager.instance != null)
            {
                jumpsRemaining = StatsManager.instance.jumpCount;
            }
            else
            {
                jumpsRemaining = 1; // Fallback to 1 if StatsManager not available
            }
        }
    }

    protected virtual void CheckWall()
    {
        Vector2 direction = isFacingRight ? Vector2.left : Vector2.right;
        RaycastHit2D wallHit = Physics2D.Raycast(wallRay.position, direction, castDistance, wallMask);

        isTouchingWall = wallHit.collider != null;

        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && horizontalInput != 0 && !isAttacking)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    protected virtual IEnumerator Dash()
    {
        isInvincible = true;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float power = dashingPower;
        rb.linearVelocity = new(Mathf.Sign(transform.localScale.x) * power, 0f);

        // trail.emitting = true;

        yield return new WaitForSeconds(dashingTime);

        isInvincible = false;
        // trail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    // protected virtual IEnumerator DashCooldown(float cooldown)
    // {
    //     yield return new WaitWhile(() => isDashing);
    //     canDash = false;
    //     yield return new WaitForSeconds(cooldown);
    //     canDash = true;
    // }

    protected virtual IEnumerator Slide()
    {
        isDashing = true;

        rb.linearVelocity = new(Mathf.Sign(transform.localScale.x) * slidePower, 0f);

        yield return new WaitForSeconds(0.5f);
        // Wait while still sliding fast (velocity > 0.1), then exit when it slows down or hits wall
        yield return new WaitWhile(() => Mathf.Abs(rb.linearVelocity.x) > 0.5f);
        isDashing = false;

    }

    protected virtual IEnumerator AerialAttack()
    {
        // aerialTimer = aerialCooldown;
        SetupAerialAttack();
        isAttacking = true;
        StartAttackWatchdog(maxAttackDuration);
        yield return new WaitWhile(() => isAttacking);
    }

    protected virtual IEnumerator ThrowAttack()
    {
        isAttacking = true;
        animatorScript.ChangeAnimationState(playerStates.Throw);
        StartAttackWatchdog(maxAttackDuration);
        yield return new WaitWhile(() => isAttacking);
    }

    public void InitDynamite()
    {
        GameObject thrownDynamite = Instantiate(dynamitePrefab, transform.position, Quaternion.Euler(0, 0, isFacingRight ? 180 : 0));
        thrownDynamite.GetComponent<Dynamite>().Initialize(new(6f * (isFacingRight ? 1f : -1f), 8f));
    }

    protected virtual IEnumerator RangedAttack()
    {
        isAttacking = true;
        ammoCount--;
        OnAmmoChanged?.Invoke(ammoCount, maxAmmo);
        StartAttackWatchdog(maxAttackDuration);
        yield return new WaitWhile(() => isAttacking);
    }

    protected virtual IEnumerator Reload()
    {
        PlayerInventory.instance.UseItem("Ammo", 1);
        isReloading = true;
        yield return new WaitWhile(() => isReloading);
        ammoCount = maxAmmo;
        OnAmmoChanged?.Invoke(ammoCount, maxAmmo);
    }

    public virtual void AddAmmo(int amount)
    {
        ammoCount = Mathf.Min(ammoCount + amount, maxAmmo);
        OnAmmoChanged?.Invoke(ammoCount, maxAmmo);
    }

    public virtual void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        HealthManager.instance.SetMaxHealth(maxHealth);
    }

    public virtual void ReloadAmmo()
    {
        ammoCount = maxAmmo;
        OnAmmoChanged?.Invoke(ammoCount, maxAmmo);
    }

    protected virtual IEnumerator ResetAttackCountAfterDelay()
    {
        yield return new WaitForSeconds(comboResetTime);
        attackCount = 0;
    }

    public virtual void HurtPlayer(int damage, Vector2 knockbackForce, float? knockbackDirection = null, Vector2? hitboxCenter = null)
    {
        // CHEAT: full invulnerability
        if (CheatManager.Instance != null && CheatManager.Instance.invulnerable)
        {
        // Optional: debug log so you can see it's working
        // Debug.Log("HurtPlayer ignored: cheat invulnerability ON");
            return;
        }
        
        // Don't get hurt if invincible or dead
        if (isInvincible || isDead) return;

        // Determine knockback type and direction
        bool useRadialKnockback = hitboxCenter.HasValue;
        float finalKnockbackDir;

        if (useRadialKnockback)
        {
            // Radial knockback: calculate direction from hitbox center
            finalKnockbackDir = Mathf.Sign(knockbackForce.x);
            if (finalKnockbackDir == 0) finalKnockbackDir = Mathf.Sign(transform.position.x - hitboxCenter.Value.x);
            if (finalKnockbackDir == 0) finalKnockbackDir = isFacingRight ? -1 : 1;
        }
        else
        {
            // Directional knockback: use provided direction or calculate from force
            finalKnockbackDir = knockbackDirection ?? Mathf.Sign(knockbackForce.x);
            if (finalKnockbackDir == 0) finalKnockbackDir = isFacingRight ? -1 : 1;
        }

        // Cancel all active states
        if (!hyperArmor)
        {
            isHurt = true;
            CancelAllActions();
            // Start timeout coroutine as safety fallback
            if (hurtTimeoutCoroutine != null)
            {
                StopCoroutine(hurtTimeoutCoroutine);
            }
            hurtTimeoutCoroutine = StartCoroutine(HurtStateTimeout());
        }

        // Damage text
        if (damageText != null)
        {
            Vector2 textVelocity = useRadialKnockback
                ? new Vector2(knockbackForce.x, 5f)
                : new Vector2(knockbackForce.x * finalKnockbackDir, 5f);
            GameObject dmgText = Instantiate(damageText, transform.position, transform.rotation);
            dmgText.GetComponentInChildren<DamageText>().Initialize(textVelocity, damage, Color.red, Color.black);
        }

        StartCoroutine(animatorScript.HurtFlash(0.2f));
        GetComponentInChildren<CinemachineImpulseSource>()?.GenerateImpulse(1.0f);

        // Apply damage - HealthManager will fire OnPlayerDeath event if player dies
        // Die() will be called by the event subscription, so we just check if dead to skip knockback
        HealthManager.instance.TakeDamage(damage);
        bool playerDied = HealthManager.instance.IsDead();

        // Only apply knockback if player didn't die
        if (!playerDied)
        {
            if (useRadialKnockback)
            {
                ApplyKnockbackRadial(finalKnockbackDir, knockbackForce);
            }
            else
            {
                ApplyKnockback(finalKnockbackDir, knockbackForce);
            }
            StartCoroutine(InvincibilityFrames());
        }
        // If player died, Die() will be called by OnPlayerDeath event subscription
    }

    protected virtual void CancelAllActions()
    {
        // Cancel attacks
        if (attackCoroutine != null)
        {
            try { StopCoroutine(attackCoroutine); } catch { }
            attackCoroutine = null;
        }
        if (attackTimeoutCoroutine != null)
        {
            try { StopCoroutine(attackTimeoutCoroutine); } catch { }
            attackTimeoutCoroutine = null;
        }
        isAttacking = false;
        if (attackResetCoroutine != null)
        {
            try { StopCoroutine(attackResetCoroutine); } catch { }
            attackResetCoroutine = null;
        }

        isReloading = false;
        if (reloadCoroutine != null)
        {
            try { StopCoroutine(reloadCoroutine); } catch { }
            reloadCoroutine = null;
        }

        // Cancel dash/slide
        isDashing = false;
        if (slideCoroutine != null)
        {
            try { StopCoroutine(slideCoroutine); } catch { }
            slideCoroutine = null;
        }
        // if (dashCooldownCoroutine != null)
        // {
        //     try { StopCoroutine(dashCooldownCoroutine); } catch { }
        //     dashCooldownCoroutine = null;
        // }
        isInvincible = false;

        // Reset gravity if was dashing
        if (rb != null)
        {
            rb.gravityScale = 3f; //change, hardcoded so far.
        }

        // if (trail != null)
        //     trail.emitting = false;

        if (hitboxManager != null)
            hitboxManager.DisableHitbox();

        // Reset wall slide
        isWallSliding = false;

        // Clear hurt state timeout
        if (hurtTimeoutCoroutine != null)
        {
            try { StopCoroutine(hurtTimeoutCoroutine); } catch { }
            hurtTimeoutCoroutine = null;
        }
    }

    protected virtual void ApplyKnockback(float direction, Vector2 knockbackForce)
    {
        if (direction != (isFacingRight ? -1f : 1f))
        {
            FlipSprite();
        }
        rb.linearVelocity = new(direction * knockbackForce.x, knockbackForce.y);
    }

    protected virtual void ApplyKnockbackRadial(float direction, Vector2 knockbackForce)
    {
        // For radial/explosion knockback, use the force vector directly
        // Direction is only used for sprite flipping
        if (direction != (isFacingRight ? -1f : 1f))
        {
            FlipSprite();
        }
        rb.linearVelocity = knockbackForce;
    }

    public virtual void EndHurt()
    {
        isHurt = false;
        // Stop timeout coroutine if it's still running
        if (hurtTimeoutCoroutine != null)
        {
            try { StopCoroutine(hurtTimeoutCoroutine); } catch { }
            hurtTimeoutCoroutine = null;
        }
    }

    protected virtual IEnumerator HurtStateTimeout()
    {
        yield return new WaitForSeconds(hurtStateTimeout);
        // Safety fallback: automatically clear hurt state if animation event didn't fire
        if (isHurt)
        {
            Debug.LogWarning("Hurt state timeout reached - clearing hurt state automatically");
            EndHurt();
        }
    }

    protected virtual void Die()
    {
        // Idempotent: if already dead, don't process death again
        if (isDead) return;

        // Check if object is still valid before proceeding
        if (this == null || gameObject == null) return;

        isDead = true;

        // Disable hitbox immediately on death
        if (hitboxManager != null)
        {
            hitboxManager.DisableHitbox();
        }

        // Cancel all active actions
        CancelAllActions();

        // Stop all movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Start death sequence - use try-catch to handle destroyed object case
        try
        {
            StartCoroutine(DeathSequence());
        }
        catch (System.Exception)
        {
            // Object was destroyed, can't start coroutine - this is fine, death was already processed
            Debug.LogWarning("Player object destroyed before death sequence could start");
        }
    }

    protected virtual IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        PlayerDied?.Invoke();
        OnDeathAnimationComplete();

        // wait for fade-in
        yield return new WaitForSeconds(1.5f);

        Debug.Log("Attempted to respawn at checkpoint");
        // Respawn at checkpoint
        if (GameRestartManager.Instance != null)
        {
            GameRestartManager.Instance.RespawnCharacter();
        }
        else
        {
            Debug.LogError("GameRestartManager is null! Cannot respawn player.");
        }
    }

    protected virtual void OnDeathAnimationComplete()
    {

    }

    protected virtual void CallInputInvoke(string inputName, PlayerControls pc, KeyCode kc)
    {
        if (inputBroadcaster != null && inputBroadcaster.inputEvent != null)
        {
            inputBroadcaster.RaiseInputEvent(inputName, pc, kc);
        }
        Debug.Log("Input used: " + inputName + " " + pc + " " + kc.ToString());
    }

    // Invincibility frames
    protected virtual IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityTime);

        isInvincible = false;
    }

    public virtual void StartHyperArmor()
    {
        hyperArmor = true;
    }

    public virtual void EndHyperArmor()
    {
        hyperArmor = false;
    }

    protected void StartAttackWatchdog(float duration)
    {
        if (attackTimeoutCoroutine != null)
        {
            try { StopCoroutine(attackTimeoutCoroutine); } catch { }
        }
        attackTimeoutCoroutine = StartCoroutine(AttackTimeout(duration));
    }

    protected IEnumerator AttackTimeout(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (isAttacking)
        {
            // Force end attack if animation event failed
            EndAttack();
        }
        attackTimeoutCoroutine = null;
    }

    protected virtual void SetUpPunchAttack(int attackIndex)
    {
        CallInputInvoke("Punch", PlayerControls.Melee, ControlManager.instance.inputMapping[PlayerControls.Melee]);
        attackIndex = Mathf.Clamp(attackIndex, 0, 2);
        switch (attackIndex)
        {
            case 0:
            case 1:
                hitboxManager.CustomizeHitbox(attackHitboxes[0]);
                animatorScript.ChangeAnimationState(playerStates.Punch1);
                break;
            case 2:
                hitboxManager.CustomizeHitbox(attackHitboxes[1]);
                animatorScript.ChangeAnimationState(playerStates.Punch2);
                // attackTimer = attackCooldown;
                break;
        }
    }

    public PlayerOrientationPosition GetPlayerOrientPosition()
    {
        PlayerOrientationPosition pos;
        pos.position = transform;
        pos.isFacingRight = isFacingRight;
        return pos;
    }

    public void SetWeaponEquipped(bool equipped)
    {
        weaponEquipped = equipped;
    }

}