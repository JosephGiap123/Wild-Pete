using UnityEngine;

public class SpiderAnimRelay : MonoBehaviour
{
    [SerializeField] SpiderAI spiderAI;
    [SerializeField] GenericAttackHitbox hitboxScript;

    public void CallEndAttack()
    {
        spiderAI.EndAttack();
    }
    public void CallBoxHitbox()
    {
        hitboxScript.ActivateHitbox();
    }
    public void CallDisableHitbox()
    {
        hitboxScript.DisableHitbox();
    }

    public void CallSpawnBullet()
    {
        spiderAI.InstBullet();
    }

    public void CallEndHurt()
    {
        spiderAI.EndHurtState();
    }

    public void CallDashVelocityIncrease()
    {
        spiderAI.DashVelocityIncrease();
    }

    public void CallEndDash()
    {
        spiderAI.StopMoving();
    }
}
