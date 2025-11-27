using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Parallax effect for individual background layers with automatic tiling for infinite scroll.
/// Attach this to each background sprite/object that should move at a different speed.
/// Automatically creates tiles for seamless infinite scrolling.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("Speed multiplier relative to camera on X-axis. 0 = stationary, 1 = moves with camera, 0.5 = moves at half speed")]
    [SerializeField] private float parallaxSpeedX = 0.5f;
    
    [Tooltip("Speed multiplier relative to camera on Y-axis. 0 = stationary, 1 = moves with camera. Usually set to 0 for backgrounds.")]
    [SerializeField] private float parallaxSpeedY = 0f;
    
    [Tooltip("If true, the layer will tile infinitely. Automatically creates multiple copies of the sprite.")]
    [SerializeField] private bool infiniteScroll = true;
    
    [Header("Camera Reference")]
    [Tooltip("Cinemachine camera to follow. If null, will try to find from GameManager or use Camera.main")]
    public CinemachineCamera cinemachineCamera;
    
    [Tooltip("Regular Unity Camera (fallback if Cinemachine not available). If null, will use Camera.main")]
    public Camera targetCamera;
    
    [Header("Optional Settings")]
    [Tooltip("Offset the layer's starting position")]
    [SerializeField] private Vector2 offset = Vector2.zero;
    
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private Vector3 smoothedCameraPosition; // Smoothed camera position to avoid shake artifacts
    private float spriteWidth;
    private SpriteRenderer spriteRenderer;
    private GameObject[] tileInstances;
    private int tilesNeeded;
    [SerializeField] private float cameraSmoothing = 0.1f; // How much to smooth camera position (lower = more smoothing)
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Try to set up camera, but delay initialization if camera not ready
        SetupCamera();
        
        if (cameraTransform == null)
        {
            Debug.LogWarning($"ParallaxLayer ({gameObject.name}): No camera found in Start(). Will retry in Update().");
            // Don't return - we'll try again in Update
        }
        else
        {
            InitializeParallax();
        }
    }
    
    void InitializeParallax()
    {
        if (cameraTransform == null)
        {
            SetupCamera();
            if (cameraTransform == null)
            {
                return; // Still no camera
            }
        }
        
        // Calculate sprite size
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteWidth = spriteRenderer.sprite.bounds.size.x;
        }
        else
        {
            Debug.LogWarning($"ParallaxLayer ({gameObject.name}): No SpriteRenderer or sprite found! Using default size.");
            spriteWidth = 10f;
        }
        
        // Initialize position anchored to camera
        // Keep the original Z position to ensure proper depth sorting
        float originalZ = transform.position.z;
        transform.position = new Vector3(
            cameraTransform.position.x * parallaxSpeedX + offset.x,
            cameraTransform.position.y * parallaxSpeedY + offset.y,
            originalZ
        );
        
        Debug.Log($"ParallaxLayer ({gameObject.name}): Initialized at position {transform.position} (camera at {cameraTransform.position})");
        
        // Store initial camera position
        lastCameraPosition = cameraTransform.position;
        smoothedCameraPosition = cameraTransform.position;
        
        // Setup infinite scroll tiles
        if (infiniteScroll && spriteWidth > 0)
        {
            SetupInfiniteScroll();
            
            // If tiles weren't created successfully, keep original sprite visible
            if (tileInstances == null || tileInstances.Length == 0)
            {
                Debug.LogWarning($"ParallaxLayer ({gameObject.name}): Infinite scroll setup failed, keeping original sprite visible.");
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
            }
        }
        else if (!infiniteScroll && spriteRenderer != null)
        {
            // If infinite scroll is disabled, make sure sprite is visible
            spriteRenderer.enabled = true;
        }
    }
    
    void SetupCamera()
    {
        // Try Cinemachine camera first (if manually assigned)
        if (cinemachineCamera != null)
        {
            cameraTransform = cinemachineCamera.transform;
            Debug.Log($"ParallaxLayer ({gameObject.name}): Using assigned Cinemachine camera.");
            return;
        }
        
        // Try to get camera from GameManager (if it exists)
        if (GameManager.Instance != null && GameManager.Instance.cinemachineCam != null)
        {
            cinemachineCamera = GameManager.Instance.cinemachineCam;
            cameraTransform = cinemachineCamera.transform;
            Debug.Log($"ParallaxLayer ({gameObject.name}): Using Cinemachine camera from GameManager.");
            return;
        }
        
        // Try to find Cinemachine camera in scene
        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        if (cameras.Length > 0)
        {
            cinemachineCamera = cameras[0];
            cameraTransform = cinemachineCamera.transform;
            Debug.Log($"ParallaxLayer ({gameObject.name}): Found Cinemachine camera in scene: {cameras[0].name}.");
            return;
        }
        
        // Fallback to regular camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        if (targetCamera != null)
        {
            cameraTransform = targetCamera.transform;
            Debug.Log($"ParallaxLayer ({gameObject.name}): Using Camera.main as fallback.");
        }
        else
        {
            Debug.LogError($"ParallaxLayer ({gameObject.name}): No camera found at all!");
        }
    }
    
    void SetupInfiniteScroll()
    {
        if (cameraTransform == null || spriteWidth <= 0 || spriteRenderer == null) 
        {
            Debug.LogWarning($"ParallaxLayer ({gameObject.name}): Cannot setup infinite scroll - camera: {cameraTransform != null}, spriteWidth: {spriteWidth}, spriteRenderer: {spriteRenderer != null}");
            return;
        }
        
        // Get camera view size
        float cameraWidth = GetCameraWidth();
        
        // Calculate how many tiles we need (at least 3: left, center, right, plus buffer)
        tilesNeeded = Mathf.CeilToInt(cameraWidth / spriteWidth) + 4; // Extra tiles for safety
        
        Debug.Log($"ParallaxLayer ({gameObject.name}): Setting up {tilesNeeded} tiles. Camera width: {cameraWidth}, Sprite width: {spriteWidth}");
        
        // Create tile instances
        tileInstances = new GameObject[tilesNeeded];
        
        // Get the current parallax layer position (should be at camera position * parallax speed)
        float baseX = transform.position.x;
        
        for (int i = 0; i < tilesNeeded; i++)
        {
            GameObject tile = new GameObject($"ParallaxTile_{i}");
            tile.transform.SetParent(transform);
            
            SpriteRenderer tileRenderer = tile.AddComponent<SpriteRenderer>();
            tileRenderer.sprite = spriteRenderer.sprite;
            tileRenderer.sortingOrder = spriteRenderer.sortingOrder;
            tileRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            tileRenderer.color = spriteRenderer.color;
            
            // Position tiles side by side, centered around the parallax layer's current position
            // Z should be 0 in local space (relative to parent) - parent's Z will handle depth
            float xPos = (i - (tilesNeeded - 1) / 2f) * spriteWidth;
            tile.transform.localPosition = new Vector3(xPos, 0, 0);
            
            // Ensure tile is enabled and visible
            tile.SetActive(true);
            
            tileInstances[i] = tile;
        }
        
        Debug.Log($"ParallaxLayer ({gameObject.name}): Created {tilesNeeded} tiles. Parallax layer position: {transform.position}, First tile world pos: {tileInstances[0].transform.position}");
        
        // Hide original sprite renderer (we're using tiles now)
        spriteRenderer.enabled = false;
    }
    
    float GetCameraWidth()
    {
        float cameraHeight = 10f;
        float cameraWidth = 10f;
        
        if (targetCamera != null)
        {
            cameraHeight = targetCamera.orthographicSize * 2f;
            cameraWidth = cameraHeight * targetCamera.aspect;
        }
        else if (cinemachineCamera != null)
        {
            cameraHeight = cinemachineCamera.Lens.OrthographicSize * 2f;
            cameraWidth = cameraHeight * (Screen.width / (float)Screen.height);
        }
        
        return cameraWidth;
    }
    
    void LateUpdate()
    {
        // If camera not set up yet, try to initialize
        if (cameraTransform == null)
        {
            SetupCamera();
            if (cameraTransform != null)
            {
                InitializeParallax();
            }
            return;
        }
        
        // Smooth camera position to avoid shake artifacts affecting parallax
        // Use frame-rate independent smoothing
        float smoothFactor = 1f - Mathf.Pow(1f - cameraSmoothing, Time.deltaTime * 60f);
        smoothedCameraPosition = Vector3.Lerp(smoothedCameraPosition, cameraTransform.position, smoothFactor);
        
        // Anchor position to smoothed camera with parallax speed
        // This ensures the parallax layer follows the camera smoothly, ignoring shake
        float targetX = smoothedCameraPosition.x * parallaxSpeedX + offset.x;
        float targetY = smoothedCameraPosition.y * parallaxSpeedY + offset.y;
        float currentZ = transform.position.z; // Preserve Z position for depth sorting
        transform.position = new Vector3(targetX, targetY, currentZ);
        
        // Infinite scroll with tiling
        if (infiniteScroll)
        {
            // If tiles exist, update them
            if (tileInstances != null && tileInstances.Length > 0 && spriteWidth > 0)
            {
                UpdateInfiniteScroll();
            }
            // If tiles don't exist but should, try to create them
            else if (spriteWidth > 0 && spriteRenderer != null)
            {
                Debug.LogWarning($"ParallaxLayer ({gameObject.name}): Tiles missing, attempting to recreate.");
                SetupInfiniteScroll();
            }
        }
        
        // Update last camera position
        lastCameraPosition = cameraTransform.position;
    }
    
    void UpdateInfiniteScroll()
    {
        if (cameraTransform == null || tileInstances == null || tileInstances.Length == 0) return;
        
        float cameraWidth = GetCameraWidth();
        // Use smoothed camera position for tile repositioning to avoid shake artifacts
        float cameraLeft = smoothedCameraPosition.x - cameraWidth * 0.5f;
        float cameraRight = smoothedCameraPosition.x + cameraWidth * 0.5f;
        
        // Check each tile and reposition if needed to ensure continuous coverage
        for (int i = 0; i < tileInstances.Length; i++)
        {
            if (tileInstances[i] == null) continue;
            
            float tileWorldX = transform.position.x + tileInstances[i].transform.localPosition.x;
            float tileLeft = tileWorldX - spriteWidth * 0.5f;
            float tileRight = tileWorldX + spriteWidth * 0.5f;
            
            // If tile is completely to the left of camera view, move it to the right
            if (tileRight < cameraLeft)
            {
                // Find the rightmost tile
                float rightmostX = float.MinValue;
                foreach (GameObject tile in tileInstances)
                {
                    if (tile != null)
                    {
                        float tileX = transform.position.x + tile.transform.localPosition.x;
                        if (tileX > rightmostX) rightmostX = tileX;
                    }
                }
                
                // Position this tile immediately to the right of the rightmost tile
                float newWorldX = rightmostX + spriteWidth;
                float newLocalX = newWorldX - transform.position.x;
                tileInstances[i].transform.localPosition = new Vector3(newLocalX, tileInstances[i].transform.localPosition.y, 0);
            }
            // If tile is completely to the right of camera view, move it to the left
            else if (tileLeft > cameraRight)
            {
                // Find the leftmost tile
                float leftmostX = float.MaxValue;
                foreach (GameObject tile in tileInstances)
                {
                    if (tile != null)
                    {
                        float tileX = transform.position.x + tile.transform.localPosition.x;
                        if (tileX < leftmostX) leftmostX = tileX;
                    }
                }
                
                // Position this tile immediately to the left of the leftmost tile
                float newWorldX = leftmostX - spriteWidth;
                float newLocalX = newWorldX - transform.position.x;
                tileInstances[i].transform.localPosition = new Vector3(newLocalX, tileInstances[i].transform.localPosition.y, 0);
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up tile instances
        if (tileInstances != null)
        {
            foreach (GameObject tile in tileInstances)
            {
                if (tile != null)
                {
                    Destroy(tile);
                }
            }
        }
    }
}

