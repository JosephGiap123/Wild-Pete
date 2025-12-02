using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button that listens for a key press to rebind a control.
/// Attach this to the "Change" button for each control.
/// </summary>
public class ControlRebindButton : MonoBehaviour
{
	[Header("Control to Rebind")]
	[SerializeField] private PlayerControls controlToRebind;

	[Header("UI References")]
	[SerializeField] private Button rebindButton; // The button this script is on (auto-assigned if null)
	[SerializeField] private Image buttonImage; // Image component on the button (auto-assigned if null)

	[Header("Button Sprites")]
	[SerializeField] private Sprite idleSprite; // Sprite to show when button is idle
	[SerializeField] private Sprite listeningSprite; // Sprite to show when button is listening for input

	[Header("Settings")]
	[SerializeField] private KeyCode cancelKey = KeyCode.Escape; // Key to cancel rebinding

	private bool isListening = false;

	// Static flag to prevent multiple buttons from listening at once
	private static bool isAnyButtonListening = false;
	private static ControlRebindButton currentListeningButton = null;

	private void Awake()
	{
		// Auto-assign button if not set
		if (rebindButton == null)
		{
			rebindButton = GetComponent<Button>();
		}

		// Auto-assign button image if not set
		if (buttonImage == null && rebindButton != null)
		{
			buttonImage = rebindButton.GetComponent<Image>();
		}

		// Store original sprite if not set
		if (buttonImage != null && idleSprite == null)
		{
			idleSprite = buttonImage.sprite;
		}
	}

	private void OnEnable()
	{
		// Ensure button is interactable when re-enabled
		if (rebindButton != null)
		{
			rebindButton.interactable = true;
		}

		// Ensure sprite is set to idle when re-enabled
		if (buttonImage != null && idleSprite != null && !isListening)
		{
			buttonImage.sprite = idleSprite;
		}
	}

	private void Start()
	{
		// Wire up button click
		if (rebindButton != null)
		{
			rebindButton.onClick.AddListener(OnRebindButtonClicked);
		}
	}

	private void Update()
	{
		if (!isListening) return;

		// Check for cancel key
		if (Input.GetKeyDown(cancelKey))
		{
			CancelRebinding();
			return;
		}

		// Check for valid key press from ControlManager's allowed keys
		if (ControlManager.instance != null && ControlManager.instance.keyCodeConnection != null)
		{
			foreach (KeyCode validKey in ControlManager.instance.keyCodeConnection)
			{
				if (Input.GetKeyDown(validKey))
				{
					// Found a valid key - try to change the input
					TryChangeInput(validKey);
					return;
				}
			}
		}
	}

	private void OnRebindButtonClicked()
	{
		if (isListening)
		{
			// Already listening - cancel
			CancelRebinding();
			return;
		}

		// If another button is already listening, cancel it first
		if (isAnyButtonListening && currentListeningButton != null && currentListeningButton != this)
		{
			currentListeningButton.CancelRebinding();
		}

		StartListening();
	}

	private void StartListening()
	{
		// Prevent starting if another button is already listening
		if (isAnyButtonListening && currentListeningButton != null && currentListeningButton != this)
		{
			Debug.LogWarning($"[ControlRebindButton] Another button is already listening. Cannot start listening for {controlToRebind}.");
			return;
		}

		isListening = true;
		isAnyButtonListening = true;
		currentListeningButton = this;

		// Change button sprite to listening sprite
		if (buttonImage != null && listeningSprite != null)
		{
			buttonImage.sprite = listeningSprite;
		}

		// Disable button interaction while listening
		if (rebindButton != null)
		{
			rebindButton.interactable = false;
		}

		Debug.Log($"[ControlRebindButton] Listening for key to rebind {controlToRebind}...");
	}

	private void TryChangeInput(KeyCode newKey)
	{
		if (ControlManager.instance == null)
		{
			Debug.LogError("[ControlRebindButton] ControlManager.instance is null!");
			StopListening();
			return;
		}

		// Check if key is valid (not already in use)
		if (!ControlManager.instance.CheckInputExists(newKey))
		{
			Debug.LogWarning($"[ControlRebindButton] Key {newKey} is already in use!");
			// Show error feedback (you could flash the button or show a message)
			StopListening();
			return;
		}

		// Change the input through ControlManager
		ControlManager.instance.ChangeInput(controlToRebind, newKey);

		Debug.Log($"[ControlRebindButton] Changed {controlToRebind} to {newKey}");

		// Stop listening
		StopListening();
	}

	private void CancelRebinding()
	{
		Debug.Log($"[ControlRebindButton] Cancelled rebinding {controlToRebind}");
		StopListening();
	}

	private void StopListening()
	{
		isListening = false;

		// Clear static flags if this was the listening button
		if (currentListeningButton == this)
		{
			isAnyButtonListening = false;
			currentListeningButton = null;
		}

		// Restore button sprite to idle sprite
		if (buttonImage != null && idleSprite != null)
		{
			buttonImage.sprite = idleSprite;
		}

		// Re-enable button
		if (rebindButton != null)
		{
			rebindButton.interactable = true;
		}
	}


	private void OnDisable()
	{
		// Stop listening if the button is disabled (e.g., settings panel closes)
		if (isListening)
		{
			StopListening();
		}
	}

	private void OnDestroy()
	{
		// Clean up button listener
		if (rebindButton != null)
		{
			rebindButton.onClick.RemoveListener(OnRebindButtonClicked);
		}

		// Clean up listening state if this was the active listener
		if (currentListeningButton == this)
		{
			isAnyButtonListening = false;
			currentListeningButton = null;
		}
	}
}

