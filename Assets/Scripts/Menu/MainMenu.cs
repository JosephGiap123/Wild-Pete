using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
  [SerializeField] private Canvas mainMenuCanvas;
  [SerializeField] private Canvas charSelectCanvas;

  [Header("Audio")]
  [SerializeField] private AudioClip buttonClickClip;
  [SerializeField] [Range(0f, 2f)] private float buttonClickVolume = 1f;
  [SerializeField] private AudioSource buttonAudioSource; // Optional: assign to reuse a source

  public void PlayGame()
  {
    PlayButtonClick();
    // Show character select menu and background
    charSelectCanvas.gameObject.SetActive(true);

    mainMenuCanvas.gameObject.SetActive(false);
  }

  public void QuitGame()
  {
    PlayButtonClick();
    Debug.Log("Quit Game"); // Works in editor only
    Application.Quit();     // Works in a built game
  }

  private void PlayButtonClick()
  {
    if (buttonClickClip == null) return;

    float volume = buttonClickVolume;
    if (buttonAudioSource != null)
    {
      buttonAudioSource.PlayOneShot(buttonClickClip, volume);
    }
    else
    {
      // Fallback: spawn a temp AudioSource so the click always plays
      var tempGO = new GameObject("MainMenuButtonClick_Temp");
      var tempSource = tempGO.AddComponent<AudioSource>();
      tempSource.spatialBlend = 0f;
      tempSource.PlayOneShot(buttonClickClip, volume);
      Object.Destroy(tempGO, buttonClickClip.length + 0.05f);
    }
  }
}
