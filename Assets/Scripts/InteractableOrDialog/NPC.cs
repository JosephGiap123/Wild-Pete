using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPC : MonoBehaviour, IInteractable
{
    public Dialogue dialogueData;

    public string npcName;
    public string InteractMessage()
    {
        return " to speak with " + npcName;
    }

    public bool CanInteract()
    {
        return !DialogManager.Instance.isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            Debug.LogError("NPC: No dialogue data assigned!");
            return;
        }
        DialogManager.Instance.StartDialogue(dialogueData);
    }

    // void NextLine()
    // {
    //     if (isTyping)
    //     {
    //         StopAllCoroutines();
    //         dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
    //         isTyping = false;
    //     }
    //     else if (++dialogueIndex < dialogueData.dialogueLines.Length)
    //     {
    //         StartCoroutine(TypeLine());
    //     }
    //     else
    //     {
    //         EndDialogue();
    //     }
    // }

    // void StartDialogue()
    // {
    //     isDialogueActive = true;
    //     dialogueIndex = 0;

    //     nameText.SetText(dialogueData.npcName);
    //     portraitImage.sprite = dialogueData.portrait;

    //     dialoguePanel.SetActive(true);
    //     PauseController.SetPause(true);

    //     StartCoroutine(TypeLine());
    // }

    // private IEnumerator TypeLine()
    // {
    //     isTyping = true;
    //     dialogueText.SetText("");

    //     foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
    //     {
    //         dialogueText.text += letter;
    //         yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
    //     }
    //     isTyping = false;
    //     if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
    //     {
    //         yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
    //         NextLine();
    //     }
    // }

    // public void EndDialogue()
    // {
    //     StopAllCoroutines();
    //     isDialogueActive = false;
    //     dialogueText.SetText("");
    //     dialoguePanel.SetActive(false);
    //     PauseController.SetPause(false);
    // }
}
