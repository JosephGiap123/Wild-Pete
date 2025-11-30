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
    private List<Dialogue.Choice> currentAvailableChoices; // Track which choices are actually available
    private Coroutine typingCoroutine;
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
        currentAvailableChoices = null;
        dialogueText.text = "";
        nameText.text = "";
        portraitImage.sprite = null;
        HideChoices();

        // Stop any running typing coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
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

                // Check if this choice requires a trade and if player has the item
                if (choices[i].requiredItem != null)
                {
                    int playerHas = PlayerInventory.instance != null ?
                        PlayerInventory.instance.HasItem(choices[i].requiredItem.itemName) : 0;
                    bool hasEnough = playerHas >= choices[i].requiredQuantity;

                    // Disable button if player doesn't have required item
                    choiceButtons[i].interactable = hasEnough;

                    // Optionally gray out or add visual feedback
                    if (!hasEnough)
                    {
                        // You could add visual feedback here, like changing button color
                        Debug.Log($"Choice '{choices[i].choiceText}' requires {choices[i].requiredQuantity}x {choices[i].requiredItem.itemName}, but player only has {playerHas}");
                    }
                }
                else
                {
                    // No trade requirement, button is always interactable
                    choiceButtons[i].interactable = true;
                }
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
        if (dialogue == null || dialogue.dialogueNodes == null || dialogue.dialogueNodes.Count == 0)
        {
            Debug.LogError("DialogManager: Cannot start dialogue - dialogue is null or has no nodes!");
            return;
        }

        // Stop any existing typing coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        this.dialogue = dialogue;
        currentNode = dialogue.dialogueNodes[0];
        isDialogueActive = true;
        ShowDialoguePanel();
        typingCoroutine = StartCoroutine(TypeLine(currentNode));
    }

    private IEnumerator TypeLine(Dialogue.DialogueNode node)
    {
        // Starting a new line/node: reset choice state
        choseChoice = false;
        dialogueText.text = "";
        if (!string.IsNullOrEmpty(node.npcName)) nameText.text = node.npcName;
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
        currentAvailableChoices = availableChoices; // Store for ChooseChoice to use
        if (hasChoices)
        {
            DisplayChoices(availableChoices.ToArray());
        }
        //dialog displayed now, waits for player to input a choice, or auto progress or manual progress.
    }

    public void ProgressDialogue()
    {
        // End dialogue if node has no choices and no next node
        if (!hasChoices && currentNode.nextNodeIndex == -1)
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
        typingCoroutine = StartCoroutine(TypeLine(currentNode));
    }
    // Called by button listeners (wired in Awake)
    private void OnChoiceButton(int choiceIndex)
    {
        if (!isDialogueActive || !hasChoices) return;
        ChooseChoice(choiceIndex);
    }

    public void ChooseChoice(int choiceIndex)
    {
        if (choseChoice || !hasChoices || currentAvailableChoices == null) return;

        // Validate button index maps to an available choice
        if (choiceIndex < 0 || choiceIndex >= currentAvailableChoices.Count)
        {
            Debug.LogWarning($"DialogManager: Invalid choice index {choiceIndex} (available: {currentAvailableChoices.Count})");
            return;
        }

        // Use the choice from availableChoices, not the raw array
        var choice = currentAvailableChoices[choiceIndex];

        // Handle trade if this choice requires one
        if (choice.requiredItem != null)
        {
            if (PlayerInventory.instance == null)
            {
                Debug.LogError("DialogManager: PlayerInventory.instance is null! Cannot process trade.");
                return;
            }

            // Check if player has required item
            int playerHas = PlayerInventory.instance.HasItem(choice.requiredItem.itemName);
            if (playerHas < choice.requiredQuantity)
            {
                Debug.LogWarning($"DialogManager: Player doesn't have enough {choice.requiredItem.itemName} (has {playerHas}, needs {choice.requiredQuantity})");
                return; // Don't proceed with the choice
            }

            // Remove required item from inventory
            bool removed = PlayerInventory.instance.UseItem(choice.requiredItem.itemName, choice.requiredQuantity);
            if (!removed)
            {
                Debug.LogWarning($"DialogManager: Failed to remove {choice.requiredQuantity}x {choice.requiredItem.itemName} from inventory");
                return; // Don't proceed if removal failed
            }

            Debug.Log($"DialogManager: Removed {choice.requiredQuantity}x {choice.requiredItem.itemName} from inventory");

            // Add reward item if specified
            if (choice.rewardItem != null && choice.rewardQuantity > 0)
            {
                bool added = PlayerInventory.instance.AddItemFromItemSO(choice.rewardItem, choice.rewardQuantity);
                if (added)
                {
                    Debug.Log($"DialogManager: Added {choice.rewardQuantity}x {choice.rewardItem.itemName} to inventory");
                }
                else
                {
                    Debug.LogWarning($"DialogManager: Failed to add {choice.rewardQuantity}x {choice.rewardItem.itemName} to inventory (inventory may be full)");
                    // Note: We still proceed with the dialogue even if reward couldn't be added
                }
            }
        }

        choseChoice = true;

        if (choice != null &&
            choice.nextNodeIndex >= 0 &&
            choice.nextNodeIndex < dialogue.dialogueNodes.Count)
        {
            Debug.Log("Choosing choice: " + choice.choiceText + " with next node index: " + choice.nextNodeIndex);
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
