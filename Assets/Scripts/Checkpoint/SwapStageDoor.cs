using UnityEngine;

public class SwapStageDoor : MonoBehaviour, IInteractable
{
    public string interactionName;
    public string InteractMessage()
    {
        return " to enter " + interactionName;
    }
    public bool CanInteract()
    {
        return PlayerInventory.instance.HasItem("Master Key") > 0;
    }
    public void Interact()
    {
        Debug.Log("Interacted with SwapStageDoor");
    }

}
