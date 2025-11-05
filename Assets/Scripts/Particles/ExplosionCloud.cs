using UnityEngine;

public class ExplosionCloud : MonoBehaviour
{
    public void Start()
    {
        GetComponent<Animator>().Play("explosion");
    }
    public void DeleteParticle()
    {
        Destroy(gameObject);
    }
}

