using UnityEngine;
using UnityEngine.SceneManagement;   // for SceneManager & Scene

public class MenuController : MonoBehaviour
{
    [Header("Inventory / Tab menu panel (NOT the whole UI root)")]
    public GameObject menuCanvas;                 // e.g. your Inventory / In-game menu panel
    public InputBroadcaster inputBroadcaster;     // already used in your project

    void Awake()
    {
        // Keep this object between scenes
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);

        if (menuCanvas != null)
        {
            // Start with the inventory/menu closed
            menuCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("MenuController: menuCanvas is not assigned in the Inspector!");
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called every time a new scene is loaded
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // IMPORTANT: we ONLY touch the menu panel, never this.gameObject
        if (menuCanvas != null)
        {
            // Whether it's a Menu scene or Gameplay scene, start with Tab menu closed
            menuCanvas.SetActive(false);
        }
    }

    void Update()
    {
        // Don't allow opening inventory in menu scenes
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName.Contains("Menu") || currentSceneName.Contains("menu"))
        {
            return;
        }

        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Inventory]))
        {
            if (menuCanvas == null)
            {
                Debug.LogError("MenuController: menuCanvas is null! Cannot toggle menu.");
                return;
            }

            // Don't allow opening a second menu if some other system has already paused the game
            if (!menuCanvas.activeSelf && PauseController.IsGamePaused)
                return;

            if (inputBroadcaster != null)
            {
                inputBroadcaster.RaiseInputEvent(
                    "Inventory",
                    PlayerControls.Inventory,
                    KeyCode.Tab
                );
            }

            menuCanvas.SetActive(!menuCanvas.activeSelf);
        }
    }
}
