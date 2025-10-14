using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockPick : MonoBehaviour, IInteractable
{
    [Header("UI / Prefab")]
    [SerializeField] private Canvas uiCanvas;                    // Drag your UI Canvas here
    [SerializeField] private LockpickFiveInARow minigamePrefab;  // Drag your mini-game prefab here

    [Header("Door to unlock (optional)")]
    [SerializeField] private InteractableDoor door;              // Assign if you want the door to open on success
    bool active = true;

    private LockpickFiveInARow activeGame;

    public bool CanInteract()
    {
        // Keep your existing logic; block re-opening while active
        if(!active) return false;
        return activeGame == null;
    }

    public void Interact()
    {
        Debug.Log($"Interacted with {gameObject.name}");
        if (activeGame != null) return;

        // Spawn popup under the UI Canvas
        activeGame = Instantiate(minigamePrefab, uiCanvas.transform);

        // Optional: disable player movement here
        // PlayerController.Instance.enabled = false;

        activeGame.OnComplete = success =>
        {
            // Optional: re-enable movement here
            // PlayerController.Instance.enabled = true;

            if (success)
            {
                Debug.Log("Lock successfully picked!");
                if (door != null) door.Open();
                active = false;
            }
            else
            {
                Debug.Log("Lockpick cancelled or failed.");
            }

            Destroy(activeGame.gameObject);
            activeGame = null;
        };
    }
}
