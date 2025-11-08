using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    // Make public so you can drag and drop your UI ItemSlot components here in the Inspector
    public ItemSlot[] itemSlots;
    //contains ALL item SOs in game.
    public ItemSO[] itemSOs;
    public ConsumableSO[] consumableSOs;

    // Static instance setup (Singleton pattern)
    public static PlayerInventory instance;

    [Header("Item Description UI")]

    public Image itemDescriptionIcon;
    public TMP_Text ItemDescriptionNameText;
    public TMP_Text ItemDescriptionText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Note: If you use GetComponentsInChildren<ItemSlot>(), do it here:
            // itemSlots = GetComponentsInChildren<ItemSlot>();
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

        for (int i = 0; i < consumableSOs.Length; i++)
        {
            if (consumableSOs[i] != null && consumableSOs[i].itemName == itemName)
            {
                if (consumableSOs[i].ConsumeItem())
                {
                    itemSlots[inventoryLocation].DecreaseQuantity(1);
                }
                return;
            }
        }
        Debug.LogWarning("Item is not a consumable: " + itemName);
    }
    public void AddItem(Item item)
    {
        if (item == null || itemSlots == null || item.quantity <= 0) return;

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

            // Check for full consumption *after* every slot interaction
            if (item.quantity <= 0)
            {
                // Fully added!
                return;
            }
        }

        // If we reach here, the item was not fully added.
        if (item.quantity > 0)
        {
            Debug.LogWarning("Inventory Full! Could not add remaining item: " + item.itemName);
        }
    }

    public bool UseItem(string itemName, int amount)
    {
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot != null && slot.itemName == itemName && slot.quantity > 0)
            {
                slot.DecreaseQuantity(amount);
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
            }
        }
    }

    public void FillDescriptionUI(string descName, string descText, Sprite descIcon)
    {
        if (ItemDescriptionNameText == null || ItemDescriptionText == null || itemDescriptionIcon == null)
        {
            Debug.LogWarning("PlayerInventory: Description UI elements are not assigned!");
            return;
        }

        ItemDescriptionNameText.text = descName ?? "";
        ItemDescriptionText.text = descText ?? "";
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
                
                // If not found in consumables, check regular itemSOs
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
    }
}