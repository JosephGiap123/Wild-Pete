# Empty Vending Machine Sprite Setup Guide

This guide will help you set up the empty vending machine sprite that appears after completing the keypad game.

## Step 1: Prepare Your Empty Sprite

1. Make sure you have an **empty vending machine sprite** ready (the same vending machine but without the bread/item)
2. Import it into Unity if you haven't already
3. Make sure it's set up as a **Sprite** (not Texture2D) in the Import Settings

## Step 2: Find the VendingPopupInteractable Component

1. In your Unity scene, find the GameObject that has the **VendingPopupInteractable** component
   - This is usually on the vending machine GameObject in the world
   - Or it might be in a manager GameObject

2. Select that GameObject in the Hierarchy

## Step 3: Assign the Empty Sprite

1. In the Inspector, look for the **VendingPopupInteractable** component
2. Find the **"Vending Machine Sprites"** section (it should be near the top)
3. You'll see a field called **"Empty Vending Sprite"**
4. **Drag your empty vending machine sprite** from the Project window into this field
   - OR click the circle icon next to the field and select it from the picker

## Step 4: Verify the Setup

1. Make sure the **vendingPopup** field is still assigned (the GameObject that shows the vending machine UI)
2. Make sure the **emptyVendingSprite** field now has your sprite assigned

## Step 5: Test It

1. Play the game
2. Complete the wire connection game
3. Complete the keypad game (enter the correct code)
4. After "DISPENSING" shows for 3 seconds, the vending popup should return
5. **The vending machine should now show the empty sprite** instead of the original one

## Troubleshooting

### If the sprite doesn't change:

1. **Check the Console** for any error messages
   - Look for messages like "No Image component found" or "Empty vending sprite not assigned"

2. **Verify the vendingPopup GameObject has an Image component:**
   - Select the `vendingPopup` GameObject in the Hierarchy
   - Check if it has an **Image** component in the Inspector
   - If not, add one: Component → UI → Image

3. **Check the sprite assignment:**
   - Go back to VendingPopupInteractable
   - Make sure the "Empty Vending Sprite" field is not empty
   - Make sure you're using a Sprite, not a Texture2D

4. **Check the Image component:**
   - The Image component on `vendingPopup` should have a sprite assigned
   - This is the sprite that will be replaced with the empty one

## What Happens When It Works:

1. Player completes wire game → wires connected
2. Player completes keypad game → enters correct code
3. Keypad shows "DISPENSING" for 3 seconds
4. Keypad closes and returns to vending popup
5. **Vending popup now shows empty sprite** (bread/item is gone)

## Notes:

- The empty sprite should be the **same size and position** as the original sprite for best results
- The sprite change happens automatically when the code is correct
- The change is permanent for that game session (the sprite stays empty)

