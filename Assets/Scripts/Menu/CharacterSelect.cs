using UnityEngine;
using UnityEngine.SceneManagement;

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
    // sceneSwapEventSO.RaiseEvent("Prison");
    SceneManager.LoadScene(2);
    //then load scene.
  }
}
