using System.Collections;
using UnityEngine;

public class PeteMovement2D : BasePlayerMovement2D
{
    protected override float CharWidth => 0.61f;
    protected override Vector2 CrouchOffset => new(0, -0.1238286f);
    protected override Vector2 CrouchSize => new(CharWidth, 0.7002138f);
    protected override Vector2 StandOffset => new(0, 0.1042647f);
    protected override Vector2 StandSize => new(CharWidth, 1.1564f);

    [Header("Pete Specific Combat Settings")]
    [SerializeField] protected int melee1Damage = 2;
    [SerializeField] protected Vector2 melee1Size = new(1.3f, 0.55f);
    [SerializeField] protected Vector2 melee1Offset = new(0.4f, 0f);
    [SerializeField] protected Vector2 melee1Knockback = new(1f, 0f);
    [SerializeField] protected int melee2Damage = 2;
    [SerializeField] protected Vector2 melee2Size = new(1.3f, 0.55f);
    [SerializeField] protected Vector2 melee2Offset = new(0.25f, 0f);
    [SerializeField] protected Vector2 melee2Knockback = new(-2f, 0f);

    [SerializeField] protected int melee3Damage = 3;
    [SerializeField] protected Vector2 melee3Size = new(1.75f, 0.55f);
    [SerializeField] protected Vector2 melee3Offset = new(0.5f, 0f);
    [SerializeField] protected Vector2 melee3Knockback = new(5f, 0f);
    [SerializeField] protected int crouchAttackDamage = 3;
    [SerializeField] protected Vector2 crouchAttackSize = new(1.35f, 0.55f);
    [SerializeField] protected Vector2 crouchAttackOffset = new(0.4f, -0.25f);
    [SerializeField] protected Vector2 crouchAttackKnockback = new(2f, 1f);
    [SerializeField] protected int aerialAttackDamage = 4;
    [SerializeField] protected Vector2 aerialAttackSize = new(1.9f, 1f);
    [SerializeField] protected Vector2 aerialAttackOffset = new(0f, 0f);
    [SerializeField] protected Vector2 aerialAttackKnockback = new(0f, 0f);

    protected override void SetupGroundAttack(int attackIndex)
    {
        switch (attackIndex)
        {
            case 0:
                hitboxManager.ChangeHitboxBox(melee1Offset, melee1Size, melee1Knockback, melee1Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.ChangeHitboxBox(melee2Offset, melee2Size, melee2Knockback, melee2Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                break;
            case 2:
                hitboxManager.ChangeHitboxBox(melee3Offset, melee3Size, melee3Knockback, melee3Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee3);
                attackTimer = attackCooldown;
                break;
        }
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.ChangeHitboxBox(crouchAttackOffset, crouchAttackSize, crouchAttackKnockback, crouchAttackDamage);
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
        attackTimer = attackCooldown / 2;
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.ChangeHitboxBox(aerialAttackOffset, aerialAttackSize, aerialAttackKnockback, aerialAttackDamage);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
        aerialTimer = aerialCooldown;
    }

    protected override void HandleAerialAttackMovement()
    {
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, 0.3f), 0);
    }

    protected override IEnumerator AerialAttack()
    {
        aerialTimer = aerialCooldown;
        SetupAerialAttack();
        isAttacking = true;
        float oldGravity = rb.linearVelocity.y > 0 ? -1f : rb.linearVelocity.y;
        yield return new WaitWhile(() => isAttacking);
        rb.linearVelocity = new(rb.linearVelocity.x, oldGravity);
    }

    protected override IEnumerator ThrowAttack()
    {
        animatorScript.ChangeAnimationState(playerStates.Throw);
        return base.ThrowAttack();
    }

    protected override IEnumerator RangedAttack()
    {
        if (isCrouching)
        {
            bulletOrigin.transform.localPosition = new(bulletOrigin.transform.localPosition.x, -0.12f, 0f);
            animatorScript.ChangeAnimationState(playerStates.CrouchRangedAttack);
        }
        else
        {
            bulletOrigin.transform.localPosition = new(bulletOrigin.transform.localPosition.x, 0.2f, 0f);
            animatorScript.ChangeAnimationState(playerStates.RangedAttack);
        }
        return base.RangedAttack();
    }

    public void InstBullet()
    {
        bulletInstance = Instantiate(bullet, bulletOrigin.position, bulletOrigin.rotation);
    }
}