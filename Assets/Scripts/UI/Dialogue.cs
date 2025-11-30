using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue")]

public class Dialogue : ScriptableObject
{

    [System.Serializable]
    public class DialogueNode
    {
        public string text;
        public string npcName;
        public Sprite portrait;
        public Choice[] choices = new Choice[3];
        [Range(-1, 99)]
        public int nextNodeIndex = -1; // index into dialogueNodes, -1 means no next node
    }

    [System.Serializable]
    public class Choice
    {
        public string choiceText;
        [Range(-1, 99)]
        public int nextNodeIndex = -1; // index into dialogueNodes, -1 means end or no next

        [Header("Trade (Optional)")]
        [Tooltip("If set, player must have this item to select this choice")]
        public ItemSO requiredItem;
        [Tooltip("How many of the required item the player needs")]
        public int requiredQuantity = 1;
        [Tooltip("Item the player receives when selecting this choice")]
        public ItemSO rewardItem;
        [Tooltip("How many of the reward item the player receives")]
        public int rewardQuantity = 1;
    }

    public List<DialogueNode> dialogueNodes;

    public Sprite defaultPortrait;
    public string defaultName;
}
