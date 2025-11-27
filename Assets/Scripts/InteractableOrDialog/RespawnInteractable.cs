using UnityEngine;

public class RespawnInteractable : MonoBehaviour, IInteractable
{

    public string interactionName { get; private set; }
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
        // Save checkpoint at current position
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SaveCheckpoint(transform.position);
            Debug.Log($"Checkpoint saved at {transform.position}");
        }
    }
}
