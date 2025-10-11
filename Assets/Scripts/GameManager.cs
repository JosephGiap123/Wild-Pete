using UnityEngine;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject Pete;
    [SerializeField] private GameObject Alice;
    [SerializeField] private CinemachineCamera cinemachineCam;
    public int selectedCharacter = 1;
    private GameObject player;
    void Start()
    {

        selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);
        selectedCharacter = 2;
        switch(selectedCharacter){
            case 1:
                player = Instantiate(Pete, transform.position, Quaternion.identity);
                break;
            case 2:
                player = Instantiate(Alice, transform.position, Quaternion.identity);
                break;
            default:
                break;
        }
        cinemachineCam.Follow = player.transform;
        cinemachineCam.LookAt = player.transform; // optional, but often useful
    }
}
