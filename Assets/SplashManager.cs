using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SplashSequence : MonoBehaviour
{

  [SerializeField] private GameObject thirdWard;
  [SerializeField] private GameObject names;
  [SerializeField] private GameObject mainMenuCanvas;


  [SerializeField] private Image blackScreen;


  [SerializeField] private float logoDisplayTime = 2f;
  [SerializeField] private float namesDisplayTime = 2f;
  [SerializeField] private float fadeDuration = 0.3f;

  void Awake()
  {
    // Force correct initial state
    if (blackScreen != null)
    {
      // Make sure it's active and fully opaque
      if (!blackScreen.gameObject.activeSelf) blackScreen.gameObject.SetActive(true);
      blackScreen.color = new Color(0, 0, 0, 1f);

      // Ensure it covers the screen and is on top
      var rt = blackScreen.rectTransform;
      rt.anchorMin = Vector2.zero;
      rt.anchorMax = Vector2.one;
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;

      // Put it last so it renders on top (if sharing a Canvas)
      blackScreen.transform.SetAsLastSibling();

      // If on its own Canvas, bump sorting order
      var canvas = blackScreen.GetComponentInParent<Canvas>();
      if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
      {
        canvas.overrideSorting = true;
        canvas.sortingOrder = 9999;
      }
    }

    if (thirdWard) thirdWard.SetActive(false);
    if (names) names.SetActive(false);
    if (mainMenuCanvas) mainMenuCanvas.SetActive(false);
  }

  private IEnumerator Start()
  {
    if (thirdWard) thirdWard.SetActive(true);
    yield return new WaitForSecondsRealtime(logoDisplayTime);

    yield return FadeIn();
    if (thirdWard) thirdWard.SetActive(false);
    if (names) names.SetActive(true);
    yield return FadeOut();

    yield return new WaitForSecondsRealtime(namesDisplayTime);

    yield return FadeIn();
    if (names) names.SetActive(false);
    if (mainMenuCanvas) mainMenuCanvas.SetActive(true);
    yield return FadeOut();

    if (blackScreen) blackScreen.gameObject.SetActive(false);
  }

  private IEnumerator FadeIn()
  {
    float t = 0f;
    if (!blackScreen.gameObject.activeSelf) blackScreen.gameObject.SetActive(true);

    while (t < fadeDuration)
    {
      t += Time.unscaledDeltaTime;
      float a = Mathf.Clamp01(t / fadeDuration);
      blackScreen.color = new Color(0, 0, 0, a);
      yield return null;
    }
    blackScreen.color = new Color(0, 0, 0, 1f);
  }

  private IEnumerator FadeOut()
  {
    float t = fadeDuration;
    if (!blackScreen.gameObject.activeSelf) blackScreen.gameObject.SetActive(true);

    while (t > 0f)
    {
      t -= Time.unscaledDeltaTime;
      float a = Mathf.Clamp01(t / fadeDuration);
      blackScreen.color = new Color(0, 0, 0, a);
      yield return null;
    }
    blackScreen.color = new Color(0, 0, 0, 0f);
  }
}
