using System;
using UnityEngine;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject Pete;
    [SerializeField] private GameObject Alice;
    [SerializeField] private CinemachineCamera cinemachineCam;
    public int selectedCharacter = 1;
    public GameObject player { get; private set; }

    public static event Action<GameObject> OnPlayerSet;

    void Awake()
    {
        // singleton of death and doom
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); //keep across scene loads
    }

    public void SetPlayer()
    {
        selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);
        switch (selectedCharacter)
        {
            case 1:
                player = Instantiate(Pete, transform.position, Quaternion.identity);
                break;
            case 2:
                player = Instantiate(Alice, transform.position, Quaternion.identity);
                break;
            default:
                break;
        }
        GameObject camFollowTarget = new GameObject("CameraFollowTarget");
        camFollowTarget.transform.SetParent(player.transform);
        camFollowTarget.transform.localPosition = new Vector3(0, 2f, 0);
        OnPlayerSet?.Invoke(player); // Notify listeners
        cinemachineCam.Follow = camFollowTarget.transform;
        cinemachineCam.LookAt = camFollowTarget.transform;
    }

    void Start()
    {

        SetPlayer();
    }
}
