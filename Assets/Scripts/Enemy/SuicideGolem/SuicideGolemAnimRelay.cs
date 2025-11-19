using UnityEngine;

public class SuicideGolemAnimRelay : MonoBehaviour
{
    [SerializeField] SuicideGolemAI suicideGolemAI;
    [SerializeField] GenericAttackHitbox hitboxScript;

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

}
