using UnityEngine;

public class SuicideGolemAnimRelay : MonoBehaviour
{
    [SerializeField] SuicideGolemAI suicideGolemAI;
    [SerializeField] GenericAttackHitbox hitboxScript;

    [SerializeField] SGAudioManager audioMgr;

    public void CallEndAttack()
    {
        suicideGolemAI.EndAttack();
    }
    public void CallBoxHitbox()
    {
        hitboxScript.ActivateHitbox();
        // Trigger camera impulse when explosion hitbox activates (when explosion actually happens)
        suicideGolemAI.ImpulsePlayer();
    }
    public void CallDisableHitbox()
    {
        hitboxScript.DisableHitbox();
    }
    public void CallEndHurt()
    {
        suicideGolemAI.EndHurtState();
    }
    public void CallZeroVelocity()
    {
        suicideGolemAI.StopMoving();
    }

    public void CallSetInvincible()
    {
        suicideGolemAI.SetInvincible();
    }

    public void CallEndInvincible()
    {
        suicideGolemAI.EndInvincible();
    }
    public void CallExplosionDeathSequence()
    {
        StartCoroutine(suicideGolemAI.ExplosionDeathSequence());
    }

    public void CallExplosionSound()
    {
        if (audioMgr == null) return;
        audioMgr.StopRunLoop();
        audioMgr.StopBeeping();
        audioMgr.PlayExplode();
    }

    public void CallWalkSound()
    {
        if (audioMgr == null) return;
        audioMgr.StartRunLoop();
    }

    public void CallStopWalkSound()
    {
        if (audioMgr == null) return;
        audioMgr.StopRunLoop();
    }

    public void CallStartBeeping()
    {
        if (audioMgr == null) return;
        audioMgr.StartBeeping();
    }

}
