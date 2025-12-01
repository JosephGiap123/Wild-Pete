using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathTipScript : MonoBehaviour
{
    [SerializeField] string[] deathTips;
    [SerializeField] GameObject deathCanvas;
    [SerializeField] TMP_Text deathCountText;
    [SerializeField] TMP_Text deathTipText;
    private Animator anim;
    public void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        GameRestartManager.CharacterRespawned += Respawn;
        GameManager.OnPlayerSet += SetPlayerEvents;
        anim = deathCanvas.GetComponent<Animator>();
        deathCanvas.SetActive(false);
    }

    public void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        GameRestartManager.CharacterRespawned -= Respawn;
        GameManager.OnPlayerSet -= SetPlayerEvents;
    }


    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        deathCanvas.SetActive(false);
    }
    public void SetPlayerEvents(GameObject player)
    {
        player.GetComponent<BasePlayerMovement2D>().PlayerDied += PlayerDeath;
    }

    public void PlayerDeath()
    {
        StartCoroutine(PlayerDeathCoroutine());
    }
    public IEnumerator PlayerDeathCoroutine()
    {
        if (deathTips.Length > 0)
        {
            deathTipText.text = deathTips[Random.Range(0, deathTips.Length)];
        }
        deathCountText.text = "You have died " + HealthManager.instance.numDeaths.ToString() + " times";
        deathCanvas.SetActive(true);
        anim.Play("FadeIn");
        yield return new WaitForSeconds(1.5f);
        anim.Play("FadeOut");
    }

    public void Respawn(Vector2 spawnLoc)
    {
        anim.Play("Fadeout");
    }

}
