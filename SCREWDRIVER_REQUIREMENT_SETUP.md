# Screwdriver Requirement Setup Guide

This guide will help you set up the requirement system where players need a screwdriver in their inventory to open the screw panel.

## Step 1: Create the Screwdriver Item (ItemSO)

1. In Unity, go to your **Project** window
2. Navigate to where your other ItemSO ScriptableObjects are stored (usually in `Assets/ScriptableObjects/Items/` or similar)
3. **Right-click** → **Create** → Find your **ItemSO** script (or whatever you named your item scriptable object)
4. Name it **"Screwdriver"** (or "ScrewdriverSO")
5. In the Inspector, set:
   - **Item Name**: `"Screwdriver"` (exact spelling matters - must match what we check in code)
   - **Icon**: Assign your screwdriver sprite/icon
   - **Max Stack Size**: `1` (usually tools don't stack)
   - **Quantity**: `1`
   - **Item Description**: Add a description like "A tool for removing screws"

## Step 2: Update VendingPopupInteractable Script

The script has been updated to check for the screwdriver. Here's what was added:

- **`screwdriverItemName`** field: Set this to `"Screwdriver"` in the Inspector
- **Check in `OpenScrewPanel()`**: Before opening the panel, it checks if player has the screwdriver
- **Error message**: Shows a debug message if screwdriver is missing

## Step 3: Configure in Unity Inspector

1. Select the GameObject that has the **VendingPopupInteractable** component
2. In the Inspector, find the **VendingPopupInteractable** component
3. Look for the **"Screwdriver Requirement"** section (newly added)
4. Set **"Screwdriver Item Name"** to: `Screwdriver` (must match the ItemSO name exactly)

## Step 4: Test It

1. **Without screwdriver**: Try to open the screw panel - it should NOT open and show a message in console
2. **With screwdriver**: Add screwdriver to inventory, then try to open - it should work normally

## Step 5: Optional - Add Visual Feedback

If you want to show a message to the player (instead of just console), you can:
- Create a UI text element for error messages
- Assign it to the `VendingPopupInteractable` component
- The script will show a message when screwdriver is missing

## How It Works

- When player clicks to open the screw panel, the script checks:
  ```csharp
  if (PlayerInventory.instance.HasItem("Screwdriver") <= 0)
  {
      // Don't open panel, show message
      return;
  }
  ```
- If screwdriver is found (count > 0), the panel opens normally
- If not found, the panel stays closed and a message is logged

## Troubleshooting

- **Panel still opens without screwdriver**: 
  - Check that "Screwdriver Item Name" in Inspector matches your ItemSO name exactly (case-sensitive)
  - Check Console for debug messages
  
- **Can't find PlayerInventory.instance**:
  - Make sure PlayerInventory is in the scene
  - Make sure it has the singleton instance set up correctly

- **Item name not matching**:
  - The item name must match EXACTLY (case-sensitive)
  - Check your ItemSO's "Item Name" field
  - Check the "Screwdriver Item Name" field in VendingPopupInteractable

