using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue")]

public class Dialogue : ScriptableObject
{
    public string npcName;
    public Sprite portrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public float typingSpeed = 0.05f;
    //can include audio after.
    public float autoProgressDelay = 1.5f;


}
