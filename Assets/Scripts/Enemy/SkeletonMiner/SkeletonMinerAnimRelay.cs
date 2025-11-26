using UnityEngine;

public class SkeletonMinerAnimRelay : MonoBehaviour
{
    [SerializeField] SkeletonMinerAI skeletonMinerAI;
    [SerializeField] GenericAttackHitbox hitboxScript;

    [SerializeField] SkeletonAudioManager audioMgr;

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

    public void CallAttackSound()
    {
        audioMgr.PlayAttack();
    }

    public void CallWalkSound()
    {
        audioMgr.StartRunLoop();
    }

    public void CallHurt()
    {
        audioMgr.PlayHurt();
    }
}
