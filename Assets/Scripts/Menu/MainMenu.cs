using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
  [SerializeField] private Canvas mainMenuCanvas;
  [SerializeField] private Canvas charSelectCanvas;

  public void PlayGame()
  {
    // Show character select menu and background
    charSelectCanvas.gameObject.SetActive(true);

    mainMenuCanvas.gameObject.SetActive(false);
  }

  public void QuitGame()
  {
    Debug.Log("Quit Game"); // Works in editor only
    Application.Quit();     // Works in a built game
  }
}
