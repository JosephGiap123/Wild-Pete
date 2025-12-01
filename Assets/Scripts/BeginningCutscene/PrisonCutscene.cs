using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PrisonCutscene : MonoBehaviour
{
    [SerializeField] private Animator eyesAnimator;
    [SerializeField] private VoidEvents onCutsceneEnd;
    [SerializeField] private Dialogue peteDialogue;
    [SerializeField] private Dialogue aliceDialogue;

    void Start()
    {
        StartCoroutine(StartCutscene());
    }

    private IEnumerator StartCutscene()
    {
        yield return new WaitForSeconds(2f);
        eyesAnimator.Play("openeyes");
        StartCoroutine(StartDialogue());
    }

    private IEnumerator StartDialogue()
    {
        yield return new WaitForSeconds(7f);
        if (GameManager.Instance.selectedCharacter == GameManager.Characters.Pete)
        {
            DialogManager.Instance.StartDialogue(peteDialogue);
        }
        else
        {
            DialogManager.Instance.StartDialogue(aliceDialogue);
        }
        yield return new WaitForSeconds(1f); //game pauses, so this will run after the dialogue is finished
        StartCoroutine(EndCutscene());
    }


    private IEnumerator EndCutscene()
    {
        if (onCutsceneEnd != null)
        {
            onCutsceneEnd.RaiseEvent();
        }
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }
}
