using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Objective
{
    public enum ObjectiveType
    {
        CollectItem,
        TalkToNPC
    }

    [Header("Objective Info")]
    public ObjectiveType objectiveType;
    public string objectiveDescription;

    [Header("Collect Item Settings")]
    [Tooltip("List of items that count towards this objective")]
    public List<string> requiredItemNames = new List<string>();
    [Tooltip("How many items from the list are required (e.g., 2/4 means collect 2 out of 4 items)")]
    public int requiredItemCount = 1;

    [Header("Talk to NPC Settings")]
    [Tooltip("The NPC GameObject to talk to (optional - can use event instead)")]
    public GameObject targetNPC;
    [Tooltip("Event that fires when this NPC is talked to (assign in NPC component)")]
    public VoidEvents npcTalkedToEvent;

    [Header("Prerequisites")]
    [Tooltip("Indices of objectives that must be completed before this one becomes active (0-based index)")]
    public List<int> prerequisiteObjectiveIndices = new List<int>();

    [Header("Internal State (Do not edit)")]
    public bool isCompleted = false;
    public bool isUnlocked = false;
    public int collectedItemCount = 0;
    public HashSet<string> collectedItems = new HashSet<string>();
}

