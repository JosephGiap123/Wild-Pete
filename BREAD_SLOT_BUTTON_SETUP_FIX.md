# Bread Slot Button Setup - CRITICAL FIX

## The Problem
The button isn't receiving ANY mouse events. This means Unity's event system can't detect it.

## The Solution - Use Unity's Button Component

The bread slot button should work EXACTLY like the PinScreen and SidePanel buttons. Here's how to set it up:

### STEP 1: Check Your BreadSlotButton GameObject

1. **Select `BreadSlotButton` in Hierarchy**
2. **Check these components exist:**

#### Image Component (REQUIRED)
- **Source Image**: Can be `None` (transparent)
- **Color**: `(255, 255, 255, 0)` or `(255, 255, 255, 10)` - Nearly transparent
- **Raycast Target**: ✅ **MUST BE CHECKED** - This is CRITICAL!

#### Button Component (REQUIRED)
- **Interactable**: ✅ **MUST BE CHECKED**
- **Transition**: `Color Tint` (or `None`)
- **Target Graphic**: Drag the **Image** component here

#### VendingBreadSlotClick Component
- **Parent**: Drag the GameObject with `VendingPopupInteractable` here

### STEP 2: Check Canvas Setup

1. **Find the Canvas** that `BreadSlotButton` is on
2. **Check Canvas has:**
   - **Graphic Raycaster** component ✅
   - **Canvas** component with **Render Mode**: `Screen Space - Overlay` or `Screen Space - Camera`

### STEP 3: Check Event System

1. **In Hierarchy, look for "EventSystem" GameObject**
2. **If missing**, create one:
   - Right-click in Hierarchy → **UI** → **Event System**
3. **Make sure it's active** ✅

### STEP 4: Check Button Position & Size

1. **Select `BreadSlotButton`**
2. **In Scene view**, make sure you can see the button's outline
3. **Check RectTransform:**
   - **Width** and **Height** should be > 0 (e.g., 100x100)
   - **Position** should be visible on screen
   - **Scale** should be `(1, 1, 1)`

### STEP 5: Check if Button is Behind Other UI

1. **In Hierarchy**, check the order of UI elements
2. **Elements LOWER in the list render BEHIND**
3. **Elements HIGHER in the list render IN FRONT**
4. **Make sure `BreadSlotButton` is ABOVE other UI elements** that might block it

**OR** increase the Canvas Sort Order:
- Select the Canvas
- In Inspector, find **Canvas** component
- Increase **Sort Order** (e.g., from 0 to 10)

### STEP 6: Manual Button Test

If nothing works, try wiring the button manually:

1. **Select `BreadSlotButton`**
2. **Find the Button component** in Inspector
3. **Scroll down to "On Click ()"**
4. **Click the + button** to add an event
5. **Drag the GameObject with `VendingPopupInteractable`** into the object field
6. **Click the dropdown** and select: `VendingPopupInteractable` → `OnBreadSlotClicked()`

This bypasses the `VendingBreadSlotClick` script entirely and uses Unity's built-in Button system.

### STEP 7: Debug Checklist

Run the game and check Console for these messages:

✅ **You SHOULD see:**
- `[VendingBreadSlotClick] Component initialized...`
- `[VendingBreadSlotClick] Button wired...`
- `[VendingPopupInteractable] Start() - breadSlotButton found...`

❌ **If you see errors:**
- `No Image component found!` → Add Image component
- `No Button component found!` → Add Button component
- `No Canvas found!` → Button must be on a Canvas
- `Could not find VendingPopupInteractable!` → Assign Parent field

### Most Common Issues:

1. **Image Raycast Target is OFF** → Turn it ON ✅
2. **Button is behind another UI element** → Move it up in Hierarchy or increase Canvas Sort Order
3. **Button has 0 width/height** → Set RectTransform size to something like 100x100
4. **No EventSystem in scene** → Create one (UI → Event System)
5. **Canvas doesn't have GraphicRaycaster** → Add it

## Quick Fix - Copy PinScreen Setup

The easiest way is to **copy the exact setup from PinScreen**:

1. **Select `PinScreen`** (the one that works)
2. **Note its components:**
   - Image (with Raycast Target ✅)
   - Button (with Interactable ✅)
   - VendingKeypadClick
3. **Select `BreadSlotButton`**
4. **Add the same components** with the same settings
5. **Replace `VendingKeypadClick` with `VendingBreadSlotClick`**

