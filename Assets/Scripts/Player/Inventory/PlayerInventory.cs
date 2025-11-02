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
}