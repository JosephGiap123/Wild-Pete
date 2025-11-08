using UnityEngine;

// Checkpoint trigger that saves the player's position and game state when touched.
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool saveOnTrigger = true;
    [SerializeField] private bool showVisualFeedback = true;
    [SerializeField] private SpriteRenderer checkpointSprite;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;

    private bool isActive = false;
    private Vector2 checkpointPosition;

    private void Start()
    {
        checkpointPosition = transform.position;
        
        // Set initial visual state
        if (checkpointSprite != null)
        {
            checkpointSprite.color = inactiveColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (saveOnTrigger && collision.CompareTag("Player"))
        {
            ActivateCheckpoint();
        }
    }

    // Activates this checkpoint and saves the game state.
    public void ActivateCheckpoint()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SaveCheckpoint(checkpointPosition);
            isActive = true;

            // Visual feedback
            if (showVisualFeedback && checkpointSprite != null)
            {
                checkpointSprite.color = activeColor;
            }

            Debug.Log($"Checkpoint activated at {checkpointPosition}");
        }
        else
        {
            Debug.LogWarning("CheckpointManager instance not found! Make sure CheckpointManager is in the scene.");
        }
    }

    // Manually sets this checkpoint as active (for visual purposes).
    public void SetActive(bool active)
    {
        isActive = active;
        if (checkpointSprite != null)
        {
            checkpointSprite.color = active ? activeColor : inactiveColor;
        }
    }

    private void OnDrawGizmos()
    {
        // Draw checkpoint position in editor
        Gizmos.color = isActive ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

