using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PlayerInventory : MonoBehaviour
{
    // Make public so you can drag and drop your UI ItemSlot components here in the Inspector
    public ItemSlot[] itemSlots;
    //contains ALL item SOs in game.
    public ItemSO[] itemSOs;
    public ConsumableSO[] consumableSOs;
    public EquipmentSO[] equipmentSOs;

    [Header("Equipment Slots")]
    // Equipment slots - assign these in the Inspector (one for each EquipmentSlot type)
    public EquipmentSlot[] equipmentSlots;

    // Static instance setup (Singleton pattern)
    public static PlayerInventory instance;

    [Header("Item Description UI")]

    public Image itemDescriptionIcon;
    public TMP_Text ItemDescriptionNameText;
    public TMP_Text ItemDescriptionText;

    [SerializeField] private EquipmentChangeEventSO equipEventSO;
    [SerializeField] private EquipmentChangeEventSO unequipEventSO;
    [SerializeField] private VoidEvents inventoryChangedEventSO;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ClearDescriptionPanel();
            DeselectAllSlots();
            DeselectAllEquipmentSlots();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UseConsumable(string itemName, int inventoryLocation)
    {
        if (itemSlots == null)
        {
            Debug.LogError("PlayerInventory: itemSlots array is null!");
            return;
        }

        if (inventoryLocation < 0 || inventoryLocation >= itemSlots.Length)
        {
            Debug.LogError($"PlayerInventory: inventoryLocation {inventoryLocation} is out of bounds! Array length is {itemSlots.Length}");
            return;
        }

        if (itemSlots[inventoryLocation] == null)
        {
            Debug.LogError($"PlayerInventory: itemSlots[{inventoryLocation}] is null!");
            return;
        }

        if (consumableSOs == null)
        {
            Debug.LogWarning("PlayerInventory: consumableSOs array is null!");
            return;
        }

        // Check if the slot has any quantity left
        if (itemSlots[inventoryLocation].quantity <= 0)
        {
            Debug.LogWarning($"PlayerInventory: Cannot use consumable {itemName} - quantity is 0!");
            return;
        }
        
        for (int i = 0; i < consumableSOs.Length; i++)
        {
            if (consumableSOs[i] != null && consumableSOs[i].itemName == itemName)
            {
                if (consumableSOs[i].ConsumeItem())
                {
                    itemSlots[inventoryLocation].DecreaseQuantity(1);
                }
                inventoryChangedEventSO.RaiseEvent();
                return;
            }
        }
        Debug.LogWarning("Item is not a consumable: " + itemName);
    }
    public bool AddItem(Item item)
    {
        if (item == null || itemSlots == null || item.quantity <= 0) return false;

        int originalQuantity = item.quantity; // Store original quantity

        // We will loop until the item is fully added (item.quantity <= 0) OR we run out of slots.

        // --- 1. Try to STACK or find an EMPTY slot to initialize ---
        // We check all slots in one loop.
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null) continue; // Skip null slots

            // 1a. Prioritize stacking onto existing partial stacks
            if (itemSlots[i].IsSameItem(item))
            {
                itemSlots[i].AddItem(item);
            }
            // 1b. If the item still has quantity left, try to use the empty slot
            else if (item.quantity > 0 && itemSlots[i].IsEmpty())
            {
                itemSlots[i].AddItem(item);
            }
            inventoryChangedEventSO.RaiseEvent();
            // Check for full consumption *after* every slot interaction
            if (item.quantity <= 0)
            {
                // Fully added!
                return true;
            }
        }

        // If we reach here, the item was not fully added.
        if (item.quantity > 0)
        {
            Debug.LogWarning("Inventory Full! Could not add remaining item: " + item.itemName + " (remaining: " + item.quantity + ")");
            return false; // Return false to indicate item was not fully added
        }

        return true; // Item was fully added
    }

    public bool UseItem(string itemName, int amount)
    {
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot != null && slot.itemName == itemName && slot.quantity > 0)
            {
                // Check if this is a consumable - if so, consume it properly
                if (consumableSOs != null)
                {
                    for (int i = 0; i < consumableSOs.Length; i++)
                    {
                        if (consumableSOs[i] != null && consumableSOs[i].itemName == itemName)
                        {
                            // It's a consumable - try to consume it
                            bool consumed = false;
                            for (int j = 0; j < amount; j++)
                            {
                                if (consumableSOs[i].ConsumeItem())
                                {
                                    consumed = true;
                                }
                            }

                            // Only decrease quantity if consumption was successful
                            if (consumed)
                            {
                                slot.DecreaseQuantity(amount);
                                inventoryChangedEventSO.RaiseEvent();
                                Debug.Log("Used " + amount + " " + itemName);
                                return true;
                            }
                            else
                            {
                                Debug.LogWarning($"Cannot consume {itemName} - consumption failed (e.g., health already full)");
                                return false;
                            }
                        }
                    }
                }

                // Not a consumable, or consumableSOs array is null - just decrease quantity
                slot.DecreaseQuantity(amount);
                inventoryChangedEventSO.RaiseEvent();
                Debug.Log("Used " + amount + " " + itemName);
                return true;
            }
        }
        return false;
    }

    public void DeselectAllSlots()
    {
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot != null) // Always check for null here too!
            {
                slot.selectedShader.SetActive(false);
                slot.thisItemSelected = false;
            }
        }
    }

    public void DeselectAllEquipmentSlots()
    {
        if (equipmentSlots == null) return;

        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot != null)
            {
                if (slot.selectedShader != null)
                {
                    slot.selectedShader.SetActive(false);
                }
                slot.thisItemSelected = false;
            }
        }
    }

    public void ClearDescriptionPanel()
    {
        if (ItemDescriptionNameText != null) ItemDescriptionNameText.text = "";
        if (ItemDescriptionText != null) ItemDescriptionText.text = "";
        if (itemDescriptionIcon != null) itemDescriptionIcon.enabled = false;
    }

    public void ClearInventory()
    {
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
                inventoryChangedEventSO.RaiseEvent();
            }
        }
    }

    public void FillDescriptionUI(string descName, string descText, Sprite descIcon)
    {
        FillDescriptionUI(descName, descText, descIcon, null);
    }

    public void FillDescriptionUI(string descName, string descText, Sprite descIcon, EquipmentSO equipment)
    {
        if (ItemDescriptionNameText == null || ItemDescriptionText == null || itemDescriptionIcon == null)
        {
            Debug.LogWarning("PlayerInventory: Description UI elements are not assigned!");
            return;
        }

        ItemDescriptionNameText.text = descName ?? "";

        // Build description text with stats if equipment is provided
        string fullDescription = descText ?? "";
        if (equipment != null && equipment.itemStats != null && equipment.itemStatAmounts != null)
        {
            string statsText = FormatEquipmentStats(equipment);
            if (!string.IsNullOrEmpty(statsText))
            {
                if (!string.IsNullOrEmpty(fullDescription))
                {
                    fullDescription += "\n\n";
                }
                fullDescription += statsText;
            }
        }

        ItemDescriptionText.text = fullDescription;
        itemDescriptionIcon.sprite = descIcon;
        if (descIcon == null)
        {
            itemDescriptionIcon.enabled = false;
        }
        else
        {
            itemDescriptionIcon.enabled = true;
        }
    }

    private string FormatEquipmentStats(EquipmentSO equipment)
    {
        if (equipment.itemStats == null || equipment.itemStatAmounts == null)
        {
            return "";
        }

        if (equipment.itemStats.Count == 0 || equipment.itemStats.Count != equipment.itemStatAmounts.Count)
        {
            return "";
        }

        System.Text.StringBuilder statsBuilder = new System.Text.StringBuilder();
        statsBuilder.Append("<color=yellow>Stats:</color>\n");

        for (int i = 0; i < equipment.itemStats.Count && i < equipment.itemStatAmounts.Count; i++)
        {
            EquipmentSO.Stats stat = equipment.itemStats[i];
            float amount = equipment.itemStatAmounts[i];

            string statName = FormatStatName(stat);
            string statValue = FormatStatValue(stat, amount);

            // Determine color and sign based on value
            string color = amount >= 0 ? "green" : "red";
            string sign = amount >= 0 ? "+" : ""; // Negative numbers already have a minus sign

            statsBuilder.Append($"  {statName}: <color={color}>{sign}{statValue}</color>\n");
        }

        return statsBuilder.ToString();
    }

    private string FormatStatName(EquipmentSO.Stats stat)
    {
        switch (stat)
        {
            case EquipmentSO.Stats.MaxHealth:
                return "Max Health";
            case EquipmentSO.Stats.MeleeAttack:
                return "Melee Attack";
            case EquipmentSO.Stats.WeaponlessMeleeAttack:
                return "Weaponless Attack";
            case EquipmentSO.Stats.RangedAttack:
                return "Ranged Attack";
            case EquipmentSO.Stats.UniversalAttack:
                return "Universal Attack";
            case EquipmentSO.Stats.MovementSpeed:
                return "Movement Speed";
            case EquipmentSO.Stats.JumpCount:
                return "Jump Count";
            case EquipmentSO.Stats.DashSpeed:
                return "Dash Speed";
            case EquipmentSO.Stats.SlideSpeed:
                return "Slide Speed";
            case EquipmentSO.Stats.MaxAmmo:
                return "Max Ammo";
            case EquipmentSO.Stats.BulletSpeed:
                return "Bullet Speed";
            case EquipmentSO.Stats.BulletCount:
                return "Bullet Count";
            case EquipmentSO.Stats.MaxEnergy:
                return "Max Energy";
            case EquipmentSO.Stats.EnergyRegenRate:
                return "Energy Regen";
            default:
                return stat.ToString();
        }
    }

    private string FormatStatValue(EquipmentSO.Stats stat, float amount)
    {
        // Check if this is an integer stat
        bool isIntStat = stat == EquipmentSO.Stats.MaxHealth ||
                         stat == EquipmentSO.Stats.MaxAmmo ||
                         stat == EquipmentSO.Stats.JumpCount ||
                         stat == EquipmentSO.Stats.BulletCount ||
                         stat == EquipmentSO.Stats.MeleeAttack ||
                         stat == EquipmentSO.Stats.WeaponlessMeleeAttack ||
                         stat == EquipmentSO.Stats.RangedAttack ||
                         stat == EquipmentSO.Stats.UniversalAttack;

        if (isIntStat)
        {
            return ((int)amount).ToString();
        }
        else
        {
            // Format float with 1 decimal place, remove trailing zeros
            return amount.ToString("0.#");
        }
    }

    public int HasItem(string itemName) //return 0 if none found lol.
    {
        int count = 0;
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].itemName == itemName)
            {
                count += itemSlots[i].quantity;
            }
        }
        Debug.Log("PlayerInventory: HasItem found " + count + " of item: " + itemName);
        return count;
    }

    // Restores inventory state from checkpoint data.
    public void RestoreInventory(List<CheckpointManager.InventorySlotData> savedSlots)
    {
        if (itemSlots == null || savedSlots == null)
        {
            Debug.LogWarning("PlayerInventory: Cannot restore inventory - itemSlots or savedSlots is null");
            return;
        }

        // Deselect all slots first
        DeselectAllSlots();
        
        // Clear description panel
        ClearDescriptionPanel();

        // Clear current inventory
        ClearInventory();

        // Restore each slot
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null) continue;

            // If we have saved data for this slot, restore it
            if (i < savedSlots.Count)
            {
                var savedSlot = savedSlots[i];
                
                // If slot is empty in checkpoint, ensure it's cleared
                if (string.IsNullOrEmpty(savedSlot.itemName) || savedSlot.quantity <= 0)
                {
                    itemSlots[i].ClearSlot();
                    continue;
                }

                // Find the ItemSO by name - check consumableSOs first (since ConsumableSO extends ItemSO)
                ItemSO foundItemSO = null;
                
                // First check consumableSOs (for consumables)
                if (consumableSOs != null)
                {
                    foreach (ConsumableSO consumableSO in consumableSOs)
                    {
                        if (consumableSO != null && consumableSO.itemName == savedSlot.itemName)
                        {
                            foundItemSO = consumableSO; // ConsumableSO extends ItemSO, so this works
                            break;
                        }
                    }
                }
                
                // If not found in consumables, check equipmentSOs (for equipment)
                if (foundItemSO == null && equipmentSOs != null)
                {
                    foreach (EquipmentSO equipmentSO in equipmentSOs)
                    {
                        if (equipmentSO != null && equipmentSO.itemName == savedSlot.itemName)
                        {
                            foundItemSO = equipmentSO; // EquipmentSO extends ItemSO, so this works
                            break;
                        }
                    }
                }

                // If not found in consumables or equipment, check regular itemSOs
                if (foundItemSO == null && itemSOs != null)
                {
                    foreach (ItemSO itemSO in itemSOs)
                    {
                        if (itemSO != null && itemSO.itemName == savedSlot.itemName)
                        {
                            foundItemSO = itemSO;
                            break;
                        }
                    }
                }

                if (foundItemSO != null)
                {
                    // Directly restore the slot data (don't use AddItem which can stack items)
                    // This ensures items are restored to their exact saved slot positions
                    itemSlots[i].RestoreSlot(foundItemSO, savedSlot.quantity);
                }
                else
                {
                    Debug.LogWarning($"PlayerInventory: Could not find ItemSO for item '{savedSlot.itemName}'");
                    // Clear the slot if we can't find the ItemSO
                    itemSlots[i].ClearSlot();
                }
            }
            else
            {
                // No saved data for this slot (checkpoint had fewer slots) - ensure it's cleared
                itemSlots[i].ClearSlot();
            }
        }
        inventoryChangedEventSO.RaiseEvent();
    }

    // ========== EQUIPMENT SYSTEM ==========

    /// <summary>
    /// Tries to equip an equipment item from an inventory slot
    /// </summary>
    public bool EquipItemFromInventory(int inventorySlotIndex)
    {
        if (itemSlots == null || inventorySlotIndex < 0 || inventorySlotIndex >= itemSlots.Length)
        {
            return false;
        }

        ItemSlot slot = itemSlots[inventorySlotIndex];
        if (slot == null || slot.IsEmpty() || slot.quantity <= 0)
        {
            return false;
        }

        // Check if the item is an equipment
        EquipmentSO equipment = GetEquipmentSO(slot.itemName);
        if (equipment == null)
        {
            Debug.LogWarning($"Item {slot.itemName} is not an equipment!");
            return false;
        }

        if (equipment.equipableBy != EquipmentSO.EquipableBy.Both && GameManager.Instance.selectedCharacter.ToString() != equipment.equipableBy.ToString())
        {
            Debug.LogWarning($"Item {slot.itemName} is not equipable by {GameManager.Instance.selectedCharacter}!");
            return false;
        }
        // Find the appropriate equipment slot
        EquipmentSlot targetEquipmentSlot = GetEquipmentSlotByType(equipment.equipmentType);
        if (targetEquipmentSlot == null)
        {
            Debug.LogWarning($"No equipment slot found for type {equipment.equipmentType}!");
            return false;
        }


        // Equip the item
        targetEquipmentSlot.EquipItem(equipment);

        // Remove one from inventory
        slot.DecreaseQuantity(1);
        inventoryChangedEventSO.RaiseEvent();

        return true;
    }

    /// <summary>
    /// Adds an equipment item back to inventory when unequipped
    /// </summary>
    public void AddEquipmentToInventory(EquipmentSO equipment)
    {
        if (equipment == null) return;

        // Create a temporary Item object to add to inventory
        // We'll need to create a GameObject with Item component temporarily
        GameObject tempItem = new GameObject("TempEquipmentItem");
        Item itemComponent = tempItem.AddComponent<Item>();

        // Initialize the item with equipment data
        itemComponent.itemSO = equipment;
        itemComponent.itemName = equipment.itemName;
        itemComponent.icon = equipment.icon;
        itemComponent.dropIcon = equipment.dropIcon;
        itemComponent.maxStackSize = equipment.maxStackSize;
        itemComponent.quantity = 1;
        itemComponent.itemDesc = equipment.itemDesc;

        // Add to inventory
        AddItem(itemComponent);

        // Clean up temporary object
        Destroy(tempItem);
    }

    /// <summary>
    /// Adds an item to inventory directly from an ItemSO (for trades, rewards, etc.)
    /// </summary>
    public bool AddItemFromItemSO(ItemSO itemSO, int quantity = 1)
    {
        if (itemSO == null || quantity <= 0) return false;

        // Create a temporary Item object to add to inventory
        GameObject tempItem = new GameObject("TempItem");
        Item itemComponent = tempItem.AddComponent<Item>();

        // Initialize the item with ItemSO data
        itemComponent.itemSO = itemSO;
        itemComponent.itemName = itemSO.itemName;
        itemComponent.icon = itemSO.icon;
        itemComponent.dropIcon = itemSO.dropIcon;
        itemComponent.maxStackSize = itemSO.maxStackSize;
        itemComponent.quantity = quantity;
        itemComponent.itemDesc = itemSO.itemDesc;

        // Add to inventory
        bool success = AddItem(itemComponent);

        // Clean up temporary object
        Destroy(tempItem);

        return success;
    }

    /// <summary>
    /// Gets an EquipmentSO by name
    /// </summary>
    private EquipmentSO GetEquipmentSO(string itemName)
    {
        if (equipmentSOs == null) return null;

        foreach (EquipmentSO equipment in equipmentSOs)
        {
            if (equipment != null && equipment.itemName == itemName)
            {
                return equipment;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets an equipment slot by its type
    /// </summary>
    private EquipmentSlot GetEquipmentSlotByType(EquipmentSO.EquipmentSlot slotType)
    {
        if (equipmentSlots == null) return null;

        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot != null && slot.slotType == slotType)
            {
                return slot;
            }
        }
        return null;
    }

    /// <summary>
    /// Called when equipment is equipped - apply stat bonuses
    /// </summary>
    public void OnEquipmentEquipped(EquipmentSO equipment, EquipmentSO previousEquipment = null)
    {
        if (equipment == null) return;

        // Trigger the event
        equipEventSO.RaiseEvent(equipment);

        // Apply stat bonuses from equipment
        if (StatsManager.instance != null)
        {
            StatsManager.instance.AddEquipmentStats(equipment);
            Debug.Log($"Equipped {equipment.itemName} - Stats applied");
        }
        else
        {
            Debug.LogWarning("PlayerInventory: StatsManager.instance is null! Cannot apply equipment stats.");
        }

        // If this is a melee weapon, handle weapon equipped state
        if (equipment.equipmentType == EquipmentSO.EquipmentSlot.Melee)
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                BasePlayerMovement2D playerMovement = GameManager.Instance.player.GetComponent<BasePlayerMovement2D>();
                if (playerMovement != null)
                {
                    // If the new weapon requires weaponEquipped = true, but previous weapon disabled it,
                    // we need to check if we should allow this
                    if (!equipment.disablesHeldWeapon && previousEquipment != null && previousEquipment.disablesHeldWeapon)
                    {
                        // Previous weapon disabled held weapon, new weapon requires it
                        // This is fine - the new weapon replaces the old one
                        playerMovement.SetWeaponEquipped(true);
                    }
                    else
                    {
                        // Only set weapon equipped if it doesn't disable the held weapon
                        // Weapons that disable held weapon are meant to boost weaponless attacks
                        playerMovement.SetWeaponEquipped(!equipment.disablesHeldWeapon);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when equipment is unequipped - remove stat bonuses
    /// </summary>
    public void OnEquipmentUnequipped(EquipmentSO equipment)
    {
        if (equipment == null) return;

        // Trigger the event
        unequipEventSO.RaiseEvent(equipment);

        // Remove stat bonuses from equipment
        if (StatsManager.instance != null)
        {
            StatsManager.instance.RemoveEquipmentStats(equipment);
            Debug.Log($"Unequipped {equipment.itemName} - Stats removed");
        }
        else
        {
            Debug.LogWarning("PlayerInventory: StatsManager.instance is null! Cannot remove equipment stats.");
        }

        // If this is a melee weapon, unequip the weapon on the player
        if (equipment.equipmentType == EquipmentSO.EquipmentSlot.Melee)
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                BasePlayerMovement2D playerMovement = GameManager.Instance.player.GetComponent<BasePlayerMovement2D>();
                if (playerMovement != null)
                {
                    // Check if there's another melee weapon equipped that should keep weaponEquipped = true
                    // Note: This is called BEFORE the new weapon is equipped, so we check the slot
                    bool shouldKeepWeaponEquipped = false;
                    EquipmentSlot meleeSlot = GetEquipmentSlotByType(EquipmentSO.EquipmentSlot.Melee);
                    if (meleeSlot != null && !meleeSlot.IsEmpty())
                    {
                        EquipmentSO otherMelee = meleeSlot.GetEquippedItem();
                        if (otherMelee != null && !otherMelee.disablesHeldWeapon)
                        {
                            shouldKeepWeaponEquipped = true;
                        }
                    }
                    playerMovement.SetWeaponEquipped(shouldKeepWeaponEquipped);
                }
            }
        }
    }

    /// <summary>
    /// Checks if an item is an equipment
    /// </summary>
    public bool IsEquipment(string itemName)
    {
        return GetEquipmentSO(itemName) != null;
    }
}