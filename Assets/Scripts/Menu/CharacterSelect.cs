using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelect : MonoBehaviour
{

  [SerializeField] private SwapSceneEventSO sceneSwapEventSO;
  public void chooseCharacter(int charNum)
  {
    switch (charNum)
    {
      case 1:
        PlayerPrefs.SetInt("SelectedCharacter", 1);
        PlayerPrefs.Save();
        Debug.Log("Pete");
        break;
      case 2: //alice
        PlayerPrefs.SetInt("SelectedCharacter", 2);
        PlayerPrefs.Save();
        Debug.Log("Alice");
        break;
      default:
        break;
    }
    sceneSwapEventSO.RaiseEvent("Prison");
    StartCoroutine(LoadSceneCoroutine());
    //then load scene.
  }

  public IEnumerator LoadSceneCoroutine()
  {
    yield return new WaitForSecondsRealtime(1f);
    SceneManager.LoadScene(2);
  }
}
