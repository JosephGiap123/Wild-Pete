using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelect : MonoBehaviour
{

  [SerializeField] private SwapSceneEventSO sceneSwapEventSO;
  [SerializeField] private IntEventSO selectedCharacterEventSO;
  public void chooseCharacter(int charNum)
  {
    charNum = Mathf.Clamp(charNum, 1, 2);
    selectedCharacterEventSO.RaiseEvent(charNum);
    StartCoroutine(LoadSceneCoroutine());
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
