using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Make public so you can drag and drop your UI ItemSlot components here in the Inspector
    public ItemSlot[] itemSlots;

    // Static instance setup (Singleton pattern)
    public static PlayerInventory instance;

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
}