using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip camera confines setup for menu-type scenes
        if (scene.name.Contains("Menu"))
        {
            return;
        }

        // Wait for scene to fully load and camera to be set up
        StartCoroutine(SetCameraConfinesAfterSceneLoad());
    }

    private IEnumerator SetCameraConfinesAfterSceneLoad()
    {
        // Wait a frame to ensure scene objects are initialized
        yield return null;

        // Wait for GameManager to find the camera (it does this in SetPlayer which runs in a coroutine)
        // Give it a couple frames to ensure camera is found
        yield return null;
        yield return null;

        // Find CAMCONFINES object
        GameObject camConfinesObj = GameObject.Find("CAMCONFINES");
        if (camConfinesObj == null)
        {
            Debug.LogWarning("CameraManager: CAMCONFINES object not found in scene!");
            yield break;
        }

        BoxCollider2D camConfines = camConfinesObj.GetComponent<BoxCollider2D>();
        if (camConfines == null)
        {
            Debug.LogWarning("CameraManager: CAMCONFINES object doesn't have a BoxCollider2D component!");
            yield break;
        }

        // Check if GameManager and camera are available
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("CameraManager: GameManager.Instance is null!");
            yield break;
        }

        if (GameManager.Instance.cinemachineCam == null)
        {
            Debug.LogWarning("CameraManager: cinemachineCam is null! Camera might not be found yet.");
            yield break;
        }

        // Get CinemachineConfiner2D component (2D version, CinemachineConfiner is deprecated)
        CinemachineConfiner2D confiner = GameManager.Instance.cinemachineCam.GetComponent<CinemachineConfiner2D>();
        if (confiner == null)
        {
            Debug.LogWarning("CameraManager: CinemachineConfiner2D component not found on camera!");
            yield break;
        }

        // Set the bounding shape (property name is BoundingShape2D, not m_BoundingShape2D)
        confiner.BoundingShape2D = camConfines;
        Debug.Log($"CameraManager: Set camera confiner bounding shape to CAMCONFINES");
    }
}
