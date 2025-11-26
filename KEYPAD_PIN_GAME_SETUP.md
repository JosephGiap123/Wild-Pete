# Keypad Pin Code Game Implementation Guide

This guide will help you set up the pin code game for the vending machine keypad.

## Overview

The pin code game allows players to:
- Enter a 3-character combination using the keypad buttons (a, b, c, 1, 2, #)
- Auto-submit after 3 characters are entered
- Receive feedback showing how many characters are correct/incorrect
- Successfully complete the game and return to vending popup

---

## STEP 1: Add KeypadButton Component to Each Button

For each keypad button (a, b, c, 1, 2, #), you need to add the `KeypadButton` component:

1. **Open KeypadUI prefab**: `Assets/Prefabs/KeypadUI.prefab`
2. **Find each button GameObject** in the Hierarchy:
   - Button "a" (or "A")
   - Button "b" (or "B")
   - Button "c" (or "C")
   - Button "1"
   - Button "2"
   - Button "#" (or "Hash" or "Pound")

3. **For each button**:
   - Select the button GameObject
   - Click **Add Component**
   - Search for and add **KeypadButton** component
   - In the **KeypadButton** component:
     - Set **Button Value** to the character it represents:
       - Button "a" → Button Value: `a`
       - Button "b" → Button Value: `b`
       - Button "c" → Button Value: `c`
       - Button "1" → Button Value: `1`
       - Button "2" → Button Value: `2`
       - Button "#" → Button Value: `#`

**Note**: The Button Value is case-sensitive and must match exactly what you want to send to the keypad.

---

## STEP 2: Create Display Text UI Element

You need a TextMeshProUGUI component to show the current input (e.g., "a_b" or "abc"):

1. **In KeypadUI prefab**, create a new GameObject:
   - Right-click on **KeypadUI** root → **UI** → **Text - TextMeshPro**
   - Name it `DisplayText`

2. **Position it** where you want the input to be displayed (usually above or on the keypad)

3. **Configure the TextMeshProUGUI**:
   - Set **Font Size** to something readable (e.g., 24-36)
   - Set **Alignment** to Center
   - Set **Text** to `___` (three underscores) as placeholder
   - Set **Color** to white or a visible color

4. **In KeypadUI component**:
   - Drag `DisplayText` GameObject into the **Display Text** field

---

## STEP 3: Create Feedback Text UI Element

You need a TextMeshProUGUI component to show feedback messages:

1. **In KeypadUI prefab**, create a new GameObject:
   - Right-click on **KeypadUI** root → **UI** → **Text - TextMeshPro**
   - Name it `FeedbackText`

2. **Position it** below the display text or in a visible area

3. **Configure the TextMeshProUGUI**:
   - Set **Font Size** to something readable (e.g., 18-24)
   - Set **Alignment** to Center
   - Leave **Text** empty (will be set by script)
   - Set **Color** to a visible color (e.g., yellow for feedback)

4. **In KeypadUI component**:
   - Drag `FeedbackText` GameObject into the **Feedback Text** field

---

## STEP 4: Configure KeypadUI Component

1. **Select root KeypadUI GameObject** in the prefab

2. **In KeypadUI component**, find the **Pin Code Game** section:

### Set Correct Code:
- **Correct Code**: Enter the 3-character code players need to guess
  - Default is `abc`
  - Examples: `12#`, `a1b`, `c2a`, etc.
  - **Important**: This must match the Button Values you set in Step 1

### Assign Text References:
- **Display Text**: Drag the `DisplayText` GameObject you created
- **Feedback Text**: Drag the `FeedbackText` GameObject you created

### Code Length:
- **Code Length**: Should be `3` (default)
  - This determines how many characters are needed before auto-submit

---

## STEP 5: Test the Setup

### 5.1 Test in Scene

1. **Open your Prison scene** (or test scene)
2. **Play the game**
3. **Interact with vending machine** → Popup should appear
4. **Complete wire game first**:
   - Click SidePanel
   - Unscrew both screws
   - Connect all wires correctly
5. **Click PinScreen** → Keypad should open
6. **Try clicking keypad buttons**:
   - Buttons should now be **enabled** (wires are connected)
   - Click 3 buttons (e.g., "a", "b", "c")
   - Display should show your input (e.g., "abc")
   - After 3 characters, it should auto-submit
   - Feedback should appear: "Correct: X | Incorrect: Y"
7. **If correct**:
   - Feedback shows "CORRECT!"
   - After 1 second, returns to vending popup
8. **If incorrect**:
   - Feedback shows correct/incorrect count
   - After 1.5 seconds, input resets
   - Try again!

---

## STEP 6: Customize the Correct Code

To change the correct code:

1. **Open KeypadUI prefab**
2. **Select root KeypadUI GameObject**
3. **In KeypadUI component**, change **Correct Code** field
4. **Make sure** the Button Values you set in Step 1 include all characters in your code

**Example**: If you want code `12#`:
- Button "1" must have Button Value: `1`
- Button "2" must have Button Value: `2`
- Button "#" must have Button Value: `#`

---

## Troubleshooting

### Buttons don't work:
- **Check**: Are wires connected? Buttons only work after wire game is complete
- **Check**: Does each button have `KeypadButton` component?
- **Check**: Is Button Value set correctly on each button?

### Display text doesn't update:
- **Check**: Is Display Text assigned in KeypadUI component?
- **Check**: Is the TextMeshProUGUI component enabled?

### Feedback doesn't show:
- **Check**: Is Feedback Text assigned in KeypadUI component?
- **Check**: Is the TextMeshProUGUI component enabled?

### Code never accepts:
- **Check**: Does Correct Code match the Button Values exactly?
- **Check**: Is Correct Code exactly 3 characters?
- **Check**: Are you using the right case (a vs A)?

### Auto-submit doesn't work:
- **Check**: Is Code Length set to 3?
- **Check**: Are you clicking exactly 3 buttons?

---

## Advanced: Change Code Length

If you want a different code length (not 3):

1. **In KeypadUI component**, change **Code Length** to your desired number
2. **Update Correct Code** to match the new length
3. **Update Display Text placeholder** to show correct number of underscores
   - For length 4: `____`
   - For length 5: `_____`
   - etc.

---

## Next Steps (Future Implementation)

After the pin code game is working, you'll need to:
1. Hide bread from vending popup when code is correct
2. Add clickable area to claim bread
3. Add any additional game logic

---

## Summary Checklist

- [ ] Added `KeypadButton` component to all buttons (a, b, c, 1, 2, #)
- [ ] Set Button Value on each KeypadButton component
- [ ] Created DisplayText TextMeshProUGUI element
- [ ] Created FeedbackText TextMeshProUGUI element
- [ ] Assigned Display Text in KeypadUI component
- [ ] Assigned Feedback Text in KeypadUI component
- [ ] Set Correct Code in KeypadUI component
- [ ] Tested that buttons work after wire game is complete
- [ ] Tested that display updates when buttons are clicked
- [ ] Tested that feedback shows after 3 characters
- [ ] Tested that correct code returns to vending popup

---

## Code Reference

### KeypadButton.cs
- Attach to each keypad button
- Set `buttonValue` to the character the button represents
- Automatically wires up button click to KeypadUI

### KeypadUI.cs
- Main component on KeypadUI prefab
- Handles pin code game logic
- `OnButtonPressed(string value)` - Called by KeypadButton components
- `correctCode` - The code players need to guess
- `codeLength` - How many characters before auto-submit

