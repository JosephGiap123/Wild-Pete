using UnityEngine;

public class RespawnInteractable : MonoBehaviour, IInteractable
{

    public string interactionName { get; private set; }
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip interactClip;
    [Header("Audio Fade")]
    [Tooltip("Enable distance fade of the object's AudioSource based on player distance")]
    [SerializeField] private bool enableDistanceFade = true;
    [Tooltip("Distance within which audio plays at full base volume")]
    [SerializeField] private float fadeFullDistance = 4f;
    [Tooltip("Distance beyond which audio is silent")]
    [SerializeField] private float fadeZeroDistance = 20f;
    [Range(0f,1f)][SerializeField] private float volumeMultiplier = 1f;

    private Transform player;
    private float baseVolume = 1f;
    public string InteractMessage()
    {
        return " to set checkpoint";
    }
    public bool CanInteract()
    {
        return true;
    }
    public void Interact()
    {
        Debug.Log("Attempted to save checkpoint");
        if (sfxSource && interactClip)
        {
            sfxSource.PlayOneShot(interactClip);
        }
        // Save checkpoint at current position
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SaveCheckpoint(transform.position);
            Debug.Log($"Checkpoint saved at {transform.position}");
        }
    }

    private void Awake()
    {
        if (!sfxSource) sfxSource = GetComponent<AudioSource>();
        if (sfxSource) baseVolume = sfxSource.volume;
        // try to find player
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
    }

    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
    }

    private void HandlePlayerSet(GameObject playerObj)
    {
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (!enableDistanceFade || sfxSource == null) return;
        if (player == null) return;

        float dist = Vector2.Distance(player.position, transform.position);
        float fade = ComputeFade(dist, fadeFullDistance, fadeZeroDistance);
        sfxSource.volume = baseVolume * volumeMultiplier * fade;
    }

    private float ComputeFade(float distance, float fullDist, float zeroDist)
    {
        if (distance <= fullDist) return 1f;
        if (distance >= zeroDist) return 0f;
        if (Mathf.Approximately(zeroDist, fullDist)) return 0f;
        return 1f - ((distance - fullDist) / (zeroDist - fullDist));
    }
}
