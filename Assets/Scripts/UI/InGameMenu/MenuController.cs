using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public InputBroadcaster inputBroadcaster;



    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Menu"))
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }
    void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("MenuController: menuCanvas is not assigned in the Inspector!");
        }
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (menuCanvas == null)
            {

                Debug.LogError("MenuController: menuCanvas is null! Cannot toggle menu.");
                return;
            }

            if (!menuCanvas.activeSelf && PauseController.IsGamePaused) return;
            inputBroadcaster.RaiseInputEvent("Inventory", PlayerControls.Inventory, KeyCode.Tab);
            menuCanvas.SetActive(!menuCanvas.activeSelf);
        }
    }
}
