using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelect : MonoBehaviour
{

  [SerializeField] private SwapSceneEventSO sceneSwapEventSO;
  [SerializeField] private IntEventSO selectedCharacterEventSO;
  public CutSceneManager cutsceneManager;
  public GameObject mainMenuCanvas;
  public GameObject charSelectCanvas;
  public void chooseCharacter(int charNum)
  {
    charNum = Mathf.Clamp(charNum, 1, 2);
    selectedCharacterEventSO.RaiseEvent(charNum);
    charSelectCanvas.SetActive(false);
    mainMenuCanvas.SetActive(false);
    // StartCoroutine(LoadSceneCoroutine());
    cutsceneManager.BeginCutscene();
    //then load scene.
  }

  public IEnumerator LoadSceneCoroutine()
  {
    yield return null;
    sceneSwapEventSO.RaiseEvent("Prison");
    yield return new WaitForSecondsRealtime(1f);
    SceneManager.LoadScene(2);
  }
}
