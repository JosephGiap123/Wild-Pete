using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum playerStates { Idle, Run, Slide, Falling, Rising, Hurt, WallSlide, Attack, AerialAttack, AttackRecovery, Throw, Crouch, IdleWep, RisingWep, FallingWep, RunWep, SlideWep, HurtWep, WallSlideWep, CrouchWep, Melee1, Melee2, Dash, DashWep, RangedAttack, CrouchAttack };

public class AliceMovement2D : MonoBehaviour
{
    private const float CHAR_WIDTH = 0.6f;
    public float moveSpeed = 5f; // Adjustable movement speed
    public float jumpPower = 10f; // Adjustable jump height
    float horizontalInput; // x-movement
    bool isFacingRight = true;
    private bool weaponEquipped = true;
    private bool isGrounded;
    private bool isAttacking = false;
    private bool isCrouching = false;
    public int maxAttackChain = 2; //Wild Pete has 3 attacks in his melee frames
    private int attackCount = 0; //current attack frame
    public float comboResetTime = 3f;
    private Coroutine attackResetCoroutine;

    private bool canDash = true;
    private bool isDashing;
    public float dashingPower = 10f;
    public float dashingTime = 0.3f;
    public float dashingCooldown = 3f;

    private float aerialCooldown = 1f;
    private bool canAerial = true;

    [SerializeField] private Transform wallRay;
    [SerializeField] private LayerMask wallMask;  // Layer for walls
    [SerializeField] private float wallSlideSpeed = 1.7f;
    private bool isTouchingWall;
    private bool isWallSliding = false;
    private float castDistance = 0.3f;

    //refs
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D boxCol;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private AliceAnimScript animatorScript;
    [SerializeField] private AliceAttackHitbox hitboxManager;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Transform bulletOrigin;
    [SerializeField] private GameObject bullet;

    private GameObject bulletInstance;


    void AnimationControl()
    {
        //inside func, has attackCount, maxAttackChain, and weaponEquipped.
        if (isWallSliding)
        {
            if (weaponEquipped)
                animatorScript.ChangeAnimationState(playerStates.WallSlideWep);
            else
                animatorScript.ChangeAnimationState(playerStates.WallSlide);
            return;
        }
        if (isDashing)
        {
            if (isCrouching)
            {
                if (weaponEquipped)
                {
                    animatorScript.ChangeAnimationState(playerStates.SlideWep);
                }
                else
                {
                    animatorScript.ChangeAnimationState(playerStates.Slide);
                }
            }
            else
            {
                if (weaponEquipped)
                {
                    animatorScript.ChangeAnimationState(playerStates.DashWep);
                }
                else
                {
                    animatorScript.ChangeAnimationState(playerStates.Dash);
                }
            }
        }
        else if (weaponEquipped)
        { //weapon + melee attacks + gun
            if (!isAttacking)
            {
                if (!isGrounded)
                {
                    if (rb.linearVelocity.y > 0.1f)
                    {
                        animatorScript.ChangeAnimationState(playerStates.RisingWep);
                    }
                    else if (rb.linearVelocity.y < -0.1f)
                    {
                        animatorScript.ChangeAnimationState(playerStates.FallingWep);
                    }
                }
                else
                { //isgrounded
                    if (isCrouching)
                    {
                        animatorScript.ChangeAnimationState(playerStates.CrouchWep);
                    }
                    else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                    {
                        animatorScript.ChangeAnimationState(playerStates.RunWep);
                    }
                    else
                    {
                        animatorScript.ChangeAnimationState(playerStates.IdleWep);
                    }
                }
            }
        }
        else
        { //regular
            if (!isAttacking)
            {
                if (!isGrounded)
                {
                    if (rb.linearVelocity.y > 0.1f)
                    {
                        animatorScript.ChangeAnimationState(playerStates.Rising);
                    }
                    else if (rb.linearVelocity.y < -0.1f)
                    {
                        animatorScript.ChangeAnimationState(playerStates.Falling);
                    }
                }
                else
                { //isgrounded
                    if (isCrouching)
                    {
                        animatorScript.ChangeAnimationState(playerStates.Crouch);
                    }
                    else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                    {
                        animatorScript.ChangeAnimationState(playerStates.Run);
                    }
                    else
                    {
                        animatorScript.ChangeAnimationState(playerStates.Idle);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (isDashing || isAttacking)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.E) && weaponEquipped && !isWallSliding)
        { //melee attack.
            Attack();
        }
        if (Input.GetKeyDown(KeyCode.F) && isGrounded)
        {
            StartCoroutine(ThrowAttack());
        }
        if (Input.GetKeyDown(KeyCode.R) && isGrounded)
        { //shoot gun
            StartCoroutine(RangedAttack());
        }
        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking && canDash && !isWallSliding)
        {
            StartCoroutine(Dash());
        }
        if (!isAttacking && isGrounded)
        { //unsheath/sheath weapon.
            if (Input.GetKeyDown(KeyCode.Z))
            {
                weaponEquipped = !weaponEquipped;
            }
        }
        // Get input for horizontal and vertical movement
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow keys
        if (Input.GetAxisRaw("Vertical") == 1 && isGrounded && !isCrouching)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        }
        if (Input.GetAxisRaw("Vertical") == -1 && isGrounded)
        {
            //crouch logic.
            isCrouching = true;
            boxCol.offset = new Vector2(0, -0.1238286f);
            boxCol.size = new Vector2(CHAR_WIDTH, 0.7002138f);
        }
        else
        {
            isCrouching = false;
            boxCol.offset = new Vector2(0, 0.1042647f);
            boxCol.size = new Vector2(CHAR_WIDTH, 1.1564f);
        }

        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
        {
            FlipSprite();
        }
        AnimationControl();
    }

