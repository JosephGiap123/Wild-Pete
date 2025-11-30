# Bread Popup Overlay Setup Guide

The bread popup now **overlays** the vending machine popup in the "open" area, instead of replacing it.

---

## PART 1: Create the Bread Popup Prefab (Smaller Overlay)

### 1.1 Create the Popup GameObject

1. In your scene, find the **MiniGameCanvas**
2. Right-click **MiniGameCanvas** → **UI** → **Panel**
3. Rename to: **`BreadPopup`**

### 1.2 Configure the Panel (Smaller Size)

1. Select **BreadPopup**
2. In **RectTransform**:
   - **Anchor Presets**: Click **top-left** (or wherever your "open" area is)
   - **Pos X**: `0` (we'll adjust this)
   - **Pos Y**: `0` (we'll adjust this)
   - **Width**: `200` (or size of your open area)
   - **Height**: `200` (or size of your open area)
   - **Scale**: `(1, 1, 1)`

3. In **Image** component:
   - **Source Image**: `None` (transparent background)
   - **Color**: `(255, 255, 255, 0)` - Fully transparent

### 1.3 Add the Slot Image

1. Right-click **BreadPopup** → **UI** → **Image**
2. Rename to: **`SlotImage`**
3. Configure:
   - **RectTransform**: 
     - **Anchor Presets**: **stretch-stretch** (hold Alt+Shift)
     - **Left, Top, Right, Bottom**: All `0` (fills the popup)
   - **Image** → **Source Image**: Your `emptySlotSprite`
   - **Image** → **Color**: White

### 1.4 Add the Bread Button (for claiming bread)

1. Right-click **BreadPopup** → **UI** → **Button**
2. Rename to: **`BreadButton`**
3. Configure:
   - **RectTransform**: 
     - **Anchor Presets**: **stretch-stretch** (hold Alt+Shift)
     - **Left, Top, Right, Bottom**: All `0` (fills the popup)
   - **Image** → **Source Image**: `None` (transparent)
   - **Image** → **Color**: `(255, 255, 255, 0)` - Fully transparent
   - **Image** → **Raycast Target**: ✅ **CHECKED**
   - **Button** → **Interactable**: ✅ **CHECKED**
4. **Delete the Text child**

### 1.5 Add the Close Button (Optional)

1. Right-click **BreadPopup** → **UI** → **Button**
2. Rename to: **`CloseButton`**
3. Configure:
   - **RectTransform**: Small button in corner
   - **Image** → **Source Image**: Your X/close sprite
   - **Button** → **Interactable**: ✅ **CHECKED**
4. Add a **Text** child showing "X" (or leave as Image)

**Note**: Close button is optional. The popup will auto-close after 3 seconds if empty, or when bread is claimed.

### 1.6 Add the BreadPopupUI Script

1. Select **BreadPopup**
2. **Add Component** → **BreadPopupUI**
3. In Inspector, assign:
   - **Slot Image**: Drag `SlotImage` here
   - **Bread Button**: Drag `BreadButton` here
   - **Close Button**: Drag `CloseButton` here (or leave `None`)
   - **Empty Slot Sprite**: Drag your empty slot sprite asset
   - **Bread Slot Sprite**: Drag your bread slot sprite asset
   - **Empty Auto Close Delay**: `3` (seconds)
   - **Use Auto Position**: `false` (we'll position manually)
   - **Overlay Offset**: `(0, 0)` (not used if auto-position is off)

### 1.7 Position the Popup to Overlay the "Open" Area

**IMPORTANT**: You need to position this popup to match where the "open" area is on your vending machine art.

1. **Temporarily make the vending popup visible** in the scene
2. **Select `BreadPopup`**
3. **In Scene view**, move and resize `BreadPopup` so it **exactly overlays** the "open" area on your vending machine art
4. **Note the RectTransform values** (Pos X, Pos Y, Width, Height)
5. **Set these values** in the Inspector

**OR** use anchors:
- Set **Anchor Presets** to match the position of your "open" area
- For example, if the open area is in the bottom-right, use **bottom-right** anchor

### 1.8 Create the Prefab

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
   - Position and size to **exactly match** the "open" area on your vending machine art
   - This should be the **same position and size** as your `BreadPopup` prefab

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

## PART 4: Positioning Tips

### Method 1: Manual Positioning (Recommended)

1. **Open your `BreadPopup.prefab`** in Prefab mode
2. **Set the RectTransform** to match your "open" area:
   - Use **anchors** to position relative to the vending popup
   - Or use **absolute positions** if you know the exact coordinates
3. **Save the prefab**

### Method 2: Use Anchors

If your "open" area is always in the same relative position:

1. **Set Anchor Presets** to match the position (e.g., bottom-right)
2. **Set Pivot** to match (e.g., bottom-right)
3. **Set Pos X and Pos Y** to offset from the anchor point

### Method 3: Match Click Area Position

1. **Position `BreadSlotClickArea`** first (easier to see in Scene view)
2. **Note its RectTransform values**
3. **Apply the same values** to `BreadPopup` prefab

---

## PART 5: Test

1. **Play the game**
2. **Interact with the vending machine** (press E)
3. **Vending popup shows** (stays visible)
4. **Click the open area** → Bread popup **overlays** on top showing **empty slot**
5. **Wait 3 seconds** → Popup should auto-close (vending popup stays visible)
6. **Complete the wire game and keypad puzzle**
7. **Click the open area again** → Bread popup **overlays** showing **bread**
8. **Click the bread** → Should add to inventory and close popup (vending popup stays visible)
9. **Click the X button** (if you added one) → Should close popup (vending popup stays visible)

---

## Troubleshooting

### Popup is in the wrong position
- Check the `BreadPopup` prefab's **RectTransform** values
- Make sure it matches the position of your "open" area
- Try using **anchors** instead of absolute positions

### Popup is too big/small
- Adjust **Width** and **Height** in the `BreadPopup` prefab's **RectTransform**
- Should match the size of your "open" area

### Popup doesn't overlay correctly
- Make sure `BreadPopup` is a **child of MiniGameCanvas** (same as vending popup)
- Check that `BreadPopup` has a **higher sibling index** than vending popup (renders on top)
- The code calls `SetAsLastSibling()` to bring it to front

### Vending popup disappears
- This shouldn't happen anymore - the code no longer hides the vending popup
- If it does, check that `vendingPopup.SetActive(false)` is not being called elsewhere

