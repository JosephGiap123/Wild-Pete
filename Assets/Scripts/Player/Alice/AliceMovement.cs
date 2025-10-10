using System.Collections;
using UnityEngine;

public class AliceMovement2D : BasePlayerMovement2D
{
    protected override float CharWidth => 0.6f;
    protected override Vector2 CrouchOffset => new Vector2(0, -0.1238286f);
    protected override Vector2 CrouchSize => new Vector2(CharWidth, 0.7002138f);
    protected override Vector2 StandOffset => new Vector2(0, 0.1042647f);
    protected override Vector2 StandSize => new Vector2(CharWidth, 1.1564f);

    protected override void AnimationControl()
    {
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
            }
            else
            {
                animatorScript.ChangeAnimationState(weaponEquipped ? playerStates.DashWep : playerStates.Dash);
            }
        }
        else if (weaponEquipped)
        {
            if (!isAttacking)
            {
                if (!isGrounded)
                {
                    if (rb.linearVelocity.y > 0.1f)
                        animatorScript.ChangeAnimationState(playerStates.RisingWep);
                    else if (rb.linearVelocity.y < -0.1f)
                        animatorScript.ChangeAnimationState(playerStates.FallingWep);
                }
                else
                {
                    if (isCrouching)
                        animatorScript.ChangeAnimationState(playerStates.CrouchWep);
                    else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                        animatorScript.ChangeAnimationState(playerStates.RunWep);
                    else
                        animatorScript.ChangeAnimationState(playerStates.IdleWep);
                }
            }
        }
        else
        {
            if (!isAttacking)
            {
                if (!isGrounded)
                {
                    if (rb.linearVelocity.y > 0.1f)
                        animatorScript.ChangeAnimationState(playerStates.Rising);
                    else if (rb.linearVelocity.y < -0.1f)
                        animatorScript.ChangeAnimationState(playerStates.Falling);
                }
                else
                {
                    if (isCrouching)
                        animatorScript.ChangeAnimationState(playerStates.Crouch);
                    else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                        animatorScript.ChangeAnimationState(playerStates.Run);
                    else
                        animatorScript.ChangeAnimationState(playerStates.Idle);
                }
            }
        }
    }

    protected override void SetupGroundAttack(int attackIndex)
    {
        switch (attackIndex)
        {
            case 0:
                hitboxManager.ChangeHitboxCircle(new Vector2(0.5f, 0f), 0.8f);
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.ChangeHitboxCircle(new Vector2(0.5f, 0f), 0.8f);
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                break;
        }
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.ChangeHitboxBox(new Vector2(0.3f, -0.25f), new Vector2(1f, 0.55f));
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.ChangeHitboxCircle(new Vector2(0f, 0f), 1f);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
    }

    protected override void CheckGround()
    {
        bool wasGrounded = isGrounded;
        base.CheckGround();

        if (!wasGrounded && isGrounded && isAttacking && animatorScript.returnCurrentState() == playerStates.AerialAttack)
        {
            CancelAerialAttack();
        }
    }

    private void CancelAerialAttack()
    {
        StopCoroutine("AerialAttack");
        hitboxManager.DisableHitbox();
        isAttacking = false;
        AnimationControl();
    }

    protected override IEnumerator ThrowAttack()
    {
        animatorScript.ChangeAnimationState(playerStates.Throw);
        return base.ThrowAttack();
    }

    protected override IEnumerator RangedAttack()
    {
        animatorScript.ChangeAnimationState(playerStates.RangedAttack);
        return base.RangedAttack();
    }

    public void InstBullet(int num)
    {
        for (int i = 0; i < num; i++)
            bulletInstance = Instantiate(bullet, bulletOrigin.position, bulletOrigin.rotation);
    }
}