using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Cinemachine;

public abstract class BasePlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpPower = 6f;
    public float minJumpPower = 3f; // Minimum jump height if button is tapped
    protected float horizontalInput;
    public bool isFacingRight = true;
    protected bool weaponEquipped = true;
    protected bool isGrounded;
    protected bool isAttacking = false;
    protected bool isCrouching = false;
    protected bool isJumping = false;
    protected float jumpTimeCounter;

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
    [SerializeField] protected float attackCooldown = 1.2f;
    protected float attackTimer = 0f;

    [Header("Hurt Settings")] //clauded code here
    [SerializeField] protected float invincibilityTime = 0.1f; // I-frames after hurt
    [SerializeField] protected float hurtStateTimeout = 1.0f; // Max time to stay in hurt state (safety fallback)
    protected bool isHurt = false;
    protected bool isInvincible = false;
    protected bool isDead = false;
    [SerializeField] protected float deathAnimationDuration = 8f; // How long death animation plays

    [Header("Dash Settings")]
    protected bool canDash = true;
    protected bool isDashing;
    public float dashingPower = 12f;
    public float dashingTime = 0.3f;
    public float slidePower = 6f;
    public float dashingCooldown = 3f;
    public float slidingCooldown = 0.75f;
    protected Coroutine slideCoroutine;
    protected Coroutine dashCooldownCoroutine;
    protected Coroutine attackCoroutine;
    protected Coroutine attackTimeoutCoroutine;
    protected Coroutine hurtTimeoutCoroutine;

    [Header("Aerial Settings")]
    [SerializeField] protected float aerialCooldown = 1f;
    protected float aerialTimer = 0f;

    [Header("Attack Safety")]
    [SerializeField] protected float maxAttackDuration = 0.9f; // watchdog timeout for attacks

    [Header("Wall Slide Settings")]
    [SerializeField] protected Transform wallRay;
    [SerializeField] protected LayerMask wallMask;
    [SerializeField] protected float wallSlideSpeed = 1.5f;
    protected bool isTouchingWall;
    protected bool isWallSliding = false;
    protected float castDistance = 0.3f;

    [Header("References")]
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected BoxCollider2D boxCol;
    [SerializeField] protected BoxCollider2D groundCheck;
    [SerializeField] protected BoxCollider2D roofCheck; //check if u can uncrouch
    [SerializeField] protected LayerMask groundMask;
    [SerializeField] protected TrailRenderer trail;
    [SerializeField] protected Transform bulletOrigin;
    [SerializeField] protected GameObject bullet;
    [SerializeField] protected AttackHitbox hitboxManager;
    [SerializeField] protected AnimScript animatorScript;
    [SerializeField] protected InteractionDetection interactor;
    [SerializeField] protected GameObject damageText;
    [SerializeField] protected GameObject dynamitePrefab;

    protected GameObject bulletInstance;

    // Abstract properties for character-specific values
    protected abstract float CharWidth { get; }
    protected abstract Vector2 CrouchOffset { get; }
    protected abstract Vector2 CrouchSize { get; }
    protected abstract Vector2 StandOffset { get; }
    protected abstract Vector2 StandSize { get; }

    //events
    public event Action<int, int> OnAmmoChanged;

    protected virtual void Awake()
    {
        // Make ground check slightly smaller than character width
        if (groundCheck != null)
        {
            groundCheck.size = new(CharWidth * 0.9f, groundCheck.size.y);
        }
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
            if (Input.GetKeyDown(KeyCode.I))
            {
                interactor.OnInteract();
            }
            return;
        }
        attackTimer -= Time.deltaTime;
        aerialTimer -= Time.deltaTime;

        if (isAttacking) return;
        AnimationControl();
        if (isHurt || isReloading) return;
        HandleMovement();
        if (!isDashing)
        {
            HandleInput();
            HandleFlip();
        }
    }

    protected virtual void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isDashing && isGrounded)
        {
            interactor.OnInteract();
        }
        if (Input.GetKeyDown(KeyCode.E) && !isWallSliding)
        {
            Attack();
        }
        if (Input.GetKeyDown(KeyCode.F) && isGrounded && false)
        {
            attackCoroutine = StartCoroutine(ThrowAttack());
        }
        if (Input.GetKeyDown(KeyCode.R) && isGrounded && !isAttacking && ammoCount > 0)
        {
            attackCoroutine = StartCoroutine(RangedAttack());
        }
        if (Input.GetKeyDown(KeyCode.T) && isGrounded && !isCrouching && !isDashing && !isAttacking && !isReloading && ammoCount < maxAmmo && PlayerInventory.instance.HasItem("Ammo") > 0)
        {
            reloadCoroutine = StartCoroutine(Reload());
        }
        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking && canDash && !isWallSliding)
        {
            isDashing = true;
            if (!isCrouching)
            {
                slideCoroutine = StartCoroutine(Dash());
                dashCooldownCoroutine = StartCoroutine(DashCooldown(dashingCooldown));
            }
            else
            {
                StartCoroutine(Slide());
                dashCooldownCoroutine = StartCoroutine(DashCooldown(slidingCooldown));
            }

        }
        if (!isAttacking && isGrounded && Input.GetKeyDown(KeyCode.Z))
        {
            weaponEquipped = !weaponEquipped;
        }
    }

    public PlayerOrientationPosition GetPlayerOrientPosition()
    {
        PlayerOrientationPosition pos;
        pos.position = transform;
        pos.isFacingRight = isFacingRight;
        return pos;
    }

    protected virtual void HandleMovement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Jump initiation
        if (Input.GetKeyDown(KeyCode.W) && isGrounded || Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            isJumping = true;
            isGrounded = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            if (isDashing && isCrouching)
            { //dash cancel
                if (slideCoroutine != null)
                    StopCoroutine(slideCoroutine);
                if (dashCooldownCoroutine != null)
                    StopCoroutine(dashCooldownCoroutine);
                isDashing = false;
                StartCoroutine(DashCooldown(slidingCooldown));
            }
        }

        if (isDashing) return;

        // Release jump button early for shorter jump
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
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
        {
            if (isCrouching || isJumping || !isGrounded || attackTimer > 0f) return;
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
            if (isJumping || attackTimer > 0f) return; //frame perfect check.
            if (isCrouching)
            {
                SetupCrouchAttack();
                isAttacking = true;
                StartAttackWatchdog(maxAttackDuration);
                return;
            }

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
            if (aerialTimer <= 0f)
                attackCoroutine = StartCoroutine(AerialAttack());
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

        // Reset jump state when landing
        if (!wasGrounded && isGrounded)
        {
            isJumping = false;
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
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float power = dashingPower;
        rb.linearVelocity = new(Mathf.Sign(transform.localScale.x) * power, 0f);

        trail.emitting = true;

        yield return new WaitForSeconds(dashingTime);

        isInvincible = false;
        trail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    protected virtual IEnumerator DashCooldown(float cooldown)
    {
        yield return new WaitWhile(() => isDashing);
        canDash = false;
        yield return new WaitForSeconds(cooldown);
        canDash = true;
    }

    protected virtual IEnumerator Slide()
    {
        canDash = false;
        isDashing = true;

        rb.linearVelocity = new(Mathf.Sign(transform.localScale.x) * slidePower, 0f);

        yield return new WaitForSeconds(slidingCooldown);
        isDashing = false;

    }

    protected virtual IEnumerator AerialAttack()
    {
        aerialTimer = aerialCooldown;
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


    public virtual void HurtPlayer(int damage, float knockbackDirection, Vector2 knockbackForce)
    {
        // Don't get hurt if invincible or dead
        if (isInvincible || isDead) return;

        // Cancel all active states
        if (!hyperArmor)
        { //hyper armor can ignore.
            isHurt = true;
            CancelAllActions();
            // Start timeout coroutine as safety fallback
            if (hurtTimeoutCoroutine != null)
            {
                StopCoroutine(hurtTimeoutCoroutine);
            }
            hurtTimeoutCoroutine = StartCoroutine(HurtStateTimeout());
        }
        if (damageText != null)
        {
            GameObject dmgText = Instantiate(damageText, transform.position, transform.rotation);
            dmgText.GetComponentInChildren<DamageText>().Initialize(new(knockbackForce.x * knockbackDirection, 5f), damage, Color.red, Color.black);
        }
        StartCoroutine(animatorScript.HurtFlash(0.2f));
        HealthManager.instance.TakeDamage(damage);
        GetComponentInChildren<CinemachineImpulseSource>()?.GenerateImpulse(1.0f);
        isDead = HealthManager.instance.IsDead();
        // Only apply knockback if player didn't die
        if (!isDead)
        {
            ApplyKnockback(knockbackDirection, knockbackForce);
            StartCoroutine(InvincibilityFrames());
        }
    }

    protected virtual void CancelAllActions()
    {
        // Cancel attacks
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        if (attackTimeoutCoroutine != null)
        {
            StopCoroutine(attackTimeoutCoroutine);
            attackTimeoutCoroutine = null;
        }
        isAttacking = false;
        if (attackResetCoroutine != null)
        {
            StopCoroutine(attackResetCoroutine);
            attackResetCoroutine = null;
        }

        isReloading = false;
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }


        // Cancel dash/slide
        isDashing = false;
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
            slideCoroutine = null;
        }
        if (dashCooldownCoroutine != null)
        {
            StopCoroutine(dashCooldownCoroutine);
            dashCooldownCoroutine = null;
        }
        canDash = true;
        isInvincible = false;

        // Reset gravity if was dashing
        rb.gravityScale = 3f; //change, hardcoded so far.

        if (trail != null)
            trail.emitting = false;

        if (hitboxManager != null)
            hitboxManager.DisableAll();

        // Reset wall slide
        isWallSliding = false;

        // Clear hurt state timeout
        if (hurtTimeoutCoroutine != null)
        {
            StopCoroutine(hurtTimeoutCoroutine);
            hurtTimeoutCoroutine = null;
        }
    }

    // Overload for knockback based on hitbox position
    public virtual void HurtPlayer(int damage, Vector2 hitboxCenter, Vector2 knockbackForce)
    {
        // For explosion/radial knockback, use the Vector2 directly
        // Extract direction sign from the knockback force itself for sprite flipping
        float knockbackDir = Mathf.Sign(knockbackForce.x);
        if (knockbackDir == 0) knockbackDir = Mathf.Sign(transform.position.x - hitboxCenter.x);
        if (knockbackDir == 0) knockbackDir = isFacingRight ? -1 : 1; // Fallback

        // Use the full Vector2 knockback force, preserving both X and Y components
        // Pass a flag to indicate this is radial knockback
        ApplyKnockbackRadial(knockbackDir, knockbackForce);

        // Handle damage and other effects
        if (isInvincible || isDead) return;

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
        if (damageText != null)
        {
            GameObject dmgText = Instantiate(damageText, transform.position, transform.rotation);
            dmgText.GetComponentInChildren<DamageText>().Initialize(new(knockbackForce.x, 5f), damage, Color.red, Color.black);
        }
        StartCoroutine(animatorScript.HurtFlash(0.2f));
        HealthManager.instance.TakeDamage(damage);
        GetComponentInChildren<CinemachineImpulseSource>()?.GenerateImpulse(1.0f);
        isDead = HealthManager.instance.IsDead();
        // Only apply invincibility if player didn't die
        if (!isDead)
        {
            StartCoroutine(InvincibilityFrames());
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
            StopCoroutine(hurtTimeoutCoroutine);
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
        isDead = true;

        // Cancel all active actions
        CancelAllActions();

        // Stop all movement
        rb.linearVelocity = Vector2.zero;

        // Start death sequence
        StartCoroutine(DeathSequence());
    }

    protected virtual IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        // Additional death logic can go here (e.g., respawn, game over screen, etc.)
        // For now, we just disable the player
        OnDeathAnimationComplete();
    }

    protected virtual void OnDeathAnimationComplete()
    {
        // Override this method in child classes to handle what happens after death
        // For example: respawn, show game over, reload scene, etc.
        gameObject.SetActive(false);
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
            StopCoroutine(attackTimeoutCoroutine);
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
        attackIndex = Mathf.Clamp(attackIndex, 0, 2);
        switch (attackIndex)
        {
            case 0:
            case 1:
                hitboxManager.ChangeHitboxBox(new(0.6f, 0f), new(0.6f, 0.4f), new(1f, 0f), 1);
                animatorScript.ChangeAnimationState(playerStates.Punch1);
                break;
            case 2:
                hitboxManager.ChangeHitboxBox(new(0.7f, 0f), new(0.8f, 0.4f), new(3f, 0f), 3);
                animatorScript.ChangeAnimationState(playerStates.Punch2);
                attackTimer = attackCooldown;
                break;
        }
    }

}