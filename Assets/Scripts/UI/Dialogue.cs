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
        public bool isEnd;
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
    }

    public List<DialogueNode> dialogueNodes;

    public Sprite defaultPortrait;
    public string defaultName;
}
