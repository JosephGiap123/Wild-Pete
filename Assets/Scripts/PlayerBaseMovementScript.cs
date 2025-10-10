using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpPower = 6f;
    protected float horizontalInput;
    protected bool isFacingRight = true;
    protected bool weaponEquipped = true;
    protected bool isGrounded;
    protected bool isAttacking = false;
    protected bool isCrouching = false;

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
    public float dashingCooldown = 3f;

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
    [SerializeField] protected LayerMask groundMask;
    [SerializeField] protected TrailRenderer trail;
    [SerializeField] protected Transform bulletOrigin;
    [SerializeField] protected GameObject bullet;
    [SerializeField] protected AttackHitbox hitboxManager;

    protected GameObject bulletInstance;

    // Abstract properties for character-specific values
    protected abstract float CharWidth { get; }
    protected abstract Vector2 CrouchOffset { get; }
    protected abstract Vector2 CrouchSize { get; }
    protected abstract Vector2 StandOffset { get; }
    protected abstract Vector2 StandSize { get; }

    // Abstract methods for character-specific behavior
    protected abstract void AnimationControl();
    protected abstract void SetupGroundAttack(int attackIndex);
    protected abstract void SetupCrouchAttack();
    protected abstract void SetupAerialAttack();

    protected virtual void Update()
    {
        if (isDashing || isAttacking)
        {
            return;
        }

        HandleInput();
        HandleMovement();
        HandleFlip();
        AnimationControl();
    }

    protected virtual void HandleInput()
    {
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
            StartCoroutine(Dash());
        }
        if (!isAttacking && isGrounded && Input.GetKeyDown(KeyCode.Z))
        {
            weaponEquipped = !weaponEquipped;
        }
    }

    protected virtual void HandleMovement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetAxisRaw("Vertical") == 1 && isGrounded && !isCrouching)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        }

        if (Input.GetAxisRaw("Vertical") == -1 && isGrounded)
        {
            isCrouching = true;
            boxCol.offset = CrouchOffset;
            boxCol.size = CrouchSize;
        }
        else
        {
            isCrouching = false;
            boxCol.offset = StandOffset;
            boxCol.size = StandSize;
        }
    }

    protected virtual void Attack()
    {
        if (isGrounded)
        {
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
        if (isDashing) return;

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(-0.5f, rb.linearVelocity.y, -wallSlideSpeed));
        }
        else if (isAttacking && isGrounded)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else if (isAttacking && !isGrounded)
        {
            HandleAerialAttackMovement();
        }
        else
        {
            float speed = isCrouching ? horizontalInput * moveSpeed * 0.2f : horizontalInput * moveSpeed;
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }

        CheckGround();
        CheckWall();
    }

    protected virtual void HandleAerialAttackMovement()
    {
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, 0.3f), rb.linearVelocity.y);
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
        isGrounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
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
        bool slide = isCrouching;
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;

        if (!slide)
            rb.gravityScale = 0f;

        float power = slide ? dashingPower / 1.5f : dashingPower;
        rb.linearVelocity = new Vector2(Mathf.Sign(transform.localScale.x) * power, 0f);

        if (!slide)
            trail.emitting = true;

        AnimationControl();
        yield return new WaitForSeconds(slide ? dashingTime * 2 : dashingTime);

        if (!slide)
            trail.emitting = false;

        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
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