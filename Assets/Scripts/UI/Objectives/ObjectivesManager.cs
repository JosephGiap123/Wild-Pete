using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ObjectivesManager : MonoBehaviour
{
	[Header("Activation")]
	[SerializeField] private VoidEvents activationEvent;
	[SerializeField] private bool activateOnStart = false;
	[SerializeField] private VoidEvents bossBarShownEvent;

	[Header("UI References")]
	[SerializeField] private GameObject objectivesPanel;
	[SerializeField] private TextMeshProUGUI objectivesText;

	[Header("Objectives")]
	[SerializeField] private List<Objective> objectives = new List<Objective>();

	[Header("Item Pickup Event")]
	[SerializeField] private ItemPickUpEvent itemPickUpEvent;

	[Header("Colors")]
	[SerializeField] private Color incompleteColor = Color.red;
	[SerializeField] private Color completeColor = Color.green;
	[SerializeField] private Color lockedColor = Color.gray;
	[SerializeField] private bool showLockedObjectives = true;

	private bool isActive = false;
	private int completedObjectivesCount = 0;
	private Dictionary<Objective, UnityEngine.Events.UnityAction> npcEventListeners = new Dictionary<Objective, UnityEngine.Events.UnityAction>();
	private bool allObjectivesCompletedAtCheckpoint = false; // Track if all objectives were completed when checkpoint was saved

	void Awake()
	{
		// Ensure panel starts inactive
		if (objectivesPanel != null)
		{
			objectivesPanel.SetActive(false);
		}
	}

	void OnEnable()
	{
		// Subscribe to activation event
		if (activationEvent != null)
		{
			activationEvent.onEventRaised.AddListener(ActivateObjectives);
		}

		if (bossBarShownEvent != null)
		{
			bossBarShownEvent.onEventRaised.AddListener(OnBossBarShown);
		}


		// Subscribe to respawn event to show objectives if needed
		GameRestartManager.CharacterRespawned += OnCharacterRespawned;

		// Subscribe to item pickup event
		if (itemPickUpEvent != null)
		{
			itemPickUpEvent.onEventRaised.AddListener(OnItemPickedUp);
		}

		// Subscribe to NPC talk events
		for (int i = 0; i < objectives.Count; i++)
		{
			var objective = objectives[i];
			if (objective.objectiveType == Objective.ObjectiveType.TalkToNPC)
			{
				// Use event from objective, or get it from targetNPC if not assigned
				VoidEvents npcEvent = objective.npcTalkedToEvent;
				if (npcEvent == null && objective.targetNPC != null)
				{
					NPC npc = objective.targetNPC.GetComponent<NPC>();
					if (npc != null)
					{
						npcEvent = npc.onDialogueEndEvent;
					}
				}

				if (npcEvent != null)
				{
					int index = i; // Capture index to avoid closure issues
					UnityEngine.Events.UnityAction listener = () => OnNPCTalkedTo(objectives[index]);
					npcEvent.onEventRaised.AddListener(listener);
					npcEventListeners[objective] = listener; // Store for proper cleanup
				}
			}
		}
	}

	void OnDisable()
	{
		// Unsubscribe from activation event
		if (activationEvent != null)
		{
			activationEvent.onEventRaised.RemoveListener(ActivateObjectives);
		}

		if (bossBarShownEvent != null)
		{
			bossBarShownEvent.onEventRaised.RemoveListener(OnBossBarShown);
		}

		// Unsubscribe from respawn event
		GameRestartManager.CharacterRespawned -= OnCharacterRespawned;

		// Unsubscribe from item pickup event
		if (itemPickUpEvent != null)
		{
			itemPickUpEvent.onEventRaised.RemoveListener(OnItemPickedUp);
		}

		// Unsubscribe from NPC talk events
		foreach (var kvp in npcEventListeners)
		{
			var objective = kvp.Key;
			var listener = kvp.Value;

			VoidEvents npcEvent = objective.npcTalkedToEvent;
			if (npcEvent == null && objective.targetNPC != null)
			{
				NPC npc = objective.targetNPC.GetComponent<NPC>();
				if (npc != null)
				{
					npcEvent = npc.onDialogueEndEvent;
				}
			}

			if (npcEvent != null && listener != null)
			{
				npcEvent.onEventRaised.RemoveListener(listener);
			}
		}
		npcEventListeners.Clear();
	}

	void Start()
	{
		if (activateOnStart)
		{
			ActivateObjectives();
		}
	}

	public void OnBossBarShown()
	{
		if (objectivesPanel != null)
		{
			objectivesPanel.SetActive(false);
		}
	}


	private void OnCharacterRespawned(Vector2 respawnLocation)
	{

		StartCoroutine(ShowObjectivesOnRespawn());
	}

	private IEnumerator ShowObjectivesOnRespawn()
	{
		// Wait a frame to ensure checkpoint restore has completed
		yield return null;

		if (isActive && !allObjectivesCompletedAtCheckpoint)
		{
			if (objectivesPanel != null)
			{
				objectivesPanel.SetActive(true);
			}
		}
	}
	public void ActivateObjectives()
	{
		if (isActive) return;

		isActive = true;
		InitializeObjectives();
		CreateObjectiveUI();

		if (objectivesPanel != null)
		{
			objectivesPanel.SetActive(true);
		}
	}

	private void InitializeObjectives()
	{
		// Reset all objectives
		foreach (var objective in objectives)
		{
			objective.isCompleted = false;
			objective.isUnlocked = false;
			objective.collectedItemCount = 0;
			objective.collectedItems.Clear();
		}
		completedObjectivesCount = 0;

		// Unlock objectives with no prerequisites
		UnlockObjectivesWithPrerequisites();
	}

	private void UnlockObjectivesWithPrerequisites()
	{
		for (int i = 0; i < objectives.Count; i++)
		{
			var objective = objectives[i];

			// Skip if already unlocked or completed
			if (objective.isUnlocked || objective.isCompleted) continue;

			// Check if all prerequisites are met
			bool allPrerequisitesMet = true;
			foreach (int prereqIndex in objective.prerequisiteObjectiveIndices)
			{
				if (prereqIndex < 0 || prereqIndex >= objectives.Count)
				{
					Debug.LogWarning($"ObjectivesManager: Invalid prerequisite index {prereqIndex} for objective at index {i}");
					continue;
				}

				if (!objectives[prereqIndex].isCompleted)
				{
					allPrerequisitesMet = false;
					break;
				}
			}

			// Unlock if all prerequisites are met (or if there are no prerequisites)
			if (allPrerequisitesMet && objective.prerequisiteObjectiveIndices.Count == 0)
			{
				objective.isUnlocked = true;
			}
			else if (allPrerequisitesMet)
			{
				objective.isUnlocked = true;
			}
		}
	}

	private void CreateObjectiveUI()
	{
		if (objectivesText == null)
		{
			Debug.LogError("ObjectivesManager: objectivesText is not assigned!");
			return;
		}

		UpdateObjectivesText();
	}

	private void UpdateObjectivesText()
	{
		if (objectivesText == null) return;

		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		bool isFirstObjective = true;

		for (int i = 0; i < objectives.Count; i++)
		{
			var objective = objectives[i];

			// Skip locked objectives - don't display them at all
			if (!objective.isUnlocked && !objective.isCompleted)
			{
				continue;
			}

			string objectiveText = GetObjectiveDisplayText(objective);

			// Determine color based on state
			Color textColor = objective.isCompleted ? completeColor : incompleteColor;

			string colorTag = $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>";
			string endColorTag = "</color>";

			// Add line break before each objective (except the first one)
			if (!isFirstObjective)
			{
				sb.Append("\n");
			}

			sb.Append($"{colorTag}{objectiveText}{endColorTag}");
			isFirstObjective = false;
		}

		objectivesText.text = sb.ToString();
	}

	private string GetObjectiveDisplayText(Objective objective)
	{
		if (objective.objectiveType == Objective.ObjectiveType.CollectItem)
		{
			if (objective.requiredItemCount == 1)
			{
				// Single item objective
				if (objective.requiredItemNames.Count > 0)
				{
					return $"Collect {objective.requiredItemNames[0]}";
				}
			}
			else
			{
				// Multiple items objective (e.g., "Collect 2/4 items: Shotgun, Gun, Knife, Hammer")
				string itemList = string.Join(", ", objective.requiredItemNames);
				return $"Collect {objective.collectedItemCount}/{objective.requiredItemCount} items: {itemList}";
			}
		}
		else if (objective.objectiveType == Objective.ObjectiveType.TalkToNPC)
		{
			string npcName = objective.targetNPC != null ? objective.targetNPC.name : "NPC";
			return $"Talk to {npcName}";
		}

		return objective.objectiveDescription;
	}

	private void OnItemPickedUp(Item item)
	{
		if (!isActive) return;

		foreach (var objective in objectives)
		{
			if (objective.isCompleted) continue;
			if (!objective.isUnlocked) continue; // Only track unlocked objectives
			if (objective.objectiveType != Objective.ObjectiveType.CollectItem) continue;

			// Check if this item is in the required list
			if (objective.requiredItemNames.Contains(item.itemName))
			{
				// Only count if we haven't collected this specific item yet
				if (!objective.collectedItems.Contains(item.itemName))
				{
					objective.collectedItems.Add(item.itemName);
					objective.collectedItemCount++;

					// Check if objective is complete
					if (objective.collectedItemCount >= objective.requiredItemCount)
					{
						CompleteObjective(objective);
					}
					else
					{
						UpdateObjectiveUI(objective);
					}
				}
			}
		}
	}

	private void OnNPCTalkedTo(Objective objective)
	{
		if (!isActive) return;
		if (objective.isCompleted) return;
		if (!objective.isUnlocked) return; // Only track unlocked objectives
		if (objective.objectiveType != Objective.ObjectiveType.TalkToNPC) return;

		CompleteObjective(objective);
	}

	private void CompleteObjective(Objective objective)
	{
		if (objective.isCompleted) return;

		objective.isCompleted = true;
		completedObjectivesCount++;

		// Unlock objectives that have this as a prerequisite
		UnlockObjectivesWithPrerequisites();

		// Re-render the entire objectives text
		UpdateObjectivesText();

		// Check if all objectives are complete
		if (completedObjectivesCount >= objectives.Count)
		{
			StartCoroutine(AllObjectivesComplete());
		}
	}

	private void UpdateObjectiveUI(Objective objective)
	{
		// Re-render the entire objectives text
		UpdateObjectivesText();
	}

	private IEnumerator AllObjectivesComplete()
	{
		yield return new WaitForSeconds(1f);
		if (objectivesPanel != null)
		{
			objectivesPanel.SetActive(false);
		}
	}

	// Save objectives state for checkpoint
	public CheckpointManager.ObjectivesStateData SaveObjectivesState()
	{
		CheckpointManager.ObjectivesStateData state = new CheckpointManager.ObjectivesStateData
		{
			isActive = isActive,
			completedObjectivesCount = completedObjectivesCount,
			allObjectivesCompletedAtCheckpoint = (completedObjectivesCount >= objectives.Count)
		};

		// Save state for each objective
		foreach (var objective in objectives)
		{
			state.objectiveCompletedStates.Add(objective.isCompleted);
			state.objectiveUnlockedStates.Add(objective.isUnlocked);
			state.objectiveCollectedItemCounts.Add(objective.collectedItemCount);

			// Convert HashSet to List for serialization
			List<string> collectedItemsList = new List<string>(objective.collectedItems);
			state.objectiveCollectedItems.Add(collectedItemsList);
		}

		allObjectivesCompletedAtCheckpoint = state.allObjectivesCompletedAtCheckpoint;
		return state;
	}

	// Restore objectives state from checkpoint
	public void RestoreObjectivesState(CheckpointManager.ObjectivesStateData state)
	{
		if (state == null) return;

		isActive = state.isActive;
		completedObjectivesCount = state.completedObjectivesCount;
		allObjectivesCompletedAtCheckpoint = state.allObjectivesCompletedAtCheckpoint;

		// Restore state for each objective
		for (int i = 0; i < objectives.Count && i < state.objectiveCompletedStates.Count; i++)
		{
			var objective = objectives[i];
			objective.isCompleted = state.objectiveCompletedStates[i];
			objective.isUnlocked = state.objectiveUnlockedStates[i];
			objective.collectedItemCount = state.objectiveCollectedItemCounts[i];

			// Convert List back to HashSet
			objective.collectedItems.Clear();
			if (i < state.objectiveCollectedItems.Count)
			{
				foreach (string itemName in state.objectiveCollectedItems[i])
				{
					objective.collectedItems.Add(itemName);
				}
			}
		}

		// Update UI
		UpdateObjectivesText();

		// Hide panel during restore - OnCharacterRespawned will show it if needed
		// This ensures objectives don't show until respawn is complete
		if (objectivesPanel != null)
		{
			objectivesPanel.SetActive(false);
		}
	}
}

