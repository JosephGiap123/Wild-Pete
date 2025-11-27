using UnityEngine;

public class InteractionDetection : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public InteractionHintScript interactionHint;

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
