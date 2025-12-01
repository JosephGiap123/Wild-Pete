# How to Make ONLY Screwdriver Icon Smaller in Inventory

## Problem
The screwdriver icon appears too large in the inventory slot. You want to make ONLY the screwdriver smaller, not all items.

## Solution
The code has been modified to automatically scale down the screwdriver icon. The ItemSlot script now checks if the item is a "Screwdriver" and scales it down to 70% size. You can adjust this value if needed.

---

## ✅ SOLUTION IMPLEMENTED

The `ItemSlot.cs` script has been modified to automatically scale down the screwdriver icon. **No manual steps needed!**

### How It Works
- When the inventory displays an item, it checks if the item name is "Screwdriver"
- If it is, the icon is scaled down to **70%** of its normal size
- All other items remain at normal size (100%)

### Adjusting the Size (Optional)

If you want the screwdriver to be a different size:

1. Open `Assets/Scripts/Player/Inventory/ItemSlot.cs` in your code editor
2. Find this line (around line 95):
   ```csharp
   itemIcon.rectTransform.localScale = new Vector3(0.7f, 0.7f, 1f);
   ```
3. Change `0.7f` to your desired size:
   - `0.8f` = 80% size (slightly smaller)
   - `0.7f` = 70% size (current - noticeably smaller)
   - `0.6f` = 60% size (much smaller)
   - `0.5f` = 50% size (half size)
4. **Save** the file
5. **Test** in Unity

### Testing

1. **Play the game**
2. Pick up the screwdriver
3. Check the inventory - the screwdriver icon should be smaller
4. Other items should remain their original size

---

## Quick Reference

**What you're changing:**
- ✅ Screwdriver icon sprite (Pixels Per Unit OR new sprite)
- ✅ ScrewdriverSO Icon field

**What you're NOT changing:**
- ❌ ItemSlot prefab (don't touch this)
- ❌ ItemImage GameObject (don't touch this)
- ❌ Other item sprites

---

## Troubleshooting

**Q: The icon is still too big**
- Try increasing Pixels Per Unit more (try 200 or 250)
- Or create a sprite with more padding around it

**Q: The icon is now too small**
- Decrease Pixels Per Unit (try 120 or 130)
- Or use a sprite with less padding

**Q: I can't find ScrewdriverSO**
- Search in Project window: Type "Screwdriver" in search bar
- Look in `Assets/ScriptableObjects/` folder
- Or check where you created it in the setup guide

**Q: Other items are also smaller now**
- You must have modified the ItemSlot prefab by mistake
- Only change the ScrewdriverSO Icon field, nothing else
- If you modified ItemSlot, undo those changes

---

## Recommended: Use Option A (Pixels Per Unit)

**This is the easiest method:**
1. Select screwdriver sprite
2. Change Pixels Per Unit from `100` to `150-200`
3. Click Apply
4. Update ScrewdriverSO Icon field (drag the same sprite)
5. Done!

No image editing needed, and it only affects the screwdriver!
