# Wire Connection Game Implementation Guide

This guide will help you implement the wire connection minigame that appears after unscrewing the side panel. The game works like Among Us - you connect colored wires on the left to matching colored targets on the right.

---

## Overview

**Flow:**
1. Player opens vending machine → Popup appears
2. Player clicks SidePanel → Screw panel opens
3. Player unscrews screws → Panel opens, wire game appears
4. Player connects wires (match colors) → Machine powers on
5. Player clicks PinScreen → Keypad opens, buttons now work (wires connected)

**State Persistence:**
- Screws stay unscrewed if you exit and come back
- Wires stay connected if you exit and come back
- Keypad buttons remain enabled once wires are connected

---

## STEP 1: Set Up Wire Game in ScrewPanelClosed Prefab

### 1.1 Open ScrewPanelClosed Prefab

1. Navigate to `Assets/Prefabs/ScrewPanelClosed.prefab` in Project window
2. **Double-click** to open in Prefab Mode (or select and click "Open Prefab" button)

### 1.2 Create Wire Game Container

1. **Select the root ScrewPanelClosed GameObject** in Hierarchy
2. **Right-click** → **Create Empty**
3. **Rename** to: `WireGameContainer`
4. **Select WireGameContainer** and configure:

#### RectTransform:
- **Anchor Presets**: Click **stretch-stretch** (bottom-right)
- **Left**: `0`
- **Right**: `0`
- **Top**: `0`
- **Bottom**: `0`
- **Pos X**: `0`
- **Pos Y**: `0`
- **Pos Z**: `0`
- **Scale**: `(1, 1, 1)`

#### WireConnectionGame Component:
- **Add Component** → Search for `WireConnectionGame`
- **Canvas Group**: Will be auto-assigned (add CanvasGroup component if missing)
  - If missing: **Add Component** → **Canvas Group** first, then it will auto-assign
