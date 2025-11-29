using UnityEngine;

public class NearbyDialogStart : MonoBehaviour
{
    private bool dialogDone = false;
    public Dialogue dialogue;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !dialogDone)
        {
            DialogManager.Instance.StartDialogue(dialogue);
            dialogDone = true;
        }
    }
}
