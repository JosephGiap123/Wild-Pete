using TMPro;
using UnityEngine;

public class DeathTipScript : MonoBehaviour
{
    [SerializeField] string[] deathTips;
    [SerializeField] GameObject deathCanvas;
    private Animator anim;
    public void OnEnable()
    {
        GameRestartManager.CharacterRespawned += Respawn;
        GameManager.OnPlayerSet += SetPlayerEvents;
    }

    public void OnDisable()
    {
        GameRestartManager.CharacterRespawned -= Respawn;
        GameManager.OnPlayerSet -= SetPlayerEvents;
    }

    public void SetPlayerEvents(GameObject player)
    {
        player.GetComponent<BasePlayerMovement2D>().PlayerDied += PlayerDeath;
    }

    public void PlayerDeath()
    {
        if (deathTips.Length > 0)
        {
            deathCanvas.GetComponentInChildren<TMP_Text>().text = deathTips[Random.Range(0, deathTips.Length)];

        }
        anim.Play("FadeIn");
    }

    public void Respawn(Vector2 spawnLoc)
    {
        anim.Play("Fadeout");
    }

    public void Awake()
    {
        anim = deathCanvas.GetComponent<Animator>();
    }
}
