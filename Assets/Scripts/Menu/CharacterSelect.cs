using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelect : MonoBehaviour
{
  [SerializeField] private IntEventSO selectedCharacterEventSO;
  public CutSceneManager cutsceneManager;
  // public GameObject mainMenuCanvas;
  public GameObject charSelectCanvas;
  [Header("Audio")]
  [SerializeField] private AudioClip clickClip;
  [SerializeField, Range(0f, 2f)] private float clickVolume = 1f;
  [SerializeField] private AudioSource clickSource; // Optional: assign to reuse a specific source

  public void chooseCharacter(int charNum)
  {
    PlayClickSound();
    charNum = Mathf.Clamp(charNum, 1, 2);
    selectedCharacterEventSO.RaiseEvent(charNum);
    charSelectCanvas.SetActive(false);
    // mainMenuCanvas.SetActive(false);
    // StartCoroutine(LoadSceneCoroutine());
    cutsceneManager.BeginCutscene();
    //then load scene.
  }

  private void PlayClickSound()
  {
    if (clickClip == null) return;

    float volume = clickVolume;
    if (clickSource != null)
    {
      clickSource.PlayOneShot(clickClip, volume);
    }
    else
    {
      // Fallback: spawn a temp AudioSource so the click always plays
      var tempGO = new GameObject("CharacterSelectClick_Temp");
      var tempSource = tempGO.AddComponent<AudioSource>();
      tempSource.spatialBlend = 0f;
      tempSource.PlayOneShot(clickClip, volume);
      Destroy(tempGO, clickClip.length + 0.05f);
    }
  }
}
