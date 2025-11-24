using UnityEngine;

public class ExplosionCloud : MonoBehaviour
{
    public AttackHitboxInfo explosionHitbox;
    public GenericAttackHitbox attackHitbox;
    public void DeleteParticle()
    {
        Destroy(gameObject);
    }

    public void Initialize(AttackHitboxInfo newExplosionHitbox)
    {
        if (newExplosionHitbox != null)
        {
            explosionHitbox = newExplosionHitbox;
        }
        attackHitbox.CustomizeHitbox(explosionHitbox);
        GetComponent<Animator>().Play("explosion");
    }
}

