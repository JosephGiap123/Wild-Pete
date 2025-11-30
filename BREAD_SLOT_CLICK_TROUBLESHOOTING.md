# Bread Slot Click Not Working - Troubleshooting Guide

## Quick Checks

### 1. **Check Console Messages**
When you start the game, look for these messages in the Console:
- `[VendingBreadSlotClick] Component initialized...`
- `[VendingBreadSlotClick] Image component found...`
- `[VendingBreadSlotClick] Canvas found...`

**If you see errors**, the script will tell you what's missing.

### 2. **Check if Mouse is Detected**
When you hover over the button area, you should see:
- `[VendingBreadSlotClick] MOUSE ENTERED`

**If you DON'T see this**, the button isn't receiving mouse events at all.

### 3. **Common Issues & Fixes**

#### Issue: Button is Behind Another UI Element
**Fix:**
- In Hierarchy, make sure `BreadSlotButton` is ABOVE other UI elements
- Or increase the Canvas Sort Order (Inspector → Canvas → Sort Order)

#### Issue: Button Has No Image Component
**Fix:**
- Select `BreadSlotButton` in Hierarchy
- Add Component → Image
- Make sure **Raycast Target** is checked ✅

#### Issue: Button is Outside Canvas Bounds
**Fix:**
- Check the RectTransform position
- Make sure the button is within the Canvas boundaries
- Check if RectTransform has negative width/height

#### Issue: Canvas Doesn't Have GraphicRaycaster
**Fix:**
- Select the Canvas GameObject
- Add Component → Graphic Raycaster (if missing)
- The script should auto-add this, but check manually

#### Issue: Button GameObject is Inactive
**Fix:**
- In Hierarchy, make sure `BreadSlotButton` has a checkmark ✅
- Check all parent GameObjects are also active

#### Issue: Button is Disabled
**Fix:**
- Select `BreadSlotButton`
- In Inspector, find the Button component
- Make sure **Interactable** is checked ✅

### 4. **Manual Test - Add Button Component Directly**

If nothing works, try using Unity's built-in Button:

1. Select `BreadSlotButton` in Hierarchy
2. Add Component → Button (if not already there)
3. In Inspector, find the Button component
4. Scroll down to **On Click ()**
5. Click the **+** button
6. Drag the GameObject with `VendingPopupInteractable` into the object field
7. Select `VendingPopupInteractable` → `OnBreadSlotClicked()`

This will bypass the `VendingBreadSlotClick` component entirely and use Unity's Button system.

### 5. **Check Button Position**

The button might be too small or in the wrong position:

1. Select `BreadSlotButton`
2. In Scene view, make sure you can see the button's outline
3. Check RectTransform:
   - Width and Height should be > 0
   - Position should be visible on screen
   - Anchors should be set correctly

### 6. **Check Event System**

Unity needs an EventSystem to handle UI clicks:

1. In Hierarchy, look for an "EventSystem" GameObject
2. If it's missing, create one:
   - Right-click in Hierarchy → UI → Event System
3. Make sure it's active ✅

## What to Report

If it STILL doesn't work, tell me:
1. What Console messages you see when starting the game
2. What happens when you hover over the button (do you see "MOUSE ENTERED"?)
3. Whether the button has an Image component
4. Whether the button is on a Canvas
5. Whether you tried the manual Button component test (step 4)

