using UnityEngine;
using TMPro;

/// <summary>
/// Displays a control's current key binding and updates when it changes.
/// Attach this to a UI element that shows "ControlName: KeyCode"
/// </summary>
public class ControlDisplayUI : MonoBehaviour
{
    [Header("Control to Display")]
    [SerializeField] private PlayerControls controlToDisplay;

    [Header("Event")]
    [SerializeField] private InputEvent controlChangedEvent; // Assign the InputEvent from ControlManager

    [Header("UI References")]
    [SerializeField] private TMP_Text displayText; // Text that shows "ControlName: KeyCode"

    [Header("Format")]
    [SerializeField] private string displayFormat = "{0}: {1}"; // Format: "{0}" = control name, "{1}" = key code

    private void OnEnable()
    {
        if (controlChangedEvent != null)
        {
            controlChangedEvent.onEventRaised.AddListener(OnControlChanged);
        }

        if (ControlManager.instance != null)
        {
            UpdateDisplay();
        }
        else
        {
            StartCoroutine(WaitForControlManager());
        }
    }

    private void OnDisable()
    {
        if (controlChangedEvent != null)
        {
            controlChangedEvent.onEventRaised.RemoveListener(OnControlChanged);
        }
    }

    private System.Collections.IEnumerator WaitForControlManager()
    {
        yield return new WaitWhile(() => ControlManager.instance == null);
        UpdateDisplay();
    }

    private void OnControlChanged(string inputStringName, PlayerControls inputName, KeyCode newKeyCode)
    {
        // Only update if this is the control we're displaying
        if (inputName == controlToDisplay)
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (displayText == null || ControlManager.instance == null) return;

        if (ControlManager.instance.inputMapping != null &&
            ControlManager.instance.inputMapping.ContainsKey(controlToDisplay))
        {
            KeyCode currentKey = ControlManager.instance.inputMapping[controlToDisplay];
            string controlName = controlToDisplay.ToString();
            displayText.text = string.Format(displayFormat, controlName, currentKey);
        }
        else
        {
            displayText.text = string.Format(displayFormat, controlToDisplay.ToString(), "None");
        }
    }
}

