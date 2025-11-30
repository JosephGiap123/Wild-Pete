# Bread Drop Setup - Quick Guide

When you complete the keypad game, bread will drop from the vending machine. Here's what you need to set up:

## PART 1: Assign Item Prefab

1. **Select the GameObject with `VendingPopupInteractable` component** (your vending machine)
2. In **Inspector**, find the **Bread Drop** section
3. **Item Prefab**: Drag your **`Item.prefab`** here
   - This is the same prefab used for other items in the game
   - Usually located in `Assets/Prefabs/Item.prefab` or similar

## PART 2: Set Bread Item Name

1. Still in **Inspector** on `VendingPopupInteractable`
2. **Bread Item Name**: Set to `"Bread"` (or whatever your Bread ItemSO is named)
   - **MUST match exactly** the `itemName` in your Bread ItemSO

## PART 3: Bread Drop Position (Optional)

1. **Bread Drop Position**: 
   - **Leave empty** = bread drops at the vending machine's position
   - **OR** create an empty GameObject where you want bread to drop, and drag it here

## PART 4: Make Sure Bread ItemSO Exists

The Bread ItemSO must be in one of these arrays on `PlayerInventory`:
- `itemSOs`
- `consumableSOs` 
- `equipmentSOs`

The `itemName` in the ItemSO must match what you set in **Bread Item Name**.

---

## That's It!

When you complete the keypad game:
1. ✅ All UI closes
2. ✅ Bread drops from vending machine
3. ✅ You can walk over and pick it up

## Test It

1. Complete the wire game
2. Complete the keypad game
3. Check Console for: `✅ Dropped bread at position: [position]`
4. Look for bread item in the world near the vending machine

If bread doesn't drop, check Console for error messages - they'll tell you what's missing!

