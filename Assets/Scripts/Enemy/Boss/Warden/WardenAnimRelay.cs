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

    public void CallSpawnUlt2Lasers()
    {
        StartCoroutine(wardenAI.Ult2LaserSpawn());
    }

    public void CallTeleportAftermath()
    {
        StartCoroutine(wardenAI.Ult1Teleport());
    }

    public void CallResetUltimateTimer()
    {
        wardenAI.ResetUltimateTimer();
    }
    public void CallResetAttackTimer()
    {
        wardenAI.ResetAttackTimer();
    }
    public void CallResetRangedTimer()
    {
        wardenAI.ResetRangedTimer();
    }
}
