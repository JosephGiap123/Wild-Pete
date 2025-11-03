using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnHiddenPopup : MonoBehaviour
{
    // Assign this in Inspector (your UI panel or canvas)
    [SerializeField] private GameObject respawnUI;
    public static RespawnHiddenPopup instance;
    void Start()
    {
        respawnUI.SetActive(false); // hidden by default
    }

     public void RestartLevel()
    {
        Debug.Log("✅ RestartLevel() called!");

        // Unpause game before loading
        Hide();
        PauseController.SetPause(false);

        // Register callback BEFORE loading scene
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"✅ Scene Loaded: {scene.name} — now respawning player");

        //RespawnHiddenPopup.instance.Hide();
        // because this object no longer exists in the new scene.

        // ✅ Now safely spawn the player
        GameManager.Instance.SetPlayer();

        // Unsubscribe so we don't duplicate events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        instance = this;
    }
    public void Show()
    {
        respawnUI.SetActive(true);
        PauseController.SetPause(true);
    }
    // Called when respawn button is pressed
    public void Hide()
    {
        respawnUI.SetActive(false);
        PauseController.SetPause(false);
    }

}
