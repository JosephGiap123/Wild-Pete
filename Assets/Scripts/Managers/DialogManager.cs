using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [SerializeField] public GameObject dialoguePanel;
    [SerializeField] public TMP_Text dialogueText;
    [SerializeField] public TMP_Text nameText;
    [SerializeField] public Image portraitImage;
    [SerializeField] public GameObject choicesPanel;
    private Button[] choiceButtons;
    [SerializeField] public float typingSpeed = 0.05f;

    private bool choseChoice = false;
    private Dialogue dialogue;
    private Dialogue.DialogueNode currentNode;
    public bool isTyping;
    public bool isDialogueActive = false;
    private bool hasChoices = false;


    //holds refs to the dialogue panel, text, name, and portrait for dialog npcs to use

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        choseChoice = false;
        // Cache choice buttons and wire listeners once
        if (choicesPanel != null)
        {
            choiceButtons = choicesPanel.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i; // capture local copy
                choiceButtons[i].onClick.AddListener(() => OnChoiceButton(index));
            }
        }
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        { //if you press space while dialogue is active and there are no choices, go to the next line.
            if (!hasChoices && !isTyping)
            {
                ProgressDialogue(); //go to the next line.
            }
            else if (isTyping)
            { //if you press space while it is still typing, fast forward the typing.
                isTyping = false;
            }
        }
    }

    public void ShowDialoguePanel()
    {
        Debug.Log("Showing dialogue panel");
        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);
    }

    public void HideDialoguePanel()
    {
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
        isDialogueActive = false;
        currentNode = null;
        hasChoices = false;
        isTyping = false;
        dialogueText.text = "";
        nameText.text = "";
        portraitImage.sprite = null;
        HideChoices();
    }

    public void ShowChoices(Dialogue.Choice[] choices)
    {
        if (choicesPanel == null || choiceButtons == null) return;

        // Update up to max buttons
        int max = Mathf.Min(choices.Length, choiceButtons.Length);
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < max && choices[i] != null && !string.IsNullOrWhiteSpace(choices[i].choiceText))
            {
                choiceButtons[i].GetComponentInChildren<TMP_Text>().text = choices[i].choiceText;
                choiceButtons[i].gameObject.SetActive(true);
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
        choicesPanel.SetActive(true);
    }

    public void HideChoices()
    {
        if (choicesPanel == null || choiceButtons == null) return;

        foreach (var button in choiceButtons)
        {
            if (button != null)
                button.gameObject.SetActive(false);
        }
        choicesPanel.SetActive(false);
    }

    public void DisplayChoices(Dialogue.Choice[] choices)
    {
        HideChoices();
        ShowChoices(choices);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        this.dialogue = dialogue;
        currentNode = dialogue.dialogueNodes[0];
        isDialogueActive = true;
        ShowDialoguePanel();
        StartCoroutine(TypeLine(currentNode));
    }

    private IEnumerator TypeLine(Dialogue.DialogueNode node)
    {
        // Starting a new line/node: reset choice state
        choseChoice = false;
        dialogueText.text = "";
        if (node.npcName != null) nameText.text = node.npcName;
        else nameText.text = dialogue.defaultName;
        if (node.portrait != null) portraitImage.sprite = node.portrait;
        else portraitImage.sprite = dialogue.defaultPortrait;
        isTyping = true;
        HideChoices();
        foreach (char letter in node.text)
        {
            float timer = typingSpeed;

            while (timer > 0 && isTyping)
            {
                timer -= Time.unscaledDeltaTime;
                yield return null;
            }
            dialogueText.text += letter;
            if (!isTyping) break; //skip typing if the player presses space.
        }
        isTyping = false;
        dialogueText.text = node.text;

        // Determine which choices are actually available (non-null and non-empty)
        List<Dialogue.Choice> availableChoices = new List<Dialogue.Choice>();
        if (node.choices != null)
        {
            foreach (var choice in node.choices)
            {
                if (choice != null && !string.IsNullOrWhiteSpace(choice.choiceText))
                {
                    availableChoices.Add(choice);
                }
            }
        }

        hasChoices = availableChoices.Count > 0;
        if (hasChoices)
        {
            DisplayChoices(availableChoices.ToArray());
        }
        //dialog displayed now, waits for player to input a choice, or auto progress or manual progress.
    }

    public void ProgressDialogue()
    {
        if (currentNode.isEnd)
        {
            EndDialogue();
            return;
        }
        if (!hasChoices)
        {
            // Move to the next node by index if it's valid
            if (currentNode.nextNodeIndex >= 0 &&
                currentNode.nextNodeIndex < dialogue.dialogueNodes.Count)
            {
                currentNode = dialogue.dialogueNodes[currentNode.nextNodeIndex];
            }
            else
            {
                // No valid next node; end the dialogue
                EndDialogue();
                return;
            }
        }
        StartCoroutine(TypeLine(currentNode));
    }
    // Called by button listeners (wired in Awake)
    private void OnChoiceButton(int choiceIndex)
    {
        if (!isDialogueActive || !hasChoices) return;
        ChooseChoice(choiceIndex);
    }

    public void ChooseChoice(int choiceIndex)
    {
        if (choseChoice) return;
        choseChoice = true;
        // Follow the chosen node's next index if valid
        var choice = currentNode.choices[choiceIndex];
        if (choice != null &&
            choice.nextNodeIndex >= 0 &&
            choice.nextNodeIndex < dialogue.dialogueNodes.Count)
        {
            currentNode = dialogue.dialogueNodes[choice.nextNodeIndex];
        }
        else
        {
            // Invalid or terminal choice; end dialogue
            EndDialogue();
            return;
        }
        ProgressDialogue();
    }

    public void EndDialogue()
    {
        HideDialoguePanel();
    }


}
