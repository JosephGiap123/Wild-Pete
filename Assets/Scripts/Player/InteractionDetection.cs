using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractionDetection : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public InteractionHintScript interactionHint;
    private Collider2D detectionCollider;
    private bool hasCheckedInitialOverlap = false;

    void Awake()
    {
        detectionCollider = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        hasCheckedInitialOverlap = false;
        // Check for interactibles already in range when enabled
        CheckOverlappingColliders();
        // Also check after a frame delay to catch any physics updates
        StartCoroutine(CheckOverlapAfterFrame());
    }

    private IEnumerator CheckOverlapAfterFrame()
    {
        // Wait for physics to update
        yield return null;
        if (!hasCheckedInitialOverlap && detectionCollider != null && detectionCollider.enabled)
        {
            CheckOverlappingColliders();
            hasCheckedInitialOverlap = true;
        }
    }

    private void CheckOverlappingColliders()
    {
        if (detectionCollider == null) return;
        if (interactableInRange != null) return; // Already have one

        // Use Overlap to find all colliders already inside (same pattern as attack hitbox)
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter(); // Check all layers
        filter.useTriggers = true; // Include trigger colliders

        List<Collider2D> overlappingColliders = new List<Collider2D>();
        detectionCollider.Overlap(filter, overlappingColliders);

        // Process each overlapping collider as if it just entered
        foreach (Collider2D other in overlappingColliders)
        {
            if (other != null && other != detectionCollider)
            {
                // Simulate OnTriggerEnter2D for interactibles already inside
                OnTriggerEnter2D(other);
            }
        }
    }

    public void OnInteract()
    {
        //assume I was pressed if called
        interactableInRange?.Interact();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionHint.ActivateHint(interactable.InteractMessage());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionHint.DeactivateHint();
        }
    }
}
