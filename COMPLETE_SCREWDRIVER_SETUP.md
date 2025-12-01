# Complete Screwdriver Setup Guide

This guide covers everything you need to set up the screwdriver: creating the item, putting it in a box, and making it work with the vending machine.

---

## PART 1: Create the Screwdriver ItemSO (ScriptableObject)

1. In Unity **Project** window, navigate to where your ItemSOs are stored (usually `Assets/Items_SOs/` or `Assets/ScriptableObjects/Items/`)

2. **Right-click** in that folder → **Create** → Find **"ItemSO"** (or whatever your item scriptable object is called)

3. Name it **"Screwdriver"** or **"ScrewdriverSO"**

4. In the **Inspector**, configure:
   - **Item Name**: `"Screwdriver"` ⚠️ **MUST match exactly (case-sensitive)!**
   - **Icon**: Drag your screwdriver **inventory icon** sprite here (what shows in inventory)
   - **Drop Icon**: Drag your screwdriver **world sprite** here (what appears on the ground)
   - **Max Stack Size**: `1` (tools usually don't stack)
   - **Quantity**: `1`
   - **Item Description**: "A tool for removing screws from panels"

---

## PART 2: Create the Screwdriver Item Prefab

### Step 2A: Find the Item Prefab Template

1. In **Project** window, search for **"Item.prefab"**
   - Location: `Assets/Prefabs/UI/Item.prefab`
2. **Right-click** on `Item.prefab` → **Duplicate**
3. Rename the duplicate to **"ScrewdriverPrefab"**

### Step 2B: Configure the Screwdriver Prefab

1. **Double-click** `ScrewdriverPrefab` to open it in **Prefab view**

2. **Select the root GameObject** in the Hierarchy (inside the prefab)

3. Check/Add these components:

   **A. Item Component** (should already exist):
   - Find **Item** component in Inspector
   - Set **Item SO**: Drag your **ScrewdriverSO** here
   - Other fields will auto-fill from ItemSO

   **B. PhysicalItemModel Component** (should already exist):
   - This loads the sprite automatically
   - No configuration needed

   **C. Rigidbody2D Component**:
   - Add if missing: **Add Component** → **Physics 2D** → **Rigidbody 2D**
   - Set **Body Type**: `Dynamic` or `Kinematic`
   - Set **Gravity Scale**: `1`

   **D. Collider2D Component** (CRITICAL for pickup):
   - Add if missing: **Add Component** → **Physics 2D** → **Box Collider 2D**
   - ✅ **Check "Is Trigger"** (MUST be checked!)
   - Adjust **Size** to match your sprite

   **E. SpriteRenderer Component**:
   - Should be on the root or a child GameObject
   - Will show the **Drop Icon** sprite automatically

4. **Save** the prefab (click the back arrow or press **Ctrl+S**)

---

## PART 3: Add ScrewdriverSO to PlayerInventory

### Step 3A: Find the PlayerInventory GameObject

**EXACT LOCATION FOUND IN YOUR PROJECT:**

Based on your files, PlayerInventory is located in these scenes:

1. **`Assets/Scenes/InventoryTest.unity`** ✅ (Test scene - easiest to use)
2. **`Assets/Scenes/ParticleFromBoxes.unity`** ✅

**EXACT STEPS TO FIND IT:**

**Option 1: Use InventoryTest Scene (EASIEST - RECOMMENDED)**
1. In Unity Editor, go to **File → Open Scene**
2. Navigate to: `Assets/Scenes/InventoryTest.unity`
3. **Double-click** to open it
4. Look at the **Hierarchy** window (usually on the left side of Unity)
5. Find the GameObject named **"GameManager"** (it's at the root level)
6. **Click on "GameManager"** in the Hierarchy
7. Look at the **Inspector** window (usually on the right side)
8. You'll see **MULTIPLE components** on GameManager:
   - `GameManager` (script) - first component
   - `PlayerInventory` (script) - second component ← **THIS IS WHAT YOU NEED!**
   - `HealthManager` (script) - third component
9. **Scroll down in Inspector** until you see the **PlayerInventory** component
10. **Click on the PlayerInventory component header** to expand it (if collapsed)
11. You should now see the **ItemSOs** array field

**Option 2: Search in Any Scene**
1. Open **any scene** in Unity
2. In the **Hierarchy** window, look at the **top search bar**
3. Type: `PlayerInventory`
4. If it exists in the scene, it will highlight
5. **OR** type: `GameManager` (since PlayerInventory is attached to GameManager in test scenes)

**Option 3: Find by Component Type**
1. In **Hierarchy** window, click the **search icon** (magnifying glass)
2. Select **"Component"** from the dropdown
3. Type: `PlayerInventory`
4. It will show all GameObjects with that component

**IMPORTANT NOTES:**
- In your test scenes, PlayerInventory is attached to the **"GameManager"** GameObject
- The GameObject name is **"GameManager"**, but it has the **PlayerInventory** script component
- **Prison.unity does NOT have PlayerInventory** (I checked your files)
- **SOLUTION**: Since PlayerInventory uses `DontDestroyOnLoad`, you can:
  - **Configure it in InventoryTest.unity** (easiest)
  - When you play from Prison.unity, the PlayerInventory will persist from the test scene
  - **OR** add PlayerInventory component to GameManager in Prison.unity manually:
    1. Open `Prison.unity`
    2. Find or create a "GameManager" GameObject
    3. Add Component → Search "PlayerInventory" → Add
    4. Configure it the same way

### Step 3B: Configure PlayerInventory

1. **Select** the PlayerInventory GameObject

2. In the **Inspector** window, find the **PlayerInventory** component

3. Look for the **ItemSOs** array field (it should be near the top)

4. **Add ScrewdriverSO** to the array:
   - Find the **ItemSOs** array
   - Click the **+** button (or increase the **Size** field) to add a new slot
   - **Drag your ScrewdriverSO** from Project window into the new slot
   - The array should now show your ScrewdriverSO

**Visual Example:**
```
PlayerInventory Component:
├── Item Slots: [array of ItemSlot components]
├── ItemSOs: [array]
│   ├── [0] Lockpick (ItemSO)
│   ├── [1] Ammo (ItemSO)
│   ├── [2] Screwdriver (ItemSO) ← Add here!
│   └── ...
├── ConsumableSOs: [array]
└── EquipmentSOs: [array]
```

---

## PART 4: Put Screwdriver in a Box

### Step 4A: Find/Prepare the Box Prefab

1. In **Project** window, find `Assets/Prefabs/Box.prefab`

2. **Option A - Use Existing Box** (changes all boxes):
   - **Double-click** `Box.prefab` to open it

3. **Option B - Create Separate Box** (recommended):
   - **Right-click** on `Box.prefab` → **Duplicate**
   - Rename to **"ScrewdriverBox"** or **"BoxWithScrewdriver"**
   - **Double-click** the duplicate to open it

### Step 4B: Configure Box to Drop Screwdriver

1. **Select the root GameObject** of the Box prefab

2. Find the **DropItemsOnDeath** component in Inspector

3. Configure these fields:

   **A. Item Prefab**:
   - **Item Prefab**: Drag `Assets/Prefabs/UI/Item.prefab` here
   - This is the template that spawns when box breaks

   **B. Items Array**:
   - Find **Items** array
   - **Increase array size** by 1 (click **+** button)
   - Drag your **ScrewdriverSO** into the new slot
   - If you want ONLY screwdriver, set array size to 1 and put ScrewdriverSO in slot 0

   **C. Item Drop Chances Array**:
   - Find **Item Drop Chances** array
   - **Make sure array size matches Items array**
   - Set the chance for screwdriver:
     - **100** = Always drops (guaranteed) ✅ Recommended
     - **50** = 50% chance
     - **0** = Never drops

   **Example Setup** (Guaranteed Screwdriver):
   ```
   Items:
   [0] Screwdriver (ItemSO)
   
   Item Drop Chances:
   [0] 100
   ```

   **Example Setup** (Multiple Items):
   ```
   Items:
   [0] Lockpick (ItemSO)
   [1] Screwdriver (ItemSO)
   
   Item Drop Chances:
   [0] 50
   [1] 100
   ```

4. **Link to Crate Component**:
   - Find **Crate** component on the same GameObject
   - Make sure **Drop Items On Death** field is assigned:
     - Drag the **DropItemsOnDeath** component into this field
     - OR it should auto-link if on same GameObject

5. **Save** the prefab

---

## PART 5: Place Box in Scene

1. **Drag** your Box prefab (or ScrewdriverBox if you duplicated) from **Project** into your **Scene** (Hierarchy)

2. **Position** it where you want players to find the screwdriver

3. Make sure it's at ground level or appropriate height

---

## PART 6: Configure Vending Machine Requirement

1. Find the GameObject with **VendingPopupInteractable** component in your scene

2. Select it and find **VendingPopupInteractable** in Inspector

3. Find **"Screwdriver Requirement"** section

4. Set **"Screwdriver Item Name"** to: `Screwdriver` (must match ItemSO name exactly!)

---

## PART 7: Test Everything

### Test 1: Box Drops Screwdriver
1. **Play** the game
2. **Break the box** (attack it)
3. Screwdriver should **spawn on the ground** when box breaks
4. Check Console for any errors

### Test 2: Pick Up Screwdriver
1. **Walk your player** into the screwdriver on the ground
2. Screwdriver should:
   - **Disappear** from ground
   - **Appear in inventory**
   - Show in inventory UI

### Test 3: Screwdriver Requirement
1. **Without screwdriver**: Try to open screw panel → Should NOT open (check Console for message)
2. **With screwdriver**: After picking it up, try to open screw panel → Should work!

---

## Troubleshooting

### Screwdriver doesn't appear when box breaks:
- ✅ Check **Item Prefab** is assigned in DropItemsOnDeath
- ✅ Check **ScrewdriverSO** is in Items array
- ✅ Check **Item Drop Chance** is > 0 (try 100)
- ✅ Check **Crate** component has DropItemsOnDeath linked
- ✅ Check Console for error messages

### Screwdriver appears but can't be picked up:
- ✅ Check **Collider2D** has **Is Trigger** checked
- ✅ Check player GameObject has **Tag** set to **"Player"**
- ✅ Check player has a **Collider2D** (not trigger)
- ✅ Check Console for "picking item up" message

### Screwdriver doesn't appear in inventory:
- ✅ Check **ScrewdriverSO** is in **PlayerInventory.ItemSOs** array
- ✅ Check **Item Name** in ItemSO matches exactly (case-sensitive)
- ✅ Check Console for errors

### Screw panel still opens without screwdriver:
- ✅ Check **"Screwdriver Item Name"** in VendingPopupInteractable matches ItemSO name exactly
- ✅ Check Console for debug messages about screwdriver check
- ✅ Verify player actually has screwdriver: `PlayerInventory.instance.HasItem("Screwdriver")`

### Box doesn't break:
- ✅ Check box has **Crate** component
- ✅ Check box has **Collider2D** (not trigger)
- ✅ Check player attacks can hit the box
- ✅ Check box **Health** is set to a reasonable value

---

## Quick Checklist

### ItemSO:
- [ ] ScrewdriverSO created
- [ ] Item Name = "Screwdriver" (exact match)
- [ ] Icon assigned (inventory sprite)
- [ ] Drop Icon assigned (world sprite)

### Item Prefab:
- [ ] ScrewdriverPrefab created (duplicated from Item.prefab)
- [ ] Item component has ScrewdriverSO assigned
- [ ] PhysicalItemModel component present
- [ ] Rigidbody2D component present
- [ ] Collider2D with Is Trigger checked
- [ ] SpriteRenderer present

### PlayerInventory:
- [ ] ScrewdriverSO added to ItemSOs array

### Box:
- [ ] Box prefab has DropItemsOnDeath component
- [ ] Item Prefab assigned (Item.prefab)
- [ ] ScrewdriverSO in Items array
- [ ] Item Drop Chance set (100 for guaranteed)
- [ ] Crate component linked to DropItemsOnDeath

### Vending Machine:
- [ ] VendingPopupInteractable has Screwdriver Item Name = "Screwdriver"

### Testing:
- [ ] Box breaks when attacked
- [ ] Screwdriver drops from box
- [ ] Player can pick up screwdriver
- [ ] Screwdriver appears in inventory
- [ ] Screw panel requires screwdriver to open

---

## Summary

**Complete Flow:**
1. Create ScrewdriverSO → Configure Item Name, Icons
2. Create ScrewdriverPrefab → Add components, assign ItemSO
3. Add ScrewdriverSO to PlayerInventory.ItemSOs array
4. Configure Box → Add ScrewdriverSO to DropItemsOnDeath
5. Place Box in scene
6. Configure VendingPopupInteractable → Set Screwdriver Item Name
7. Test: Break box → Pick up screwdriver → Open screw panel

**When box breaks → Screwdriver drops → Player picks it up → Screwdriver in inventory → Can open screw panel!**

