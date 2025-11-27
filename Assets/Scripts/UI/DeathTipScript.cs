using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathTipScript : MonoBehaviour
{
    [SerializeField] string[] deathTips;
    [SerializeField] GameObject deathCanvas;
    private Animator anim;
    public void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        GameRestartManager.CharacterRespawned += Respawn;
        GameManager.OnPlayerSet += SetPlayerEvents;
    }

    public void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        GameRestartManager.CharacterRespawned -= Respawn;
        GameManager.OnPlayerSet -= SetPlayerEvents;
    }


    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Menu"))
        {
            deathCanvas.SetActive(false);
            return;
        }
        else
        {
            deathCanvas.SetActive(true);
        }
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
