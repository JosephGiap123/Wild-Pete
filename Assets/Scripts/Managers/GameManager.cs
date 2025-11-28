using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject Pete;
    [SerializeField] private GameObject Alice;
    [SerializeField] public CinemachineCamera cinemachineCam;

    [SerializeField] private string[] sceneNames;
    [SerializeField] private Vector2[] spawnPositions;
    [SerializeField] private IntEventSO selectedCharacterEventSO;

    public enum Characters
    {
        Pete,
        Alice
    }
    public bool UsePlayerPrefs = true;
    public Characters selectedCharacter = Characters.Pete;

    public GameObject player { get; private set; }

    public static event Action<GameObject> OnPlayerSet;

    void Awake()
    {
        selectedCharacterEventSO.onEventRaised.AddListener(OnSelectedCharacterChanged);
        // singleton of death and doom
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); //keep across scene loads
    }

    private void OnSelectedCharacterChanged(int characterIndex)
    {
        selectedCharacter = (characterIndex == 1) ? Characters.Pete : Characters.Alice;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void FindCinemachineCamera()
    {
        // Find the Cinemachine camera in the current scene
        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        if (cameras.Length > 0)
        {
            cinemachineCam = cameras[0];
            Debug.Log($"GameManager: Found Cinemachine camera: {cinemachineCam.name}");
        }
        else
        {
            Debug.LogWarning("GameManager: No Cinemachine camera found in scene!");
        }
    }

    public void SetPlayer(Vector2 spawnPosition)
    {
        // Find the Cinemachine camera in the current scene first
        FindCinemachineCamera();
        switch (selectedCharacter)
        {
            case Characters.Pete:
                player = Instantiate(Pete, spawnPosition, Quaternion.identity);
                break;
            case Characters.Alice:
                player = Instantiate(Alice, spawnPosition, Quaternion.identity);
                break;
            default:
                break;
        }

        if (player == null)
        {
            Debug.LogError("GameManager: Failed to instantiate player!");
            return;
        }

        GameObject camFollowTarget = new GameObject("CameraFollowTarget");
        camFollowTarget.transform.SetParent(player.transform);
        camFollowTarget.transform.localPosition = new Vector3(0, 2f, 0);
        OnPlayerSet?.Invoke(player); // Notify listeners

        // Only set camera targets if we have a valid camera reference
        if (cinemachineCam != null)
        {
            cinemachineCam.Follow = camFollowTarget.transform;
            cinemachineCam.LookAt = camFollowTarget.transform;
            Debug.Log($"GameManager: Set camera Follow and LookAt to player");
        }
        else
        {
            Debug.LogWarning("GameManager: Cannot set camera targets - cinemachineCam is null!");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip gameplay setup for menu-type scenes
        if (scene.name.Contains("Menu"))
        {
            return;
        }

        // Wait a frame to ensure the scene is fully loaded before finding camera and spawning player
        StartCoroutine(SetPlayerAfterSceneLoad(scene));
    }

    private System.Collections.IEnumerator SetPlayerAfterSceneLoad(Scene scene)
    {
        // Wait a frame to ensure all scene objects are initialized
        yield return null;

        Vector2 spawnPosition;
        Debug.Log($"GameManager: Scene name: {scene.name}");
        Debug.Log($"GameManager: Scene index: {Array.IndexOf(sceneNames, scene.name)}");
        Debug.Log($"GameManager: Spawn positions length: {spawnPositions.Length}");
        Debug.Log($"GameManager: Spawn positions: {string.Join(", ", spawnPositions)}");

        // When changing scenes, always use the scene's spawn position
        // Checkpoints are cleared on scene load and are only used for respawning within the same scene
        int sceneIndex = Array.IndexOf(sceneNames, scene.name);
        if (sceneIndex >= 0 && sceneIndex < spawnPositions.Length)
        {
            spawnPosition = spawnPositions[sceneIndex];
            Debug.Log($"GameManager: Using scene spawn position: {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"GameManager: Scene '{scene.name}' not found in sceneNames array, using default spawn position");
            spawnPosition = Vector2.zero;
        }

        SetPlayer(spawnPosition);
    }
}
