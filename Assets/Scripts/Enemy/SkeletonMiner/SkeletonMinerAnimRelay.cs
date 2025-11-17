using UnityEngine;

public class SkeletonMinerAnimRelay : MonoBehaviour
{
    [SerializeField] SkeletonMinerAI skeletonMinerAI;
    [SerializeField] GenericAttackHitbox hitboxScript;

    public void CallEndAttack()
    {
        skeletonMinerAI.EndAttack();
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
        skeletonMinerAI.EndHurtState();
    }
    public void CallZeroVelocity()
    {
        skeletonMinerAI.StopMoving();
    }
}
