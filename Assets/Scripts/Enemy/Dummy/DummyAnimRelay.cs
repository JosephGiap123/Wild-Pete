using UnityEngine;

public class DummyAnimRelay : MonoBehaviour
{
    private GenEnemy enemy;   // reference to GenEnemy script

    private void Awake()
    {
        // This will automatically find the GenEnemy on the parent object
        enemy = GetComponentInParent<GenEnemy>();
    }

    // Animation Events (call these from your Animator clips)
    public void EndHurt()
    {
        enemy.EndHurtState();
    }

    public void EndAttack()
    {
        enemy.EndAttackState();
    }

    public void DoAttackDamage()
    {
        enemy.DoAttackDamage();
    }

    public void OnDeathAnimationComplete()
    {
        enemy.OnDeathAnimationComplete();
    }
}
