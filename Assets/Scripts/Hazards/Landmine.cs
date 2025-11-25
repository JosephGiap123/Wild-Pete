using UnityEngine;

public class Landmine : MonoBehaviour
{
    [SerializeField] private AttackHitboxInfo attackHitboxInfo;

    [SerializeField] private BoxCollider2D boxCol;
    [SerializeField] private GameObject explosionPrefab;

    [SerializeField] private float destroyTimer = 15f;


    public void Awake()
    {
        boxCol.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        LayerMask explodeLayers = attackHitboxInfo.player | attackHitboxInfo.enemy | attackHitboxInfo.statics;
        if (((1 << other.gameObject.layer) & explodeLayers) != 0)
        {
            GameObject newExplosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            newExplosion.GetComponent<ExplosionCloud>().Initialize(attackHitboxInfo);
            Destroy(gameObject);
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        LayerMask explodeLayers = attackHitboxInfo.player | attackHitboxInfo.enemy | attackHitboxInfo.statics;
        if (((1 << other.gameObject.layer) & explodeLayers) != 0)
        {
            GameObject newExplosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            newExplosion.GetComponent<ExplosionCloud>().Initialize(attackHitboxInfo);
            Destroy(gameObject);
        }
    }

    public void SwitchToNotifyAnim()
    {
        GetComponent<Animator>().Play("Notify");
    }

    public void SwitchToLandMineAnim()
    {
        GetComponent<Animator>().Play("LandMine");
        boxCol.enabled = true; 
        Destroy(gameObject, destroyTimer);
    }

    public void InitializeLandMine()
    {
        SwitchToNotifyAnim();
    }
}
