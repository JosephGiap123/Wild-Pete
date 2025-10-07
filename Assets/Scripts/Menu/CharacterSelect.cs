using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void chooseCharacter(int charNum){
        switch(charNum){
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
        SceneManager.LoadScene(1);
        //then load scene.
    }
}