    void Attack()
    {
        if (isGrounded)
        {
            if (isCrouching)
            {
                hitboxManager.ChangeHitboxBox(new Vector2(0.3f, -0.25f), new Vector2(1f, 0.55f));
                animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
                isAttacking = true;
                return;
            }
            if (attackCount >= maxAttackChain || attackCount < 0) attackCount = 0;
            switch (attackCount)
            {
                case 0:
                    hitboxManager.ChangeHitboxCircle(new Vector2(0.5f, 0f), 0.8f);
                    animatorScript.ChangeAnimationState(playerStates.Melee1);
                    break;
                case 1:
                    hitboxManager.ChangeHitboxCircle(new Vector2(0.5f, 0f), 0.8f);
                    animatorScript.ChangeAnimationState(playerStates.Melee2);
                    break;
                default:
                    return;
            }
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

    public void EndAttack()
    {
        isAttacking = false;
    }

    void FixedUpdate()
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
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, 0.3f), rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(isCrouching ? horizontalInput * moveSpeed * 0.2f : horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        //check if grounded.
        CheckGround();
        CheckWall();
    }

    void FlipSprite()
    {
        bulletOrigin.localRotation = Quaternion.Euler(0, 0, isFacingRight ? 180 : 0);
        isFacingRight = !isFacingRight;
        // Multiply the player's X local scale by -1 to flip
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;

        if (!wasGrounded && isGrounded && isAttacking && animatorScript.returnCurrentState() == playerStates.AerialAttack)
        {
            CancelAerialAttack();
        }
    }

    void CheckWall()
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

    void CancelAerialAttack(){
        StopCoroutine("AerialAttack");
        hitboxManager.DisableHitbox(); 
        isAttacking = false;
        AnimationControl();
    }

    private IEnumerator Dash()
    {
        bool slide = isCrouching ? true : false;
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = slide ? new Vector2(Mathf.Sign(transform.localScale.x) * dashingPower / 1.5f, 0f) : rb.linearVelocity = new Vector2(Mathf.Sign(transform.localScale.x) * dashingPower, 0f); //if sliding, if not, etc.
        if (!slide) trail.emitting = true;
        AnimationControl();
        yield return new WaitForSeconds(slide ? dashingTime * 2 : dashingTime);
        if (!slide) trail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private IEnumerator AerialAttack()
    {
        canAerial = false;
        hitboxManager.ChangeHitboxCircle(new Vector2(0f, 0f), 1f);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
        isAttacking = true;
        yield return new WaitWhile(() => isAttacking);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);
        yield return new WaitForSeconds(aerialCooldown);
        canAerial = true;
    }

    private IEnumerator ThrowAttack()
    {
        animatorScript.ChangeAnimationState(playerStates.Throw);
        isAttacking = true;
        yield return new WaitWhile(() => isAttacking);
    }

    private IEnumerator RangedAttack()
    {
        animatorScript.ChangeAnimationState(playerStates.RangedAttack);
        isAttacking = true;
        yield return new WaitWhile(() => isAttacking);
    }

    public void InstBullet()
    {
        bulletInstance = Instantiate(bullet, bulletOrigin.position, bulletOrigin.rotation);
    }

    private IEnumerator ResetAttackCountAfterDelay()
    {
        yield return new WaitForSeconds(comboResetTime);
        attackCount = 0;
    }

}