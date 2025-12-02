using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutSceneManager : MonoBehaviour
{
  [SerializeField] private SwapSceneEventSO sceneSwapEventSO;
  [System.Serializable]
  public class Slide
  {
    public Sprite image;
    public string text;
  }

  public Image cutsceneImage;
  public TextMeshProUGUI typewriter;
  public GameObject cutsceneCanvas;

  public Slide[] slides;

  private int index = 0;
  private bool isTyping = false;
  private Coroutine playSlideCoroutine;
  private string currentSlideText; // Store current slide text for instant display

  void Update()
  {
    // Handle Space key: skip typing or go to next slide
    if (Input.GetKeyDown(KeyCode.Space))
    {
      if (isTyping)
      {
        // Skip to end of current typing
        isTyping = false;
        if (typewriter != null && !string.IsNullOrEmpty(currentSlideText))
        {
          typewriter.text = currentSlideText; // Show full text immediately
        }
      }
      else
      {
        // Typing is done, go to next slide
        if (playSlideCoroutine != null)
        {
          StopCoroutine(playSlideCoroutine);
        }
        NextSlide();
      }
    }
  }

  public void BeginCutscene()
  {
    index = 0;

    // Show cutscene canvas
    cutsceneCanvas.SetActive(true);

    // Clear text at start
    typewriter.text = "";

    playSlideCoroutine = StartCoroutine(PlaySlide());
  }

  private IEnumerator PlaySlide()
  {
    isTyping = true;

    cutsceneImage.sprite = slides[index].image;
    typewriter.text = "";

    string content = slides[index].text;
    currentSlideText = content; // Store for instant display when skipping

    // Type out each character
    foreach (char c in content)
    {
      typewriter.text += c;
      yield return new WaitForSeconds(0.03f);

      // If player pressed space, isTyping will be false and we break
      if (!isTyping)
        break;
    }

    // Ensure full text is printed (in case typing was skipped)
    typewriter.text = content;
    isTyping = false;

    // Wait a bit before auto-advancing (player can press space to skip this wait)
    yield return new WaitForSeconds(0.5f);

    // Only auto-advance if still not typing (player might have pressed space during wait)
    if (!isTyping)
    {
    NextSlide();
    }
  }

  private void NextSlide()
  {
    // Stop any running coroutine to prevent race conditions
    if (playSlideCoroutine != null)
    {
      StopCoroutine(playSlideCoroutine);
      playSlideCoroutine = null;
    }

    index++;

    if (index >= slides.Length)
    {
      StartCoroutine(EndCutscene());
    }
    else
    {
      playSlideCoroutine = StartCoroutine(PlaySlide());
    }
  }

  protected IEnumerator EndCutscene()
  {
    cutsceneCanvas.SetActive(false);

    sceneSwapEventSO.RaiseEvent("Prison");
    yield return new WaitForSecondsRealtime(1f);
    SceneManager.LoadScene("Prison");
  }

}
