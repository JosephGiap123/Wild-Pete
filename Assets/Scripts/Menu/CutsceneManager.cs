using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutSceneManager : MonoBehaviour
{
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

  void Update()
  {
    // Skip the cutscene with Space
    if (Input.GetKeyDown(KeyCode.Space))
    {
      StopAllCoroutines();
      EndCutscene();
    }
  }

  public void BeginCutscene()
  {
    index = 0;

    // Show cutscene canvas
    cutsceneCanvas.SetActive(true);

    // Clear text at start
    typewriter.text = "";

    StartCoroutine(PlaySlide());
  }

  private IEnumerator PlaySlide()
  {
    isTyping = true;

    cutsceneImage.sprite = slides[index].image;
    typewriter.text = "";

    string content = slides[index].text;

    foreach (char c in content)
    {
      typewriter.text += c;
      yield return new WaitForSeconds(0.03f);

      if (!isTyping)
        break;
    }

    // Ensure full text is printed
    typewriter.text = content;
    isTyping = false;

    yield return new WaitForSeconds(0.5f);

    NextSlide();
  }

  private void NextSlide()
  {
    index++;

    if (index >= slides.Length)
    {
      EndCutscene();
    }
    else
    {
      StartCoroutine(PlaySlide());
    }
  }

  void EndCutscene()
  {
    cutsceneCanvas.SetActive(false);

    SceneManager.LoadScene("Prison");

  }

}
