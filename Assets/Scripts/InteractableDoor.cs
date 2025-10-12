using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    [Header("Optional Animator")]
    [SerializeField] private Animator animator;   // assign if you have one
    [SerializeField] private string openTrigger = "Open";

    private bool isOpen;

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        // If you have an animation, trigger it
        if (animator != null && !string.IsNullOrEmpty(openTrigger))
            animator.SetTrigger(openTrigger);

        // Or do something simple like disabling a collider / moving object
        // gameObject.SetActive(false);
    }
}