- **Vending Popup**: Leave empty (set at runtime automatically)
- **Wires**: Leave empty for now (we'll add wires in step 4.1)
- **Target Points**: Leave empty for now (we'll add targets in step 4.2)
- **Allow Multiple Connections**: `☐` (unchecked)

---

## STEP 2: Create Wire Images (Left Side)

These are the wires the player clicks to select and connect.

### 2.1 Create Wire Parent Container

1. **Right-click WireGameContainer** → **Create Empty**
2. **Rename** to: `WiresLeft`
3. **Select WiresLeft** and configure:

#### RectTransform:
- **Anchor Presets**: Click **left-center**
- **Pos X**: `-150` (adjust based on your layout)
- **Pos Y**: `0`
- **Pos Z**: `0`
- **Width**: `100`
- **Height**: `200`
- **Scale**: `(1, 1, 1)`

### 2.2 Create Individual Wire Images

For each wire you want (example: 4 wires - Red, Blue, Yellow, Green):

#### Wire 1 (Red):
1. **Right-click WiresLeft** → **UI** → **Image**
2. **Rename** to: `WireRed`
3. **Select WireRed** and configure:

##### RectTransform:
- **Anchor Presets**: Click **left-center**
- **Pos X**: `0`
- **Pos Y**: `60` (adjust spacing - first wire)
- **Pos Z**: `0`
- **Width**: `40` (wire width)
- **Height**: `30` (wire height)
- **Scale**: `(1, 1, 1)`

##### Image Component:
- **Source Image**: Your red wire sprite
- **Color**: `Red` (255, 0, 0, 255)
- **Raycast Target**: `☑` (checked)

##### Button Component:
- **Add Component** → **Button**
- Button will be auto-configured by WireConnectionGame script

#### Wire 2 (Blue):
1. **Right-click WiresLeft** → **UI** → **Image**
2. **Rename** to: `WireBlue`
3. **Configure same as WireRed** but:
   - **Pos Y**: `20` (second wire, adjust spacing)
   - **Color**: `Blue` (0, 0, 255, 255)

#### Wire 3 (Yellow):
1. **Right-click WiresLeft** → **UI** → **Image**
2. **Rename** to: `WireYellow`
3. **Configure same as WireRed** but:
   - **Pos Y**: `-20` (third wire)
   - **Color**: `Yellow` (255, 255, 0, 255)

#### Wire 4 (Green):
1. **Right-click WiresLeft** → **UI** → **Image**
2. **Rename** to: `WireGreen`
3. **Configure same as WireRed** but:
   - **Pos Y**: `-60` (fourth wire)
   - **Color**: `Green` (0, 255, 0, 255)

---

## STEP 3: Create Target Points (Right Side)

These are where the wires connect to. They should match the wire colors.

### 3.1 Create Target Parent Container

1. **Right-click WireGameContainer** → **Create Empty**
2. **Rename** to: `TargetsRight`
3. **Select TargetsRight** and configure:

#### RectTransform:
- **Anchor Presets**: Click **right-center**
- **Pos X**: `150` (adjust based on your layout)
- **Pos Y**: `0`
- **Pos Z**: `0`
- **Width**: `100`
- **Height**: `200`
- **Scale**: `(1, 1, 1)`

### 3.2 Create Individual Target Images

Create targets in the **same order** as wires (important for correct matching):

#### Target 0 (First target - connects to first wire):
1. **Right-click TargetsRight** → **UI** → **Image**
2. **Rename** to: `Target0`
3. **Select Target0** and configure:

##### RectTransform:
- **Anchor Presets**: Click **right-center**
- **Pos X**: `0`
- **Pos Y**: `60` (same Y position as first wire)
- **Pos Z**: `0`
- **Width**: `40`
- **Height**: `30`
- **Scale**: `(1, 1, 1)`

##### Image Component:
- **Source Image**: Your target/connection point sprite
- **Color**: `White` (255, 255, 255, 255) - will change to wire color when connected
- **Raycast Target**: `☑` (checked)

##### Button Component:
- **Add Component** → **Button**
- Button will be auto-configured by WireConnectionGame script

#### Target 1, 2, 3:
- Create 3 more targets with same setup
- **Pos Y**: `20`, `-20`, `-60` (matching wire positions)
- **Rename**: `Target1`, `Target2`, `Target3`

---

## STEP 4: Configure WireConnectionGame Component

1. **Select WireGameContainer** in Hierarchy
2. In **WireConnectionGame** component:

### 4.1 Add Wires to List

1. **Click the + button** in **Wires** list (4 times for 4 wires)
2. For each wire entry:

#### Wire 0 (Red):
- **Wire Image**: Drag `WireRed` from Hierarchy
- **Target Image**: Leave empty (not used)
- **Wire Color**: Click color picker → Select **Red** (255, 0, 0, 255)
- **Correct Target Index**: `0` (connects to Target0)

#### Wire 1 (Blue):
- **Wire Image**: Drag `WireBlue` from Hierarchy
- **Target Image**: Leave empty
- **Wire Color**: **Blue** (0, 0, 255, 255)
- **Correct Target Index**: `1` (connects to Target1)

#### Wire 2 (Yellow):
- **Wire Image**: Drag `WireYellow` from Hierarchy
- **Target Image**: Leave empty
- **Wire Color**: **Yellow** (255, 255, 0, 255)
- **Correct Target Index**: `2` (connects to Target2)

#### Wire 3 (Green):
- **Wire Image**: Drag `WireGreen` from Hierarchy
- **Target Image**: Leave empty
- **Wire Color**: **Green** (0, 255, 0, 255)
- **Correct Target Index**: `3` (connects to Target3)

### 4.2 Add Target Points to List

1. **Click the + button** in **Target Points** list (4 times)
2. Drag in order:
   - **Element 0**: Drag `Target0`
   - **Element 1**: Drag `Target1`
   - **Element 2**: Drag `Target2`
   - **Element 3**: Drag `Target3`

**Important:** The order matters! Target index 0 = first target, index 1 = second target, etc.

### 4.3 Final WireConnectionGame Settings

- **Canvas Group**: Should auto-assign (if not, drag `WireGameContainer` GameObject here)
  - If CanvasGroup component is missing, **Add Component** → **Canvas Group** first
- **Vending Popup**: Leave empty (set at runtime automatically)
- **Wires**: 4 entries (all configured in step 4.1)
- **Target Points**: 4 entries (all configured in step 4.2)
- **Allow Multiple Connections**: `☐` (unchecked - one wire per target)

**Note:** There is no "Wire Game Panel" field - the script uses the GameObject it's attached to (WireGameContainer) automatically.

---

## STEP 5: Link Wire Game to ScrewPanelClosed

1. **Select root ScrewPanelClosed GameObject** in Hierarchy
2. In **ScrewPanelUI** component (this component is on ScrewPanelClosed):

### Wire Game References:
- **Wire Game**: Drag `WireGameContainer` GameObject from Hierarchy
- **Wire Game Container**: Drag `WireGameContainer` GameObject from Hierarchy

### Initial State:
- **Wire Game Container** should be **INACTIVE** initially (uncheck checkbox in Inspector)
- Wire game will activate when screws are removed

---

## STEP 6: Configure KeypadUI for Wire Check

1. **Open KeypadUI prefab**: `Assets/Prefabs/KeypadUI.prefab`
2. **Select root KeypadUI GameObject**
3. In **KeypadUI** component:

### Find All Keypad Buttons:

1. **Expand KeypadUI** in Hierarchy to see all child buttons
2. **In KeypadUI component**, find **Keypad Buttons** list
3. **Click +** to add entries
4. **Drag each button** from Hierarchy into the list:
   - Number buttons (0-9)
   - Any other interactive buttons
   - **Do NOT include CloseButton** (that should always work)

### Example:
- **Element 0**: Drag `Button0` (or whatever your number buttons are named)
- **Element 1**: Drag `Button1`
- **Element 2**: Drag `Button2`
- ... (continue for all buttons)

**Note:** If you can't find individual buttons, they might be part of the keypad sprite. In that case, you may need to create invisible Button components over the clickable areas.

---

## STEP 7: Test the System

### 7.1 Test in Scene

1. **Open your Prison scene** (or test scene)
2. **Play the game**
3. **Interact with vending machine** → Popup should appear
4. **Click PinScreen** → Keypad should open
5. **Try clicking keypad buttons** → Should be **disabled/grayed out** (wires not connected)
6. **Close keypad** (click X or close button)
7. **Click SidePanel** → Screw panel should open
8. **Unscrew both screws** → Panel should open, wire game should appear
9. **Click a wire** (left side) → Wire should highlight
10. **Click a target** (right side) → Wire should connect, target should turn wire color
11. **Connect all wires correctly** → Game should complete, return to vending popup
12. **Click PinScreen again** → Keypad should open
13. **Try clicking keypad buttons** → Should now be **enabled and work**!

### 7.2 Test State Persistence

1. **After connecting wires**, close everything
2. **Reopen vending machine**
3. **Click SidePanel** → Should show **opened panel** (screws already removed)
4. **Wire game should show** → Wires should still be connected
5. **Click PinScreen** → Keypad buttons should still work

---

## STEP 8: Customize Wire Colors and Layout

### Change Wire Colors:

1. **Select a wire Image** (e.g., `WireRed`)
2. In **Image** component, change **Color** to your desired color
3. **In WireConnectionGame component**, update the corresponding **Wire Color** to match

### Adjust Wire Positions:

1. **Select WiresLeft** or **TargetsRight** parent
2. Adjust **Pos X** to move left/right
3. **Select individual wires/targets** and adjust **Pos Y** for spacing

### Add More Wires:

1. Create additional wire and target Images
2. Add entries to **Wires** and **Target Points** lists in WireConnectionGame
3. Set **Correct Target Index** for each wire

---

## Troubleshooting

### Wire game doesn't appear after unscrewing:
- ✅ Check Wire Game Container is assigned in ScrewPanelUI component (on ScrewPanelClosed prefab)
- ✅ Check Wire Game Container starts as INACTIVE
- ✅ Check WireConnectionGame component is on WireGameContainer
- ✅ Check wire game is child of ScrewPanelClosed prefab

### Wires don't connect when clicked:
- ✅ Check wire Images have Button components (auto-added by script)
- ✅ Check target Images have Button components
- ✅ Check Raycast Target is enabled on wire and target Images
- ✅ Check CanvasGroup is not blocking raycasts

### Keypad buttons still don't work after connecting wires:
- ✅ Check KeypadUI has all buttons in Keypad Buttons list
- ✅ Check wire game reference is linked (should auto-link via VendingPopupInteractable)
- ✅ Check WireConnectionGame.IsComplete() returns true after connecting
- ✅ Check Console for debug messages

### Wires reset when reopening:
- ✅ Check WireConnectionGame component persists (not being destroyed)
- ✅ Check wasOpenedBefore flag in ScrewPanelUI component (should be true after first open)
- ✅ Check wire connections are stored in wireToTargetMap dictionary

### Wrong wire connects to target:
- ✅ Check Correct Target Index matches target position in Target Points list
- ✅ Check Target Points list order (0 = first, 1 = second, etc.)
- ✅ Verify wire colors match your intended connections

---

## File Locations

- **WireConnectionGame Script**: `Assets/Scripts/Vending/WireConnectionGame.cs`
- **ScrewPanelUI Script**: `Assets/Scripts/Vending/ScrewPanelUI.cs`
- **KeypadUI Script**: `Assets/Scripts/Vending/KeypadUI.cs`
- **ScrewPanelClosed Prefab**: `Assets/Prefabs/ScrewPanelClosed.prefab`
- **KeypadUI Prefab**: `Assets/Prefabs/KeypadUI.prefab`

---

## Quick Reference: Wire Connection Setup

```
ScrewPanelClosed (Prefab)
└── WireGameContainer (GameObject, WireConnectionGame component)
    ├── WiresLeft (Empty GameObject)
    │   ├── WireRed (Image, Button)
    │   ├── WireBlue (Image, Button)
    │   ├── WireYellow (Image, Button)
    │   └── WireGreen (Image, Button)
    └── TargetsRight (Empty GameObject)
        ├── Target0 (Image, Button)
        ├── Target1 (Image, Button)
        ├── Target2 (Image, Button)
        └── Target3 (Image, Button)
```

**WireConnectionGame Configuration:**
- Wire 0 (Red) → Correct Target Index: 0
- Wire 1 (Blue) → Correct Target Index: 1
- Wire 2 (Yellow) → Correct Target Index: 2
- Wire 3 (Green) → Correct Target Index: 3

---

## Notes

- **Wire colors** should be visually distinct
- **Target order** in list must match their visual order (left to right or top to bottom)
- **Correct Target Index** is 0-based (first target = 0, second = 1, etc.)
- Wire game **automatically adds Button components** if missing
- State **persists** - once wires are connected, they stay connected
- Screws **stay unscrewed** - once removed, they stay removed

---

**You're all set!** The wire connection game should now work perfectly with your vending machine system.

