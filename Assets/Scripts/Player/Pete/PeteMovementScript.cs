using System.Collections;
using UnityEngine;

public class PeteMovement2D : BasePlayerMovement2D
{
    protected override float CharWidth => 0.61f;
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
                hitboxManager.ChangeHitboxBox(new Vector2(0.4f, 0f), new Vector2(1.3f, 0.55f));
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.ChangeHitboxBox(new Vector2(0.25f, 0f), new Vector2(1.3f, 0.55f));
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                break;
            case 2:
                hitboxManager.ChangeHitboxBox(new Vector2(0.5f, 0f), new Vector2(1.75f, 0.55f));
                animatorScript.ChangeAnimationState(playerStates.Melee3);
                break;
        }
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.ChangeHitboxBox(new Vector2(0.4f, -0.25f), new Vector2(1.35f, 0.55f));
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.ChangeHitboxBox(new Vector2(0f, 0f), new Vector2(1.9f, 1f));
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
    }

    protected override void HandleAerialAttackMovement()
    {
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, 0.3f), 0);
    }

    protected override IEnumerator AerialAttack()
    {
        canAerial = false;
        SetupAerialAttack();
        isAttacking = true;
        float oldGravity = rb.linearVelocity.y > 0 ? -1f : rb.linearVelocity.y;
        yield return new WaitWhile(() => isAttacking);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, oldGravity);
        yield return new WaitForSeconds(aerialCooldown);
        canAerial = true;
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

    public void InstBullet()
    {
        bulletInstance = Instantiate(bullet, bulletOrigin.position, bulletOrigin.rotation);
    }
}