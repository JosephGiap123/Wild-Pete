# Drag-and-Drop Wire Connection Game Setup Guide

This guide will help you set up the drag-and-drop wire connection game that appears after unscrewing the side panel.

---

## Quick Overview

**How it works:**
- Wires appear on the **left side**
- Targets appear on the **right side**
- **Drag** a wire from left to a target on the right
- If you drop it close enough (within snap distance), it connects
- A **line visual** appears showing the connection
- Wire returns to left side after connecting (but connection stays)

---

## STEP 1: Set Up Wire Game Container in ScrewPanelClosed Prefab

### 1.1 Open ScrewPanelClosed Prefab

1. Navigate to `Assets/Prefabs/ScrewPanelClosed.prefab` in Project window
2. **Double-click** to open in Prefab Mode

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

#### CanvasGroup Component:
- **Add Component** → **Canvas Group**
- **Alpha**: `1`
- **Interactable**: `☑` (checked)
- **Blocks Raycasts**: `☑` (checked)

#### WireConnectionGame Component:
- **Add Component** → Search for `WireConnectionGame`
- **Canvas Group**: Drag `WireGameContainer` GameObject here (auto-assigns)
- **Vending Popup**: Leave empty (set at runtime)
- **Wires**: Leave empty (we'll add in step 2)
- **Target Points**: Leave empty (we'll add in step 3)
- **Wire Line Prefab**: Leave empty (optional - will create lines automatically)
- **Drag Snap Distance**: `50` (pixels - how close to target to connect)

---

## STEP 2: Create Wire Images (Left Side - Draggable)

These are the wires you drag to connect.

### 2.1 Create Wires Container

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

For each wire (example: 4 wires - Red, Blue, Yellow, Green):

#### Wire 1 (Red):
1. **Right-click WiresLeft** → **UI** → **Image**
2. **Rename** to: `WireRed`
3. **Select WireRed** and configure:

##### RectTransform:
- **Anchor Presets**: Click **left-center**
- **Pos X**: `0`
- **Pos Y**: `60` (first wire - adjust spacing)
- **Pos Z**: `0`
- **Width**: `40` (wire width)
- **Height**: `30` (wire height)
- **Scale**: `(1, 1, 1)`

##### Image Component:
- **Source Image**: Your red wire sprite
- **Color**: `Red` (255, 0, 0, 255)
- **Raycast Target**: `☑` (checked - REQUIRED for dragging!)

**Important:** The `WireDragHandler` component will be **automatically added** by the script - you don't need to add it manually!

#### Wire 2, 3, 4 (Blue, Yellow, Green):
- Create 3 more wires with same setup
- **Pos Y**: `20`, `-20`, `-60` (adjust spacing)
- **Colors**: Blue (0, 0, 255), Yellow (255, 255, 0), Green (0, 255, 0)
- **Rename**: `WireBlue`, `WireYellow`, `WireGreen`

---

## STEP 3: Create Target Points (Right Side)

These are where wires connect to.

### 3.1 Create Targets Container

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

Create targets in the **same order** as wires:

#### Target 0 (First target):
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

#### Target 1, 2, 3:
- Create 3 more targets
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
- **Wire Color**: Click color picker → Select **Red** (255, 0, 0, 255)
- **Correct Target Index**: `0` (connects to Target0)

#### Wire 1 (Blue):
- **Wire Image**: Drag `WireBlue` from Hierarchy
- **Wire Color**: **Blue** (0, 0, 255, 255)
- **Correct Target Index**: `1` (connects to Target1)

#### Wire 2 (Yellow):
- **Wire Image**: Drag `WireYellow` from Hierarchy
- **Wire Color**: **Yellow** (255, 255, 0, 255)
- **Correct Target Index**: `2` (connects to Target2)

#### Wire 3 (Green):
- **Wire Image**: Drag `WireGreen` from Hierarchy
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

### 4.3 Final Settings

- **Canvas Group**: Should be auto-assigned
- **Vending Popup**: Leave empty
- **Wires**: 4 entries (all configured)
- **Target Points**: 4 entries (all configured)
- **Wire Line Prefab**: Leave empty (lines created automatically)
- **Drag Snap Distance**: `50` (adjust if needed - pixels from target to connect)

---

## STEP 5: Link Wire Game to ScrewPanelClosed

1. **Select root ScrewPanelClosed GameObject** in Hierarchy
2. In **ScrewPanelUI** component:

### Wire Game References:
- **Wire Game**: Drag `WireGameContainer` GameObject from Hierarchy
- **Wire Game Container**: Drag `WireGameContainer` GameObject from Hierarchy

### Initial State:
- **Wire Game Container** should be **INACTIVE** initially (uncheck checkbox in Inspector)
- Wire game will activate when screws are removed

---

## STEP 6: Adjust Screw Panel Positioning

### 6.1 Find Where Panel is Positioned

The screw panel positioning is controlled in **VendingPopupInteractable.cs**:

1. **Open your scene** (Prison scene or test scene)
2. **Select VendingMachine GameObject** in Hierarchy
3. In **VendingPopupInteractable** component, find the **OpenScrewPanel()** method settings

### 6.2 Adjust Panel Position

The panel is positioned in `VendingPopupInteractable.OpenScrewPanel()`:

**Current settings:**
- **Anchor**: Center-center (0.5, 0.5)
- **Position**: (0, 0, 0)
- **Scale**: (1, 1, 1)

**To adjust:**

1. **Option A: Adjust in Code** (if you want to change default):
   - Open `Assets/Scripts/Vending/VendingPopupInteractabel.cs`
   - Find `OpenScrewPanel()` method (around line 82)
   - Change `rt.anchoredPosition = Vector2.zero;` to your desired position
   - Example: `rt.anchoredPosition = new Vector2(100, 50);` (moves right 100px, up 50px)

2. **Option B: Adjust in Scene** (temporary - will reset):
   - When panel is open in Play mode, select the **ScrewPanelClosed instance** in Hierarchy
   - Adjust **RectTransform** position in Inspector
   - **Note:** This won't persist - you need to adjust the prefab or code

3. **Option C: Adjust Prefab** (recommended):
   - Open `Assets/Prefabs/ScrewPanelClosed.prefab`
   - Select root GameObject
   - Adjust **RectTransform**:
     - **Anchor Presets**: Choose your anchor (e.g., center-center, top-left, etc.)
     - **Pos X**: Horizontal position
     - **Pos Y**: Vertical position
     - **Width/Height**: Size of panel

### 6.3 Adjust Panel Size

In `OpenScrewPanel()` method, you can also set a fixed size:

```csharp
// Add after rt.anchoredPosition = Vector2.zero;
float panelWidth = 400f;  // Adjust to your needs
float panelHeight = 300f; // Adjust to your needs
rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
```

---

## STEP 7: Test the Drag-and-Drop System

### 7.1 Test in Scene

1. **Play the game**
2. **Interact with vending machine** → Popup appears
3. **Click SidePanel** → Screw panel opens
4. **Unscrew both screws** → Panel opens, wire game appears
5. **Click and drag a wire** (left side) → Wire should follow cursor
6. **Drag wire over a target** (right side) → Wire should be draggable
7. **Drop wire near target** (within 50 pixels) → Wire connects, line appears, target changes color
8. **Wire returns to left side** → But connection stays (line visible)
9. **Connect all wires correctly** → Game completes, returns to vending popup

### 7.2 Test State Persistence

1. **After connecting wires**, close everything
2. **Reopen vending machine**
3. **Click SidePanel** → Should show opened panel with wires still connected
4. **Lines should still be visible** showing connections

---

## Troubleshooting

### Wires don't drag:
- ✅ Check wire Images have **Raycast Target** enabled
- ✅ Check **WireDragHandler** component was added (should be automatic)
- ✅ Check Canvas has **GraphicRaycaster** component
- ✅ Check CanvasGroup is not blocking raycasts

### Wires don't connect when dropped:
- ✅ Check **Drag Snap Distance** is large enough (try 100)
- ✅ Check target positions are correct
- ✅ Check wire and target are both active and visible

### Lines don't appear:
- ✅ Lines are created automatically when wire connects
- ✅ Check wire and target positions are valid
- ✅ Check line GameObject was created (look in Hierarchy when connected)

### Panel positioning is wrong:
- ✅ Adjust **RectTransform** in ScrewPanelClosed prefab
- ✅ Or modify `OpenScrewPanel()` method in VendingPopupInteractable
- ✅ Check anchor presets are correct

### Wire returns to wrong position:
- ✅ WireDragHandler stores original position in `Awake()`
- ✅ Make sure wire position is set before game starts
- ✅ Check wire is child of correct parent

---

## File Locations

- **WireConnectionGame Script**: `Assets/Scripts/Vending/WireConnectionGame.cs`
- **WireDragHandler Script**: `Assets/Scripts/Vending/WireDragHandler.cs`
- **ScrewPanelUI Script**: `Assets/Scripts/Vending/ScrewPanelUI.cs`
- **VendingPopupInteractable Script**: `Assets/Scripts/Vending/VendingPopupInteractabel.cs`
- **ScrewPanelClosed Prefab**: `Assets/Prefabs/ScrewPanelClosed.prefab`

---

## Quick Reference: Positioning Screw Panel

**To adjust panel position when it opens:**

1. **Open**: `Assets/Scripts/Vending/VendingPopupInteractabel.cs`
2. **Find**: `OpenScrewPanel()` method (around line 82)
3. **Look for**:
   ```csharp
   rt.anchoredPosition = Vector2.zero;
   ```
4. **Change to** (example):
   ```csharp
   rt.anchoredPosition = new Vector2(100, -50); // Right 100px, Down 50px
   ```

**Or adjust in prefab:**
1. Open `Assets/Prefabs/ScrewPanelClosed.prefab`
2. Select root GameObject
3. Adjust RectTransform position/size

---

**You're all set!** The drag-and-drop wire game should now work perfectly.

