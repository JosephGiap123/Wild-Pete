using UnityEngine;
using UnityEngine.UI;

// Attach this to each keypad button to send its value to KeypadUI
public class KeypadButton : MonoBehaviour
{
    [SerializeField] private string buttonValue; // The value this button represents (e.g., "a", "b", "c", "1", "2", "#")
    private KeypadUI keypadUI;
    private Button button;
    
    public string GetButtonValue() => buttonValue;
    
    void Start()
    {
        // Find KeypadUI in parent
        keypadUI = GetComponentInParent<KeypadUI>();
        
        // Wire up button click
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
            Debug.Log($"[KeypadButton] Button '{buttonValue}' initialized on {gameObject.name}. Button component: {(button != null ? "Found" : "MISSING")}, KeypadUI: {(keypadUI != null ? "Found" : "MISSING")}, Interactable: {button.interactable}");
        }
        else
        {
            Debug.LogError($"[KeypadButton] Button component MISSING on GameObject: {gameObject.name}");
        }
        
        if (keypadUI == null)
        {
            Debug.LogError($"[KeypadButton] KeypadUI component NOT FOUND in parent hierarchy for button: {gameObject.name}");
        }
        
        if (string.IsNullOrEmpty(buttonValue))
        {
            Debug.LogWarning($"[KeypadButton] Button value is EMPTY for button: {gameObject.name}");
        }
    }
    
    private void OnButtonClick()
    {
        Debug.Log($"[KeypadButton] Button clicked! Value: '{buttonValue}', GameObject: {gameObject.name}, Button interactable: {(button != null ? button.interactable.ToString() : "NULL")}");
        
        if (button != null && !button.interactable)
        {
            Debug.LogWarning($"[KeypadButton] Button '{buttonValue}' is NOT INTERACTABLE! Click ignored.");
            return;
        }
        
        if (keypadUI == null)
        {
            Debug.LogError($"[KeypadButton] KeypadUI is NULL! Cannot send button press for: {buttonValue}");
            return;
        }
        
        if (string.IsNullOrEmpty(buttonValue))
        {
            Debug.LogError($"[KeypadButton] Button value is EMPTY! Cannot send button press from: {gameObject.name}");
            return;
        }
        
        Debug.Log($"[KeypadButton] Sending button press to KeypadUI: '{buttonValue}'");
        keypadUI.OnButtonPressed(buttonValue);
    }
    
    // Debug method to check button state in editor
    void OnValidate()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        
        if (button != null && !button.interactable)
        {
            Debug.LogWarning($"[KeypadButton] Button '{buttonValue}' on {gameObject.name} is NOT INTERACTABLE!");
        }
    }
}

