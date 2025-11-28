using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : GenericAttackHitbox
{
    public override void CustomizeHitbox(AttackHitboxInfo hitboxInfo)
    {
        base.CustomizeHitbox(hitboxInfo);
        if (hitboxInfo.attackType == AttackHitboxInfo.AttackType.WeaponlessMelee)
        {
            currentDamage += StatsManager.instance.weaponlessMeleeAttack;
        }
        else if (hitboxInfo.attackType == AttackHitboxInfo.AttackType.Melee)
        {
            currentDamage += StatsManager.instance.meleeAttack;
        }
    }
}
