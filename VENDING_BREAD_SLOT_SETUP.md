# Vending Bread Slot Setup Guide

This guide explains how to add a clickable “open slot” area to the vending popup:
- Before puzzles are completed, the slot shows **empty**
- After puzzles (wires + keypad) are completed, the slot shows **bread**
- When you click the bread, it switches back to **empty** (later we can hook this to give an item)

---

## PART 1 – Create the sprites

You need **two sprites** for the slot:

1. **`slotEmptySprite`**
   - The open compartment with **no bread**
2. **`slotBreadSprite`**
   - The same open compartment but with **bread drawn in**

### 1.1 Import to Unity

1. Drag your PNGs into the **Project** window
2. Select each sprite in the Project window
3. In **Inspector**:
   - **Texture Type**: `Sprite (2D and UI)`
   - Click **Apply**

---

## PART 2 – Add the Bread Slot button to the vending popup

The bread slot is just a **Button with an Image** placed over the “open” area of the vending machine art.

### 2.1 Open the vending popup UI

1. Open the scene that has your **vending machine**
2. In **Hierarchy**, find and expand the **vending popup UI**:
   - This is the same GameObject that is assigned to `vendingPopup` on `VendingPopupInteractable`

### 2.2 Create the BreadSlotButton

1. Right‑click the **vending popup root** → **UI → Button**
2. Rename the button to: **`BreadSlotButton`**
3. Under `BreadSlotButton`, delete the Text child (you don’t need any text)

### 2.3 Configure the BreadSlotButton image

1. Select `BreadSlotButton` in **Hierarchy**
2. In **Inspector**, on the **Image** component:
   - Set **Source Image** = your **`slotEmptySprite`**
   - Set **Color** = white
3. Using the **RectTransform**:
   - Move and scale `BreadSlotButton` so the image **sits exactly over the open compartment** area on your vending machine art

Result: you now have a clickable area that visually shows the open slot.

---

## PART 3 – Add fields to `VendingPopupInteractable`

We’ll let `VendingPopupInteractable` control the bread slot.

### 3.1 Open the script

1. Open `Assets/Scripts/Vending/VendingPopupInteractabel.cs`
2. Make sure the top of the file has:

```csharp
using UnityEngine.UI;
```

### 3.2 Bread slot fields (already in script)

The `VendingPopupInteractable` script already has serialized fields for:
- `breadSlotImage`
- `breadSlotButton`
- `slotEmptySprite`
- `slotBreadSprite`

You **do not** need to add any code here — just assign them in the Inspector (see PART 4).

---

## PART 4 – Wire up references in the Inspector

1. In **Hierarchy**, select the **vending machine GameObject** that has `VendingPopupInteractable` on it
2. In **Inspector**, on the `VendingPopupInteractable` component:

- **Bread Slot Image**
  - Drag the **Image component** from `BreadSlotButton` into `breadSlotImage`
- **Bread Slot Button**
  - Drag the **BreadSlotButton GameObject** into `breadSlotButton`
- **Slot Empty Sprite**
  - Drag your `slotEmptySprite` asset into this field
- **Slot Bread Sprite**
  - Drag your `slotBreadSprite` asset into this field

3. **Save the scene**.

---

## PART 5 – Bread slot initialization (already handled)

`VendingPopupInteractable.Start()` is already set up so that:
- The bread slot overlay image (`breadSlotImage`) starts **hidden/closed**
- The bread slot button (`breadSlotButton`) is **clickable from the start**
- Internal state is initialized:
  - `hasBread = false` (no bread dispensed yet)
  - `isSlotOpen = false` (slot starts closed)

---

## PART 6 – When bread appears (after keypad puzzle)

When the keypad game is solved, your `KeypadUI` already calls `OnVendingMachineEmpty()` on `VendingPopupInteractable`.

The script logic there now:
- Swaps the **main vending sprite** to the “empty” machine art
- Sets an internal flag `hasBread = true`
- If the slot is currently open, it will switch the overlay to the **bread** sprite

So you don’t need to add any code for this — just know:
- **Before** keypad is solved: clicking the open area will open/close an **empty** slot
- **After** keypad is solved: the **next time** you open the slot, it will show **bread** (once)

---

## PART 7 – Handle bread click (switch back to empty)

For now, clicking on the bread will just visually remove it and lock the slot again.
Later, we can hook this to give a bread item to the player.

### 7.1 Bread claim behavior (already coded)

The click behavior for the open area is already implemented in `VendingPopupInteractable`:

**Before keypad complete:**
- First click:
  - Opens the slot (shows **empty** slot sprite)
- Second click:
  - Closes the slot (overlay hidden again)

**After keypad complete:**
- First click (slot closed):
  - Opens the slot and shows **bread** sprite
- Second click (slot open):
  - Treats this as “claim bread”:
    - Internally sets `hasBread = false` (so it won’t show again)
    - Closes the slot overlay
- Future clicks:
  - Behave like before puzzle: only open/close **empty** slot (no more bread)

---

## PART 8 – Test the setup (current behavior)

1. **Play the game**
2. Open the vending popup
3. Click the open-area button:
   - Slot overlay opens and shows the **empty** slot art
   - Click again → overlay closes
4. Complete the **wire game** and then the **keypad game** (enter correct code)
5. After “DISPENSING” and returning to the vending popup:
   - Click the open-area button:
     - Slot overlay opens and now shows **bread** art
   - Click again:
     - Bread is considered “claimed” (internally)
     - Slot overlay closes
6. Further clicks:
   - Slot opens/closes with **empty** art only (no more bread)

---

## Summary Checklist

- [ ] Created `slotEmptySprite` and `slotBreadSprite` and imported as Sprites
- [ ] Added `BreadSlotButton` inside the vending popup
- [ ] Positioned `BreadSlotButton` over the open slot area
- [ ] Wired `BreadSlotButton`’s Image and Button into those fields in the Inspector (`breadSlotImage`, `breadSlotButton`, `slotEmptySprite`, `slotBreadSprite`)
- [ ] Verified that clicking before keypad only opens/closes an empty slot
- [ ] Verified that after keypad, first open shows bread, second click removes it and closes slot

---

## PART 9 – (Optional) Inventory hookup

Right now, the bread claim is **visual only**:
- The bread appears once after the keypad puzzle
- When you click the open area twice (open + close), it disappears forever for that run

Later, if you want:
- We can update the `VendingPopupInteractable` script so that when bread is “claimed”, it:
  - Calls into your inventory system (e.g., `PlayerInventory.instance.AddItem("Bread", 1)`)
  - Optionally plays a sound or shows a message
