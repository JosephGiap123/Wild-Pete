# Bread Popup Setup Guide - Fresh Start

This guide shows how to set up the bread slot as a **separate popup** (like KeypadUI or ScrewPanelUI).

## How It Works

1. **Click the open area** on the vending machine popup
2. **Bread popup opens** showing:
   - **Empty slot** (before puzzles complete) → auto-closes after 3 seconds
   - **Bread** (after keypad puzzle complete) → can click to claim
3. **Click bread** → adds to inventory and closes popup
4. **Close button (X)** → closes popup anytime

---

## PART 1: Create the Bread Popup Prefab

### 1.1 Create the Popup GameObject

1. In your scene, find the **MiniGameCanvas**
2. Right-click **MiniGameCanvas** → **UI** → **Panel**
3. Rename to: **`BreadPopup`**

### 1.2 Configure the Panel

1. Select **BreadPopup**
2. In **RectTransform**:
   - **Anchor Presets**: Hold `Alt+Shift` and click **center-center**
   - **Pos X**: `0`
   - **Pos Y**: `0`
   - **Width**: `800` (or your desired size)
   - **Height**: `600` (or your desired size)
   - **Scale**: `(1, 1, 1)`

3. In **Image** component:
   - **Source Image**: Your bread popup background sprite (or leave `None` for transparent)
   - **Color**: White or transparent

### 1.3 Add the Slot Image

1. Right-click **BreadPopup** → **UI** → **Image**
2. Rename to: **`SlotImage`**
3. Configure:
   - **RectTransform**: Position and size to show the slot area
   - **Image** → **Source Image**: Your `emptySlotSprite` (we'll change this in code)
   - **Image** → **Color**: White

### 1.4 Add the Bread Button (for claiming bread)

1. Right-click **BreadPopup** → **UI** → **Button**
2. Rename to: **`BreadButton`**
3. Configure:
   - **RectTransform**: Position and size to cover the bread area (when bread is shown)
   - **Image** → **Source Image**: `None` (transparent, or your bread sprite)
   - **Image** → **Color**: `(255, 255, 255, 0)` - Fully transparent
   - **Image** → **Raycast Target**: ✅ **CHECKED**
   - **Button** → **Interactable**: ✅ **CHECKED**
4. **Delete the Text child** (you don't need text)

### 1.5 Add the Close Button

1. Right-click **BreadPopup** → **UI** → **Button**
2. Rename to: **`CloseButton`**
3. Configure:
   - **RectTransform**: Position in top-right corner (or wherever you want the X button)
   - **Image** → **Source Image**: Your X/close button sprite
   - **Button** → **Interactable**: ✅ **CHECKED**
4. Add a **Text** child (or Image) showing "X" or close icon

### 1.6 Add the BreadPopupUI Script

1. Select **BreadPopup**
2. **Add Component** → **BreadPopupUI**
3. In Inspector, assign:
   - **Slot Image**: Drag `SlotImage` here
   - **Bread Button**: Drag `BreadButton` here
   - **Close Button**: Drag `CloseButton` here
   - **Empty Slot Sprite**: Drag your empty slot sprite asset
   - **Bread Slot Sprite**: Drag your bread slot sprite asset
   - **Empty Auto Close Delay**: `3` (seconds)

### 1.7 Create the Prefab

1. **Drag `BreadPopup`** from Hierarchy to **Project** window (into a Prefabs folder)
2. This creates **`BreadPopup.prefab`**
3. **Delete `BreadPopup`** from the scene (we'll instantiate it from the prefab)

---

## PART 2: Create the Click Area on Vending Popup

### 2.1 Add Click Area to Vending Popup

1. Find your **VendingPopup** GameObject in Hierarchy
2. Right-click **VendingPopup** → **UI** → **Image**
3. Rename to: **`BreadSlotClickArea`**

### 2.2 Configure the Click Area

1. Select **BreadSlotClickArea**
2. In **RectTransform**:
   - Position and size to cover the "open" area on your vending machine art
   - This is where players will click to open the bread popup

3. In **Image** component:
   - **Source Image**: `None` (leave empty)
   - **Color**: `(255, 255, 255, 0)` - **Fully transparent**
   - **Raycast Target**: ✅ **MUST BE CHECKED** (for clicks to work)

### 2.3 Add VendingBreadSlotClick Component

1. Select **BreadSlotClickArea**
2. **Add Component** → **VendingBreadSlotClick**
3. In Inspector:
   - **Parent**: Drag the GameObject with `VendingPopupInteractable` component here

---

## PART 3: Wire Up in VendingPopupInteractable

1. Select the GameObject with **VendingPopupInteractable** component
2. In Inspector, find the **Bread Slot Popup** section:
   - **Bread Popup Prefab**: Drag your **`BreadPopup.prefab`** here
   - **Bread Item Name**: `Bread` (must match your ItemSO name)

---

## PART 4: Test

1. **Play the game**
2. **Interact with the vending machine** (press E)
3. **Click the open area** → Bread popup should open showing **empty slot**
4. **Wait 3 seconds** → Popup should auto-close
5. **Complete the wire game and keypad puzzle**
6. **Click the open area again** → Bread popup should show **bread**
7. **Click the bread** → Should add to inventory and close popup
8. **Click the X button** → Should close popup anytime

---

## Troubleshooting

### Popup doesn't open when clicking
- Check `BreadSlotClickArea` has **Image** with **Raycast Target** ✅
- Check `VendingBreadSlotClick` has **Parent** assigned
- Check Console for errors

### Empty slot doesn't auto-close
- Check `BreadPopupUI` has **Empty Auto Close Delay** set to `3`
- Check Console for errors

### Bread doesn't appear after completing puzzles
- Check `hasBread` is set to `true` in `OnVendingMachineEmpty()`
- Check `BreadPopupUI.Initialize()` is being called with `breadAvailable = true`

### Bread doesn't add to inventory
- Check `breadItemName` matches your ItemSO name exactly
- Check `PlayerInventory.instance` exists
- Check Console for errors

