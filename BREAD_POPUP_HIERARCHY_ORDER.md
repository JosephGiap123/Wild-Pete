# Bread Popup Hierarchy Order - Visual Guide

## The Problem
If the bread button is **above** the slot image in the Hierarchy, it will render **on top** and cover the sprite change, making it look like nothing happened.

## What "Lower in the List" Means

In Unity's Hierarchy window, items **higher up** in the list render **behind** items **lower down**. Think of it like layers in Photoshop - items at the bottom of the list are drawn last, so they appear on top.

### ❌ WRONG Order (Button covers Image):
```
BreadPopup
  ├── SlotImage          ← Renders FIRST (behind)
  └── BreadButton        ← Renders LAST (on top, covers SlotImage)
```

### ✅ CORRECT Order (Image visible):
```
BreadPopup
  ├── BreadButton        ← Renders FIRST (behind)
  └── SlotImage          ← Renders LAST (on top, visible)
```

## How to Fix in Unity

### Step 1: Open Your BreadPopup Prefab
1. In **Project** window, find your `BreadPopup.prefab`
2. **Double-click** it to open in Prefab mode (or click the arrow ▶ next to it)

### Step 2: Check Current Order
1. Look at the **Hierarchy** window (left side)
2. You should see something like:
   ```
   BreadPopup
     ├── SlotImage
     ├── BreadButton
     └── CloseButton (optional)
   ```

### Step 3: Reorder the Items
1. **Click and drag** `BreadButton` in the Hierarchy
2. **Move it ABOVE** `SlotImage` (higher in the list)
3. The order should now be:
   ```
   BreadPopup
     ├── BreadButton     ← Moved here (higher in list)
     ├── SlotImage       ← Should be below BreadButton
     └── CloseButton
   ```

### Visual Example:

**BEFORE (Wrong):**
```
Hierarchy Window:
┌─────────────────────┐
│ BreadPopup          │
│   ├─ SlotImage      │ ← Click and drag this
│   └─ BreadButton    │ ← Or drag this up
└─────────────────────┘
```

**AFTER (Correct):**
```
Hierarchy Window:
┌─────────────────────┐
│ BreadPopup          │
│   ├─ BreadButton    │ ← Now on top (renders first, behind)
│   └─ SlotImage      │ ← Now below (renders last, visible)
└─────────────────────┘
```

## Alternative: Make BreadButton Transparent

If you can't reorder, you can make the bread button's Image component fully transparent:

1. Select `BreadButton` in Hierarchy
2. In **Inspector**, find the **Image** component
3. Set **Color** → **Alpha** to `0` (fully transparent)
4. Make sure **Raycast Target** is still ✅ checked (for clicks to work)

## Test

After reordering:
1. **Save** the prefab (Ctrl+S)
2. **Play** the game
3. Click the bread
4. The sprite should now switch to empty!

## Still Not Working?

Check the Console for these messages:
- `[BreadPopupUI] SwitchToEmptySlotCoroutine started` - Coroutine is running
- `[BreadPopupUI] SwitchToEmptySlot() called` - Method is being called
- `[BreadPopupUI] BEFORE switch - Current sprite: [name]` - Shows current sprite
- `[BreadPopupUI] AFTER switch - Image sprite: [name]` - Shows new sprite

If you don't see these messages, the coroutine might not be starting. Check for errors in the Console.

