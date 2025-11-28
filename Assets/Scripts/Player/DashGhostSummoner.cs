using UnityEngine;

public class DashGhostSummoner : MonoBehaviour
{
    [SerializeField] protected BasePlayerMovement2D playerMovement;
    [SerializeField] protected SpriteRenderer playerSpriteRenderer;
    [SerializeField] protected GameObject dashGhostPrefab;
    [SerializeField] protected Transform spawnAt;
    public float dashGhostDelay = 0.1f;
    private float dashGhostTimer = 0.1f;
    void Update()
    {
        if (PauseController.IsGamePaused || playerMovement.IsDead || playerMovement.IsHurt) return;
        if (playerMovement.isDashing && Mathf.Abs(playerMovement.RB.linearVelocity.x) > 0.25f)
        {
            if (dashGhostTimer <= 0)
            {
                SummonDashGhost();
                dashGhostTimer = dashGhostDelay;
            }
            else
            {
                dashGhostTimer -= Time.deltaTime;
            }
        }
    }

    public void SummonDashGhost()
    {
        GameObject dashGhost = Instantiate(dashGhostPrefab, spawnAt.position, Quaternion.identity);
        dashGhost.transform.localScale = playerMovement.transform.localScale;
        dashGhost.GetComponent<SpriteRenderer>().sprite = playerSpriteRenderer.sprite;
    }
}
