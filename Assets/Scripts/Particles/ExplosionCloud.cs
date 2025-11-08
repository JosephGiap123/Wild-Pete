using UnityEngine;

public class ExplosionCloud : MonoBehaviour
{
    public AttackHitboxInfo explosionHitbox;
    public GenericAttackHitbox attackHitbox;
    public void Start()
    {
        attackHitbox.CustomizeHitbox(explosionHitbox);
        GetComponent<Animator>().Play("explosion");
    }
    public void DeleteParticle()
    {
        Destroy(gameObject);
    }
}

