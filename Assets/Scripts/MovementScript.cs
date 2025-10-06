using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum playerStates {Idle, Run, Slide, Falling, Rising, Hurt, Gun1, WallSlide, Attack, AerialAttack, AttackRecovery, Throw, Crouch, IdleWep, RisingWep, FallingWep, RunWep, SlideWep, HurtWep, WallSlideWep, CrouchWep, Knife1, Knife2, Knife3, Knife1_Recovery, Knife2_Recovery, Knife3_Recovery, Dash, DashWep};

public class PlayerMovement2D : MonoBehaviour
{
    private const float CHAR_WIDTH = 0.6182312f;
    public float moveSpeed = 5f; // Adjustable movement speed
    public float jumpPower = 6f; // Adjustable jump height
    float horizontalInput; // x-movement
    bool isFacingRight = true;
    private bool weaponEquipped = true;
    private bool isGrounded;
    private bool isAttacking = false;
    private bool isCrouching = false;
    public int maxAttackChain = 3; //Wild Pete has 3 attacks in his melee frames
    private int attackCount = 0; //current attack frame

    private bool canDash = true;
    private bool isDashing;
    public float dashingPower = 12f;
    public float dashingTime = 0.3f;
    public float dashingCooldown = 3f;


    //refs
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D boxCol;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private PlayerAnimScript animatorScript;
    [SerializeField] private TrailRenderer trail;


    void AnimationControl(){
        //inside func, has attackCount, maxAttackChain, and weaponEquipped.
        if(isDashing){ 
            if(isCrouching){
                if(weaponEquipped){
                    animatorScript.ChangeAnimationState(playerStates.SlideWep);
                }
                else{
                    animatorScript.ChangeAnimationState(playerStates.Slide);
                }
            } 
            else{
                if(weaponEquipped){
                    animatorScript.ChangeAnimationState(playerStates.DashWep);
                }
                else{
                    animatorScript.ChangeAnimationState(playerStates.Dash);
                }
            }
        }
        else if(weaponEquipped){ //weapon + melee attacks + gun
            if(!isAttacking){
                if(!isGrounded){    
                    if(rb.linearVelocity.y > 0.1f){
                        animatorScript.ChangeAnimationState(playerStates.RisingWep);
                    }
                    else if(rb.linearVelocity.y < -0.1f){
                        animatorScript.ChangeAnimationState(playerStates.FallingWep);
                    }
                }
                else{ //isgrounded
                    if(isCrouching){
                        animatorScript.ChangeAnimationState(playerStates.CrouchWep);
                    }
                    else if(Mathf.Abs(rb.linearVelocity.x) > 0.1f){
                        animatorScript.ChangeAnimationState(playerStates.RunWep);
                    }
                    else{
                        animatorScript.ChangeAnimationState(playerStates.IdleWep);
                    }
                }
            }
        }
        else{ //regular + gun
            if(!isAttacking){
                if(!isGrounded){    
                    if(rb.linearVelocity.y > 0.1f){
                        animatorScript.ChangeAnimationState(playerStates.Rising);
                    }
                    else if(rb.linearVelocity.y < -0.1f){
                        animatorScript.ChangeAnimationState(playerStates.Falling);
                    }
                }
                else{ //isgrounded
                    if(isCrouching){
                        animatorScript.ChangeAnimationState(playerStates.Crouch);
                    }
                    else if(Mathf.Abs(rb.linearVelocity.x) > 0.1f){
                        animatorScript.ChangeAnimationState(playerStates.Run);
                    }
                    else{
                        animatorScript.ChangeAnimationState(playerStates.Idle);
                    }
                }
            }
        }
    }

    void Update()
    {
        if(isDashing || isAttacking){
            return;
        }
        if(Input.GetKeyDown(KeyCode.E)){
            Attack();
        }
        if(Input.GetKeyDown(KeyCode.Q) && !isAttacking && canDash){
            StartCoroutine(Dash());
        }
        if(!isAttacking && isGrounded){ //unsheath/sheath weapon.
            if(Input.GetKeyDown(KeyCode.Z)){
                weaponEquipped = !weaponEquipped;
            }
        }
        // Get input for horizontal and vertical movement
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow keys
        if (Input.GetAxisRaw("Vertical") == 1  && isGrounded && !isCrouching)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        }
        if(Input.GetAxisRaw("Vertical") == -1 && isGrounded){
            //crouch logic.
            isCrouching = true;
            boxCol.offset = new Vector2(0, -0.1238286f);
            boxCol.size = new Vector2(CHAR_WIDTH, 0.7002138f);
        }
        else{
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

    void Attack(){
        if(isGrounded){
            if(attackCount >= maxAttackChain || attackCount < 0) attackCount = 0;
            switch(attackCount){
                case 0:
                    animatorScript.ChangeAnimationState(playerStates.Knife1);
                    break;
                case 1:
                    animatorScript.ChangeAnimationState(playerStates.Knife2);
                    break;
                case 2:
                    animatorScript.ChangeAnimationState(playerStates.Knife3);
                    break;
                default:
                    return;
            }
            attackCount++;
            isAttacking = true;
        }
        else{
            animatorScript.ChangeAnimationState(playerStates.AerialAttack);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            isAttacking = true;
        }
    }

    public void EndAttack(){
        isAttacking = false;
    }

    void FixedUpdate()
    {
        if(isDashing) return;
        // Apply movement in FixedUpdate for physics-based movement
        if(isAttacking && isGrounded)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else if (isAttacking && isGrounded)
        {
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, 0.3f), rb.linearVelocity.y);
        }
        else{
            rb.linearVelocity = new Vector2(isCrouching ? horizontalInput * moveSpeed *0.2f: horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        //check if grounded.
        CheckGround();
    }

    void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        // Multiply the player's X local scale by -1 to flip
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void CheckGround(){
        isGrounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
    }

    private IEnumerator Dash(){
        bool slide = isCrouching ? true : false;
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = slide ? new Vector2(Mathf.Sign(transform.localScale.x) * dashingPower/1.5f, 0f) : rb.linearVelocity = new Vector2(Mathf.Sign(transform.localScale.x) * dashingPower, 0f); //if sliding, if not, etc.
        if(!slide) trail.emitting = true;
        AnimationControl();
        yield return new WaitForSeconds(slide ? dashingTime * 2 : dashingTime);
        if(!slide) trail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}