using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    void Start()
    {
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
            menuCanvas.SetActive(!menuCanvas.activeSelf);
        }
    }
}
