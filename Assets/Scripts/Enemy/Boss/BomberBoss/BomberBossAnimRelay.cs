using UnityEngine;

public class BomberBossAnimRelay : MonoBehaviour
{
    [SerializeField] BomberBoss bomberBoss;
    [SerializeField] GenericAttackHitbox hitboxScript;

    public void CallEndAttack()
    {
        bomberBoss.EndAttack();
    }
    public void CallBoxHitbox()
    {
        hitboxScript.ActivateHitbox();
    }
    public void CallDisableHitbox()
    {
        hitboxScript.DisableHitbox();
    }
    public void CallEndHurt()
    {
        bomberBoss.EndHurtState();
    }
    public void CallZeroVelocity()
    {
        bomberBoss.ZeroVelocity();
    }
    public void CallAddVelocity(float addedVelocity)
    {
        bomberBoss.AddVelocity(addedVelocity);
    }

    public void CallAddYVelocity(float addedVelocity)
    {
        bomberBoss.AddYVelocity(addedVelocity);
    }
    public void CallChangeAttackNumber(int attackNum)
    {
        bomberBoss.ChangeAttackNumber(attackNum);
    }
    public void CallEndAttackState()
    {
        bomberBoss.EndAttackState();
    }

    public void CallEndMelee1Attack()
    {
        bomberBoss.EndMelee1Chain();
    }

    public void CallSetUpAndSpawnParticle(int particleNum)
    {
        bomberBoss.SetUpAndSpawnParticle(particleNum);
    }

    public void CallSpawnRocket(float angle)
    {
        bomberBoss.SpawnRocket(angle);
    }

    public void CallSpawnRocketJumpExplosion()
    {
        bomberBoss.SpawnRocketJumpExplosion();
    }

    public void CallEndUlt()
    {
        bomberBoss.EndUlt();
    }

    public void CallDoUlt()
    {
        bomberBoss.DoUlt();
    }

    public void CallReload()
    {
        bomberBoss.Reload();
    }

    public void CallEndStagger()
    {
        bomberBoss.EndStagger();
    }

    public void CallUseAmmo()
    {
        bomberBoss.UseAmmo();
    }
}
