using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

/// <summary>
/// Allows the player to look around with the mouse, constrained to a max distance from the player
/// and within camera view bounds.
/// </summary>
public class PlayerLookAround : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] private float maxLookDistance = 3f; // Max distance from player the look position can be
    [SerializeField] private bool useCameraBounds = true; // Whether to constrain to camera view bounds

    [Header("Visual Indicator (Optional)")]
    [SerializeField] private GameObject lookIndicatorPrefab; // Optional visual indicator (cursor, reticle, etc.)
    [SerializeField] private bool showIndicator = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLine = true; // Draw a line from player to look position in Scene view

    [Header("References")]
    [SerializeField] private Transform playerTransform; // Player's transform (auto-assigned if null)
    [SerializeField] private Camera mainCamera; // Main camera (auto-assigned if null)
    [SerializeField] private Transform cameraFollowTarget; // Camera follow target transform (auto-found if null)

    [Header("Camera Follow Settings")]
    [SerializeField] private bool updateCameraFollowTarget = true; // Whether to update camera follow target position
    [SerializeField] private float cameraFollowBlend = 0.5f; // Blend between player position (0) and look position (1). 0.5 = halfway between

    // Current look position in world space
    public Vector2 LookPosition { get; private set; }

    // Direction from player to look position
    public Vector2 LookDirection { get; private set; }

    private GameObject lookIndicatorInstance;
    private BoxCollider2D cameraBounds;
    private CinemachineConfiner2D cameraConfiner;
    private bool isInitialized = false;

    private void Awake()
    {
        // Auto-assign player transform if not set
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        // Create visual indicator if needed (will be positioned once camera is found)
        if (showIndicator && lookIndicatorPrefab != null)
        {
            lookIndicatorInstance = Instantiate(lookIndicatorPrefab);
            lookIndicatorInstance.name = "LookIndicator";
            lookIndicatorInstance.SetActive(false); // Hide until initialized
        }
    }

    private void Start()
    {
        // Wait for camera to be set up (similar to CameraManager)
        StartCoroutine(InitializeAfterCameraSetup());
    }

    private IEnumerator InitializeAfterCameraSetup()
    {
        // Wait a frame to ensure scene objects are initialized
        yield return null;

        // Wait for GameManager to find the camera (it does this in SetPlayer which runs in a coroutine)
        // Give it a couple frames to ensure camera is found
        yield return null;
        yield return null;

        // Try to get camera from GameManager's Cinemachine camera
        if (GameManager.Instance != null && GameManager.Instance.cinemachineCam != null)
        {
            mainCamera = GameManager.Instance.cinemachineCam.GetComponent<Camera>();
            if (mainCamera != null)
            {
                Debug.Log($"[PlayerLookAround] Using camera from Cinemachine camera: {GameManager.Instance.cinemachineCam.name}");
            }
        }

        // Fallback: Try to find Cinemachine camera in scene
        if (mainCamera == null)
        {
            CinemachineCamera[] cinemachineCams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            if (cinemachineCams.Length > 0)
            {
                mainCamera = cinemachineCams[0].GetComponent<Camera>();
                if (mainCamera != null)
                {
                    Debug.Log($"[PlayerLookAround] Using camera from Cinemachine camera found in scene: {cinemachineCams[0].name}");
                }
            }
        }

        // Final fallback: Use Camera.main or find any camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
                Debug.LogWarning("[PlayerLookAround] Could not find Cinemachine camera, using fallback camera");
            }
        }

        // Find camera bounds from CinemachineConfiner2D (same pattern as CameraManager)
        if (useCameraBounds)
        {
            FindCameraBounds();
        }

        // Find camera follow target if not assigned
        if (updateCameraFollowTarget && cameraFollowTarget == null)
        {
            // Look for "CameraFollowTarget" child of player
            Transform followTarget = playerTransform.Find("CameraFollowTarget");
            if (followTarget != null)
            {
                cameraFollowTarget = followTarget;
                Debug.Log($"[PlayerLookAround] Found camera follow target: {cameraFollowTarget.name}");
            }
            else
            {
                Debug.LogWarning("[PlayerLookAround] Camera follow target not found! Camera will not follow look position.");
            }
        }

        // Show indicator now that we're initialized
        if (lookIndicatorInstance != null)
        {
            lookIndicatorInstance.SetActive(true);
        }

        isInitialized = true;

        // Debug log to confirm initialization
        if (mainCamera != null)
        {
            Debug.Log($"[PlayerLookAround] Initialized successfully. Camera: {mainCamera.name}, Max Distance: {maxLookDistance}");
        }
        else
        {
            Debug.LogError("[PlayerLookAround] Failed to initialize - camera is null!");
        }
    }

    private void FindCameraBounds()
    {
        // Try to get camera bounds from GameManager's Cinemachine camera (same pattern as CameraManager)
        if (GameManager.Instance != null && GameManager.Instance.cinemachineCam != null)
        {
            cameraConfiner = GameManager.Instance.cinemachineCam.GetComponent<CinemachineConfiner2D>();
            if (cameraConfiner != null && cameraConfiner.BoundingShape2D != null)
            {
                cameraBounds = cameraConfiner.BoundingShape2D as BoxCollider2D;
                if (cameraBounds != null)
                {
                    Debug.Log($"[PlayerLookAround] Found camera bounds: {cameraBounds.name}");
                }
            }
        }

        // Fallback: Try to find CAMCONFINES object directly
        if (cameraBounds == null)
        {
            GameObject camConfinesObj = GameObject.Find("CAMCONFINES");
            if (camConfinesObj != null)
            {
                cameraBounds = camConfinesObj.GetComponent<BoxCollider2D>();
                if (cameraBounds != null)
                {
                    Debug.Log($"[PlayerLookAround] Found camera bounds via CAMCONFINES: {cameraBounds.name}");
                }
            }
        }

        if (cameraBounds == null && useCameraBounds)
        {
            Debug.LogWarning("[PlayerLookAround] Camera bounds not found! Look position will not be constrained to camera view.");
        }
    }

    private void Update()
    {
        // Don't update until camera is initialized
        if (!isInitialized)
        {
            return;
        }

        if (PauseController.IsGamePaused)
        {
            return;
        }

        // Check if camera is still valid
        if (mainCamera == null)
        {
            Debug.LogWarning("[PlayerLookAround] Camera is null! Cannot update look position.");
            return;
        }

        // Get mouse position in world space
        Vector2 mouseWorldPos = GetMouseWorldPosition();

        // Calculate desired look position
        Vector2 desiredLookPos = mouseWorldPos;

        // Clamp to max distance from player
        Vector2 playerPos = playerTransform.position;
        Vector2 directionToMouse = desiredLookPos - playerPos;
        float distanceToMouse = directionToMouse.magnitude;

        if (distanceToMouse > maxLookDistance)
        {
            desiredLookPos = playerPos + directionToMouse.normalized * maxLookDistance;
        }

        // Clamp to camera view bounds if enabled
        if (useCameraBounds && cameraBounds != null)
        {
            desiredLookPos = ClampToCameraBounds(desiredLookPos, playerPos);
        }

        // Update look position and direction
        LookPosition = desiredLookPos;
        LookDirection = (desiredLookPos - playerPos).normalized;

        // Update visual indicator
        if (lookIndicatorInstance != null)
        {
            lookIndicatorInstance.transform.position = LookPosition;
        }

        // Update camera follow target position
        if (updateCameraFollowTarget && cameraFollowTarget != null)
        {
            // Blend between player position and look position
            // cameraFollowBlend = 0: stays at player position
            // cameraFollowBlend = 1: follows look position exactly
            // cameraFollowBlend = 0.5: halfway between
            Vector2 targetPosition = Vector2.Lerp(playerPos, desiredLookPos, cameraFollowBlend);
            cameraFollowTarget.position = new Vector3(targetPosition.x, targetPosition.y, cameraFollowTarget.position.z);
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            return playerTransform.position;
        }

        Vector3 mouseScreenPos = Input.mousePosition;

        // For orthographic cameras, use the camera's z position
        if (mainCamera.orthographic)
        {
            mouseScreenPos.z = mainCamera.transform.position.z;
        }
        else
        {
            mouseScreenPos.z = mainCamera.nearClipPlane + 1f;
        }

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        return mouseWorldPos;
    }

    private Vector2 ClampToCameraBounds(Vector2 position, Vector2 playerPosition)
    {
        if (cameraBounds == null)
        {
            return position;
        }

        // Get camera bounds
        Bounds bounds = cameraBounds.bounds;

        // Get camera viewport bounds (what the camera can actually see)
        // For orthographic camera, viewport size = orthographicSize * 2
        float orthoSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float viewWidth = orthoSize * 2f * aspect;
        float viewHeight = orthoSize * 2f;

        // Get camera position
        Vector3 cameraPos = mainCamera.transform.position;

        // Calculate viewport bounds in world space
        float minX = cameraPos.x - viewWidth * 0.5f;
        float maxX = cameraPos.x + viewWidth * 0.5f;
        float minY = cameraPos.y - viewHeight * 0.5f;
        float maxY = cameraPos.y + viewHeight * 0.5f;

        // Clamp to camera viewport bounds
        float clampedX = Mathf.Clamp(position.x, minX, maxX);
        float clampedY = Mathf.Clamp(position.y, minY, maxY);

        // Also clamp to overall camera bounds (CAMCONFINES)
        clampedX = Mathf.Clamp(clampedX, bounds.min.x, bounds.max.x);
        clampedY = Mathf.Clamp(clampedY, bounds.min.y, bounds.max.y);

        // Ensure the clamped position is still within max distance from player
        Vector2 clampedPos = new Vector2(clampedX, clampedY);
        Vector2 directionToClamped = clampedPos - playerPosition;
        float distanceToClamped = directionToClamped.magnitude;

        if (distanceToClamped > maxLookDistance)
        {
            clampedPos = playerPosition + directionToClamped.normalized * maxLookDistance;
        }

        return clampedPos;
    }

    /// <summary>
    /// Gets the angle in degrees from the player to the look position (0 = right, 90 = up, -90 = down)
    /// </summary>
    public float GetLookAngle()
    {
        return Mathf.Atan2(LookDirection.y, LookDirection.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Gets the distance from player to look position
    /// </summary>
    public float GetLookDistance()
    {
        return Vector2.Distance(playerTransform.position, LookPosition);
    }

    private void OnDestroy()
    {
        if (lookIndicatorInstance != null)
        {
            Destroy(lookIndicatorInstance);
        }
    }

    // Draw gizmos in editor to visualize look range
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        // Draw max look distance circle
        Gizmos.color = Color.yellow;
        Vector3 center = playerTransform.position;
        Gizmos.DrawWireSphere(center, maxLookDistance);

        // Draw current look position
        if (Application.isPlaying && isInitialized)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(LookPosition, 0.2f);
            if (showDebugLine)
            {
                Gizmos.DrawLine(playerTransform.position, LookPosition);
            }
        }
    }

    // Draw debug line in game view (if enabled)
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && isInitialized && showDebugLine)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
            Gizmos.DrawLine(playerTransform.position, LookPosition);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(LookPosition, 0.15f);
        }
    }
}

