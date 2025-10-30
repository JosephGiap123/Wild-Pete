using System.Collections;
using UnityEngine;

public class AliceMovement2D : BasePlayerMovement2D
{
    protected override float CharWidth => 0.6f;
    protected override Vector2 CrouchOffset => new(0, -0.1238286f);
    protected override Vector2 CrouchSize => new(CharWidth, 0.7002138f);
    protected override Vector2 StandOffset => new(0, 0.1042647f);
    protected override Vector2 StandSize => new(CharWidth, 1.1564f);

    [Header("Alice Specific Combat Settings")]
    [SerializeField] protected int melee1Damage = 4;
    [SerializeField] protected float melee1Size = 0.8f;
    [SerializeField] protected Vector2 melee1Offset = new(0.5f, 0f);
    [SerializeField] protected Vector2 melee1Knockback = new(1f, 5f);
    [SerializeField] protected int melee2Damage = 5;
    [SerializeField] protected float melee2Size = 0.8f;
    [SerializeField] protected Vector2 melee2Offset = new(0.5f, 0f);
    [SerializeField] protected Vector2 melee2Knockback = new(1f, -3f);
    [SerializeField] protected int crouchAttackDamage = 2;
    [SerializeField] protected Vector2 crouchAttackSize = new(1f, 0.55f);
    [SerializeField] protected Vector2 crouchAttackOffset = new(0.3f, -0.25f);
    [SerializeField] protected Vector2 crouchAttackKnockback = new(2f, 1f);
    [SerializeField] protected int aerialAttackDamage = 4;
    [SerializeField] protected float aerialAttackSize = 1f;
    [SerializeField] protected Vector2 aerialAttackOffset = new(0f, 0f);
    [SerializeField] protected Vector2 aerialAttackKnockback = new(3f, 0f);



    protected override void SetupGroundAttack(int attackIndex)
    {
        switch (attackIndex)
        {
            case 0:
                hitboxManager.ChangeHitboxCircle(melee1Offset, melee1Size, melee1Knockback, melee1Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.ChangeHitboxCircle(melee2Offset, melee2Size, melee2Knockback, melee2Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                attackTimer = attackCooldown;
                break;
        }
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.ChangeHitboxBox(crouchAttackOffset, crouchAttackSize, crouchAttackKnockback, crouchAttackDamage);
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
        attackTimer = attackCooldown / 3;
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.ChangeHitboxCircle(aerialAttackOffset, aerialAttackSize, aerialAttackKnockback, aerialAttackDamage);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
        aerialTimer = aerialCooldown;
    }

    protected override void CheckGround()
    {
        bool wasGrounded = isGrounded;
        base.CheckGround();

        if (!wasGrounded && isGrounded && isAttacking && animatorScript.ReturnCurrentState() == playerStates.AerialAttack)
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
        if (isCrouching)
        {
            bulletOrigin.transform.localPosition = new(bulletOrigin.transform.localPosition.x, -0.08f, 0f);
            animatorScript.ChangeAnimationState(playerStates.CrouchRangedAttack);
        }
        else
        {
            bulletOrigin.transform.localPosition = new(bulletOrigin.transform.localPosition.x, 0.2f, 0f);
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