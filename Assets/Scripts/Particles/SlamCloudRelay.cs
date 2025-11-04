using UnityEngine;

public class SlamCloudRelay : MonoBehaviour
{
    public void Start()
    {
        GetComponent<Animator>().Play("SlamCloudPlay");
    }
    public void DeleteParticle()
    {
        Destroy(gameObject);
    }
}
