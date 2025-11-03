using UnityEngine;

public class WardenAnimRelay : MonoBehaviour
{
    [SerializeField] WardenAI wardenAI;
    public void CallEndAttack()
    {
        wardenAI.EndAttack();
    }
    public void CallActivateHitbox()
    {
        wardenAI.ActivateHitbox();
    }
    public void CallDisableHitbox()
    {
        wardenAI.DisableHitbox();
    }

    // public void CallSpawnProjectile()
    // {
    //     wardenAI.SpawnProjectile();
    // }

    public void CallEndHurt()
    {
        wardenAI.EndHurtState();
    }

    public void CallAddVelocity(float addedVelocity)
    {
        wardenAI.AddVelocity(addedVelocity);
    }

    public void CallChangeAttackNumber(int attackNum)
    {
        wardenAI.ChangeAttackNumber(attackNum);
    }
    public void CallEndAttackState()
    {
        wardenAI.EndAttackState();
    }

    public void CallEndMelee1Chain()
    {
        wardenAI.EndMelee1Chain();
    }
    public void CallEndSlamChain()
    {
        wardenAI.EndUltimate1And2();
    }

    public void CallInstBullet()
    {
        wardenAI.InstBullet();
    }

    public void CallZeroVelocity()
    {
        wardenAI.ZeroVelocity();
    }
}
