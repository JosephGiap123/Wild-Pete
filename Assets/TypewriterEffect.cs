using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
  public float typingSpeed = 0.05f;
  public TextMeshProUGUI textUI;

  private string fullText;
  private System.Action onComplete;

  public void StartTyping(string text, System.Action completeCallback)
  {
    fullText = text;
    onComplete = completeCallback;

    StopAllCoroutines();
    StartCoroutine(TypeText());
  }

  IEnumerator TypeText()
  {
    textUI.text = "";

    foreach (char c in fullText)
    {
      textUI.text += c;
      yield return new WaitForSeconds(typingSpeed);
    }

    onComplete?.Invoke();
  }
}
