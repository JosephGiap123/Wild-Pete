using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpPower = 6f;
    public float minJumpPower = 3f; // Minimum jump height if button is tapped
    protected float horizontalInput;
    protected bool isFacingRight = true;
    protected bool weaponEquipped = true;
    protected bool isGrounded;
    protected bool isAttacking = false;
    protected bool isCrouching = false;
    protected bool isJumping = false;
    protected float jumpTimeCounter;

    [Header("Combat Settings")]
    public int maxAttackChain = 3;
    protected int attackCount = 0;
    public float comboResetTime = 3f;
    protected Coroutine attackResetCoroutine;

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

    [Header("Aerial Settings")]
    protected float aerialCooldown = 1f;
    protected bool canAerial = true;

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

    protected GameObject bulletInstance;

    // Abstract properties for character-specific values
    protected abstract float CharWidth { get; }
    protected abstract Vector2 CrouchOffset { get; }
    protected abstract Vector2 CrouchSize { get; }
    protected abstract Vector2 StandOffset { get; }
    protected abstract Vector2 StandSize { get; }

    protected virtual void Awake()
    {
        // Make ground check slightly smaller than character width
        if (groundCheck != null)
        {
            groundCheck.size = new Vector2(CharWidth * 0.9f, groundCheck.size.y);
        }
    }

    // Abstract methods for character-specific behavior
    protected abstract void SetupGroundAttack(int attackIndex);
    protected abstract void SetupCrouchAttack();
    protected abstract void SetupAerialAttack();

    protected virtual void Update()
    {
        if(PauseController.IsGamePaused){
            if(Input.GetKeyDown(KeyCode.I)){
                interactor.OnInteract();
            }
            return;
        }
        if (isAttacking)
        {
            return;
        }
        HandleMovement();
        if(!isDashing){
            HandleInput();
            HandleFlip();
        }
        AnimationControl();
    }

    protected virtual void HandleInput()
    {
        if(Input.GetKeyDown(KeyCode.I) && !isDashing && isGrounded){
            interactor.OnInteract();
        }
        if (Input.GetKeyDown(KeyCode.E) && weaponEquipped && !isWallSliding)
        {
            Attack();
        }
        if (Input.GetKeyDown(KeyCode.F) && isGrounded)
        {
            StartCoroutine(ThrowAttack());
        }
        if (Input.GetKeyDown(KeyCode.R) && isGrounded && !isAttacking)
        {
            StartCoroutine(RangedAttack());
        }
        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking && canDash && !isWallSliding)
        {
            isDashing = true;
            if(!isCrouching)
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

    protected virtual void HandleMovement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Jump initiation
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            isJumping = true;
            isGrounded = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            if(isDashing && isCrouching){
                if(slideCoroutine != null)
                    StopCoroutine(slideCoroutine);
                if(dashCooldownCoroutine != null)
                    StopCoroutine(dashCooldownCoroutine);
                isDashing = false;
                StartCoroutine(DashCooldown(slidingCooldown));
            }
        }

        if(isDashing) return;

        // Release jump button early for shorter jump
        if (Input.GetKeyUp(KeyCode.W))
        {
            isJumping = false;
            // Apply minimum jump power if released early
            if (rb.linearVelocity.y > minJumpPower)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, minJumpPower);
            }
        }

        if (Input.GetAxisRaw("Vertical") == -1 && isGrounded)
        {
            isCrouching = true;
            boxCol.offset = CrouchOffset;
            boxCol.size = CrouchSize;
        }
        else if(Input.GetAxisRaw("Vertical") != - 1 && isCrouching)
        {
            //check if there is roof above.
            if((Physics2D.OverlapAreaAll(roofCheck.bounds.min, roofCheck.bounds.max, groundMask).Length + Physics2D.OverlapAreaAll(roofCheck.bounds.min, roofCheck.bounds.max, wallMask).Length) == 0){ //essentially, check if either wall or floor above player.
                isCrouching = false;
                boxCol.offset = StandOffset;
                boxCol.size = StandSize;
            }
        }
    }

    protected virtual void AnimationControl(){
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
                if (rb.linearVelocity.y > 0.1f){
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.RisingWep : playerStates.Rising);
                    return;
                }
                else if (rb.linearVelocity.y < -0.1f){
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.FallingWep : playerStates.Falling);
                    return;
                }
            }
            else
            {
                if (isCrouching){
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.CrouchWep : playerStates.Crouch);
                    return;
                }
                else if (Mathf.Abs(rb.linearVelocity.x) > 0.2f){
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.RunWep : playerStates.Run);
                    return;
                }
                else{
                    animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.IdleWep : playerStates.Idle);
                    return;
                }

            }
        }
    }

    protected virtual void Attack()
    {
        if (isGrounded)
        {
            if(isJumping) return; //frame perfect check.
            if (isCrouching)
            {
                SetupCrouchAttack();
                isAttacking = true;
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
        }
        else
        {
            if (canAerial)
                StartCoroutine(AerialAttack());
        }
    }

    public virtual void EndAttack()
    {
        isAttacking = false;
    }

    protected virtual void FixedUpdate()
    {
        // Dashes override normal movement
        if (isDashing)
        {
            if(isCrouching){
                rb.linearVelocity = new Vector2(
                Mathf.Lerp(rb.linearVelocity.x, 0, 0.05f),
                rb.linearVelocity.y);
            }
            return;
        }

        CheckWall();
        CheckGround();

        // Wall slide behavior
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue)
            );
            return;
        }

        // Grounded attack locks horizontal movement
        if (isAttacking && isGrounded)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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

        if(!isDashing){
            float speedDiff = targetSpeed - rb.linearVelocity.x;
            float accelRate = isGrounded ? 15f : 8f;
            float movementForce = speedDiff * accelRate;

            rb.AddForce(Vector2.right * movementForce);
        }


        // Smooth deceleration when no input
        if (horizontalInput == 0 && isGrounded || isDashing)
        {
            rb.linearVelocity = new Vector2(
                Mathf.Lerp(rb.linearVelocity.x, 0, 0.1f),
                rb.linearVelocity.y
            );
        }
    }


    protected virtual void HandleAerialAttackMovement()
    {
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, 0.1f), rb.linearVelocity.y);
    }

    protected virtual void HandleFlip()
    {
        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
        {
            FlipSprite();
        }
    }

    protected virtual void FlipSprite()
    {
        bulletOrigin.localRotation = Quaternion.Euler(0, 0, isFacingRight ? 180 : 0);
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    protected virtual void CheckGround()
    {
        if (isJumping && rb.linearVelocity.y > 0)
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
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float power = dashingPower;
        rb.linearVelocity = new Vector2(Mathf.Sign(transform.localScale.x) * power, 0f);

        trail.emitting = true;

        yield return new WaitForSeconds(dashingTime);

        trail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    protected virtual IEnumerator DashCooldown(float cooldown){
        yield return new WaitWhile(() => isDashing);
        canDash = false;
        yield return new WaitForSeconds(cooldown);
        canDash = true;
    }

    protected virtual IEnumerator Slide(){
        canDash = false;
        isDashing = true;

        rb.linearVelocity = new Vector2(Mathf.Sign(transform.localScale.x) * slidePower, 0f);

        yield return new WaitForSeconds(slidingCooldown);
        isDashing = false;
    }

    protected virtual IEnumerator AerialAttack()
    {
        canAerial = false;
        SetupAerialAttack();
        isAttacking = true;
        yield return new WaitWhile(() => isAttacking);
        yield return new WaitForSeconds(aerialCooldown);
        canAerial = true;
    }

    protected virtual IEnumerator ThrowAttack()
    {
        isAttacking = true;
        yield return new WaitWhile(() => isAttacking);
    }

    protected virtual IEnumerator RangedAttack()
    {
        isAttacking = true;
        yield return new WaitWhile(() => isAttacking);
    }

    protected virtual IEnumerator ResetAttackCountAfterDelay()
    {
        yield return new WaitForSeconds(comboResetTime);
        attackCount = 0;
    }
}