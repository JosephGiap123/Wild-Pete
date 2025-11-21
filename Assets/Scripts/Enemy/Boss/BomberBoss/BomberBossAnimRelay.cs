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
    public void CallChangeAttackNumber(int attackNum)
    {
        bomberBoss.ChangeAttackNumber(attackNum);
    }
    public void CallEndAttackState()
    {
        bomberBoss.EndAttackState();
    }
}
