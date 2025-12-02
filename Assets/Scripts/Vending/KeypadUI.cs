// Assets/Scripts/Vending/KeypadUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeypadUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private GameObject vendingPopup; // set at runtime so Hide() can return to vending
    private VendingPopupInteractable vendingPopupController; // Reference to controller to change sprite
    
    [Header("Wire Connection Check")]
    [SerializeField] private WireConnectionGame wireGameReference; // Reference to check if wires are connected
    [SerializeField] private List<Button> keypadButtons = new List<Button>(); // All buttons on the keypad

    [Header("Pin Code Game")]
    [SerializeField] private string correctCode = "abc"; // The correct 3-character code
    [SerializeField] private TextMeshProUGUI displayText; // Text to show current input
    [SerializeField] private TextMeshProUGUI feedbackText; // Text to show feedback (correct/incorrect count)
    [SerializeField] private int codeLength = 3; // Length of the code
    
    private string currentInput = ""; // Current input string
    private bool isComplete = false; // Whether the code has been solved
    private float originalFontSize = 0f; // Store original font size

    void Reset()
    {
        cg = GetComponent<CanvasGroup>();
    }
    
    public void SetVendingPopup(GameObject go) => vendingPopup = go;
    
    public void SetVendingPopupController(VendingPopupInteractable controller) => vendingPopupController = controller;
    
    public void SetWireGameReference(WireConnectionGame wireGame) => wireGameReference = wireGame;
    
    // Called when a keypad button is pressed
    public void OnButtonPressed(string value)
    {
        Debug.Log($"[KeypadUI] OnButtonPressed called with value: '{value}'");
        
        // # button clears the selection (works at any time, even if complete or wires not connected)
        if (value == "#")
        {
            Debug.Log("[KeypadUI] # button pressed - clearing selection");
            // Cancel any pending feedback clear
            CancelInvoke(nameof(ClearFeedback));
            // Clear input and reset display
            currentInput = "";
            UpdateDisplay();
            return;
        }
        
        if (isComplete)
        {
            Debug.Log("[KeypadUI] Button press ignored - game is already complete");
            return; // Don't accept input if already complete
        }
        
        if (!wireGameReference || !wireGameReference.IsComplete())
        {
            Debug.LogWarning($"[KeypadUI] Button press ignored - wires not connected. wireGameReference: {(wireGameReference != null ? "Found" : "NULL")}, IsComplete: {(wireGameReference != null ? wireGameReference.IsComplete().ToString() : "N/A")}");
            return; // Don't accept if wires not connected
        }
        
        Debug.Log($"[KeypadUI] Processing button press: '{value}', Current input length: {currentInput.Length}");
        
        // Cancel any pending feedback clear
        CancelInvoke(nameof(ClearFeedback));
        
        // Switch display back to input mode (from feedback)
        // This happens when clicking a button for new selection
        UpdateDisplay();
        
        // Add character to current input
        if (currentInput.Length < codeLength)
        {
            currentInput += value;
            UpdateDisplay();
            
            // Auto-submit when 3 characters are entered
            if (currentInput.Length >= codeLength)
            {
                CheckCode();
            }
        }
    }
    
    private void ClearInput()
    {
        currentInput = "";
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            // Restore original font size when switching back to input mode
            if (originalFontSize > 0f)
            {
                displayText.fontSize = originalFontSize;
            }
            
            if (currentInput.Length == 0)
            {
                // First time - show "Enter Code" on one line, blanks on next line
                displayText.text = "Enter Code\n_ _ _";
            }
            else
            {
                // Show current input with placeholders for remaining characters
                string display = "Enter Code\n";
                for (int i = 0; i < codeLength; i++)
                {
                    if (i < currentInput.Length)
                    {
                        display += currentInput[i];
                    }
                    else
                    {
                        display += "_";
                    }
                    // Add space between characters (except after last one)
                    if (i < codeLength - 1)
                    {
                        display += " ";
                    }
                }
                displayText.text = display;
            }
        }
    }
    
    private void CheckCode()
    {
        if (currentInput.Length != codeLength) return;
        
        // Track which characters in the code have been "used" for yellow highlighting
        bool[] codeUsed = new bool[codeLength];
        string[] charColors = new string[codeLength]; // Store color tags for each character
        
        // First pass: Mark correct positions (green)
        for (int i = 0; i < codeLength; i++)
        {
            if (i < currentInput.Length && i < correctCode.Length)
            {
                if (currentInput[i] == correctCode[i])
                {
                    // Correct position - mark as green
                    charColors[i] = "<color=#33CC33>"; // Green color
                    codeUsed[i] = true; // Mark this position in code as used
                }
            }
        }
        
        // Second pass: Mark characters in code but wrong position (yellow)
        for (int i = 0; i < codeLength; i++)
        {
            if (i < currentInput.Length && i < correctCode.Length)
            {
                // Skip if already marked green
                if (currentInput[i] == correctCode[i])
                {
                    continue;
                }
                
                // Check if this character exists elsewhere in the code
                char inputChar = currentInput[i];
                for (int j = 0; j < codeLength; j++)
                {
                    if (!codeUsed[j] && correctCode[j] == inputChar)
                    {
                        // Found in code at position j - mark as yellow
                        charColors[i] = "<color=#FFCC00>"; // Yellow color
                        codeUsed[j] = true; // Mark this position as used
                        break; // Only mark once per character in input
                    }
                }
            }
        }
        
        // Build colored display text
        if (displayText != null)
        {
            // Store original font size if not already stored
            if (originalFontSize == 0f)
            {
                originalFontSize = displayText.fontSize;
            }
            
            // Restore original font size
            displayText.fontSize = originalFontSize;
            
            // Check if code is completely correct
            int correctCount = 0;
            for (int i = 0; i < codeLength; i++)
            {
                if (i < currentInput.Length && i < correctCode.Length && currentInput[i] == correctCode[i])
                {
                    correctCount++;
                }
            }
            
            if (correctCount == codeLength)
            {
                // Make font smaller so "DISPENSING" fits on one line
                displayText.fontSize = originalFontSize * 0.6f;
                displayText.text = "DISPENSING";
            }
            else
            {
                // Build colored text display
                string display = "Enter Code\n";
                for (int i = 0; i < codeLength; i++)
                {
                    if (i < currentInput.Length)
                    {
                        // Apply color if set, otherwise use default (white)
                        if (!string.IsNullOrEmpty(charColors[i]))
                        {
                            display += charColors[i] + currentInput[i] + "</color>";
                        }
                        else
                        {
                            display += currentInput[i];
                        }
                    }
                    else
                    {
                        display += "_";
                    }
                    // Add space between characters (except after last one)
                    if (i < codeLength - 1)
                    {
                        display += " ";
                    }
                }
                displayText.text = display;
            }
        }
        
        // Check if code is completely correct BEFORE clearing input
        int correctCountFinal = 0;
        for (int i = 0; i < codeLength; i++)
        {
            if (i < currentInput.Length && i < correctCode.Length && currentInput[i] == correctCode[i])
            {
                correctCountFinal++;
            }
        }
        
        // Clear current input so next button press starts fresh
        currentInput = "";
        
        Debug.Log($"[KeypadUI] Input checked, showing colored text feedback. Correct: {correctCountFinal}/{codeLength}");
        
        if (correctCountFinal == codeLength)
        {
            OnCodeCorrect();
        }
        else
        {
            // Don't auto-clear feedback - it stays until user clicks a button
            // User clicking a button will switch display back to input mode
        }
    }
    
    private void ClearFeedback()
    {
        // No feedback text needed - feedback is shown in display text colors
    }
    
    private void ResetInput()
    {
        currentInput = "";
        UpdateDisplay();
        ClearFeedback();
    }
    
    // Public method to reset the keypad game state (for checkpoint restoration)
    public void ResetGame()
    {
        isComplete = false;
        currentInput = "";
        UpdateDisplay();
        ClearFeedback();
    }
    
    private void OnCodeCorrect()
    {
        isComplete = true;
        
        // Display text already shows "DISPENSING" from CheckCode()
        // Clear feedback text
        ClearFeedback();
        
        Debug.Log("[KeypadUI] Code correct! Showing DISPENSING for 3 seconds, then returning to vending popup.");
        
        // Change vending machine sprite to empty version
        if (vendingPopupController != null)
        {
            vendingPopupController.OnVendingMachineEmpty();
        }
        else
        {
            Debug.LogWarning("[KeypadUI] VendingPopupController is null, cannot change sprite to empty version");
        }
        
        // Return to vending popup after 3 seconds
        Invoke(nameof(ReturnToVending), 3f);
    }
    
    private void ReturnToVending()
    {
        Hide();
    }

    public void Show()
    {
        // Ensure the GameObject is active
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        if (!cg) cg = GetComponent<CanvasGroup>();
        gameObject.SetActive(true);
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
        
        // Check if wires are connected - this will disable buttons if not connected
        bool wiresConnected = wireGameReference != null && wireGameReference.IsComplete();
        if (!wiresConnected)
        {
            Debug.Log($"[KeypadUI] Wires not connected - keypad buttons will be disabled. wireGameReference: {(wireGameReference != null ? "Found" : "NULL")}, IsComplete: {(wireGameReference != null ? wireGameReference.IsComplete().ToString() : "N/A")}");
        }
        
        // Reset input when showing (unless already complete)
        if (!isComplete)
        {
            ResetInput();
        }
        
        // Update button states - this will disable buttons if wires aren't connected
        UpdateButtonStates();
    }
    
    private void UpdateButtonStates()
    {
        bool wiresConnected = wireGameReference != null && wireGameReference.IsComplete();
        
        Debug.Log($"[KeypadUI] UpdateButtonStates - Wires connected: {wiresConnected}, Button count: {keypadButtons.Count}");
        
        foreach (var button in keypadButtons)
        {
            if (button != null)
            {
                button.interactable = wiresConnected;
                
                // Visual feedback - gray out if not connected
                var colors = button.colors;
                if (!wiresConnected)
                {
                    colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                button.colors = colors;
                
                // Debug log for each button
                KeypadButton keypadButton = button.GetComponent<KeypadButton>();
                string buttonValue = keypadButton != null ? keypadButton.GetButtonValue() : "UNKNOWN";
                Debug.Log($"[KeypadUI] Button '{buttonValue}' on {button.gameObject.name} - Interactable: {button.interactable}");
            }
            else
            {
                Debug.LogWarning("[KeypadUI] NULL button found in keypadButtons list!");
            }
        }
        
        if (!wiresConnected)
        {
            Debug.Log("[KeypadUI] Wires not connected - keypad buttons disabled");
        }
    }

    public void Hide()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        gameObject.SetActive(false);

        // Return to vending screen
        if (vendingPopup) vendingPopup.SetActive(true);
    }
}
