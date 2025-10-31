using UnityEngine;

public class DummyAnimRelay : MonoBehaviour
{
    [SerializeField] private EnemyBase enemy;

    // Animation Events (call these from your Animator clips)
    public void CallEndHurt()
    {
        enemy.EndHurtState();
    }
}
