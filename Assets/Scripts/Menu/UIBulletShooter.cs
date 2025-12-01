using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBulletShooter : MonoBehaviour
{
  public RectTransform bulletImage;
  public RectTransform startButton;
  public Image blackScreen;

  public GameObject mainMenuCanvas;
  public GameObject cutSceneCanvas;
  public GameObject charSelectCanvas;

  public CutSceneManager cutsceneManager;

  public float speed = 6700f;

  public void StartGame()
  {
    if (startButton != null)
      startButton.gameObject.SetActive(false);

    Vector2 startPos = startButton.anchoredPosition;
    startPos.y += -30f;
    bulletImage.anchoredPosition = startPos;

    bulletImage.gameObject.SetActive(true);

    StartCoroutine(PlayStartSequence());
  }

  private IEnumerator PlayStartSequence()
  {
    float canvasWidth = ((RectTransform)bulletImage.parent).rect.width / 2f;

    while (bulletImage.anchoredPosition.x < canvasWidth + 100f)
    {
      bulletImage.anchoredPosition += Vector2.right * speed * Time.deltaTime;
      yield return null;
    }

    bulletImage.gameObject.SetActive(false);

    blackScreen.gameObject.SetActive(true);
    float t = 0f;
    while (t < 0.3f)
    {
      t += Time.deltaTime;
      blackScreen.color = new Color(0, 0, 0, t);
      yield return null;
    }

    mainMenuCanvas.SetActive(false);


    charSelectCanvas.SetActive(true);

    cutSceneCanvas.SetActive(false);


    t = 1f;
    while (t > 0f)
    {
      t -= Time.deltaTime;
      blackScreen.color = new Color(0, 0, 0, t);
      yield return null;
    }

    blackScreen.gameObject.SetActive(false);
  }

}
