using System.Collections;
using UnityEngine;

public class PeteMovement2D : BasePlayerMovement2D
{
    protected override float CharWidth => 0.61f;
    protected override Vector2 CrouchOffset => new Vector2(0, -0.1238286f);
    protected override Vector2 CrouchSize => new Vector2(CharWidth, 0.7002138f);
    protected override Vector2 StandOffset => new Vector2(0, 0.1042647f);
    protected override Vector2 StandSize => new Vector2(CharWidth, 1.1564f);

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