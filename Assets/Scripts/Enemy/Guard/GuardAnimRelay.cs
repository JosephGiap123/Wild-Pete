using UnityEngine;

public class GuardAnimRelay : MonoBehaviour
{
    [SerializeField] GuardAI guardAI;
    [SerializeField] AttackHitBoxGuard hitboxScript;

    public void CallEndAttack()
    {
        guardAI.EndAttack();
    }
    public void CallBoxHitbox()
    {
        hitboxScript.ActivateBox();
    }
    public void CallDisableHitbox()
    {
        hitboxScript.DisableHitbox();
    }

    public void CallSpawnBullet()
    {
        guardAI.InstBullet();
    }

    public void CallEndHurt()
    {
        guardAI.EndHurtState();
    }

    public void CallDashVelocityIncrease()
    {
        guardAI.DashVelocityIncrease();
    }

    public void CallEndDash()
    {
        guardAI.StopMoving();
    }
}
