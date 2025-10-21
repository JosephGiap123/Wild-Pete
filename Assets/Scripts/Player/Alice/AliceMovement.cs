using System.Collections;
using UnityEngine;

public class AliceMovement2D : BasePlayerMovement2D
{
    protected override float CharWidth => 0.6f;
    protected override Vector2 CrouchOffset => new Vector2(0, -0.1238286f);
    protected override Vector2 CrouchSize => new Vector2(CharWidth, 0.7002138f);
    protected override Vector2 StandOffset => new Vector2(0, 0.1042647f);
    protected override Vector2 StandSize => new Vector2(CharWidth, 1.1564f);

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
        if(isCrouching){
            bulletOrigin.transform.localPosition = new Vector3(bulletOrigin.transform.localPosition.x, -0.08f, 0f);
            animatorScript.ChangeAnimationState(playerStates.CrouchRangedAttack);
        }else{
            bulletOrigin.transform.localPosition = new Vector3(bulletOrigin.transform.localPosition.x, 0.2f, 0f);
            animatorScript.ChangeAnimationState(playerStates.RangedAttack);
        }
        return base.RangedAttack();
    }

    public void InstBullet(int num)
    {
        for (int i = 0; i < num; i++)
            bulletInstance = Instantiate(bullet, bulletOrigin.position, bulletOrigin.rotation);
    }
}