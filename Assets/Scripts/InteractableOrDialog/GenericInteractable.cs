using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericInteractable : MonoBehaviour, IInteractable
{

    public string interactionName { get; private set; }
    public string InteractMessage()
    {
        return interactionName;
    }
    public bool CanInteract()
    {
        //change conditions to interact here.
        return true;
    }
    public void Interact()
    {
        Debug.Log($"Intereacted with {gameObject.name}");
    }
}
