using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    // Item Data 
    public string itemName = null;
    public int quantity = 0;
    public Sprite itemSprite = null;
    [TextArea] public string itemDesc = null;
    public int maxStackSize = 1;

    // UI References
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    public GameObject selectedShader;
    public bool thisItemSelected = false;

    //item desc
    public Image itemDescriptionIcon;
    public TMP_Text ItemDescriptionNameText;
    public TMP_Text ItemDescriptionText;

    // Call UpdateUI() when the script starts to ensure the UI matches the data.
    void Start()
    {
        // This ensures the icon and text are correctly hidden when the game starts.
        UpdateUI();
    }

    /// <summary>
    /// Checks if the slot is completely empty (no item name or quantity).
    /// </summary>
    public bool IsEmpty()
    {
        // Use string.IsNullOrEmpty for the most robust check for an empty/null name.
        return string.IsNullOrEmpty(itemName);
    }

    public void AddItem(Item item)
    {
        if (item == null || item.quantity <= 0) return;

        // 1. If empty slot, initialize it with the new item's data
        if (IsEmpty())
        {
            itemName = item.itemName;
            itemSprite = item.icon;
            maxStackSize = item.maxStackSize;
            itemDesc = item.itemDesc;
        }

        // 2. Calculate how much can be added
        int spaceLeft = maxStackSize - quantity;
        int amountToTransfer = Mathf.Min(spaceLeft, item.quantity);

        // 3. Transfer
        quantity += amountToTransfer;
        item.quantity -= amountToTransfer;

        // 4. Update the UI
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Check if the slot is empty based on quantity or name
        if (quantity <= 0 || IsEmpty())
        {
            // Reset ALL data and UI elements to empty state
            itemName = null;
            itemSprite = null;
            itemIcon.enabled = false;
            quantityText.enabled = false;
            itemDesc = null;
        }
        else
        {
            // Slot has items
            itemIcon.sprite = itemSprite;
            itemIcon.enabled = true;

            quantityText.text = quantity.ToString();
            // The text is enabled ONLY if the quantity is greater than 1 (a stack)
            quantityText.enabled = quantity > 1;
        }
    }


    public bool IsAvailable()
    {
        // A slot is available if it is completely empty OR if it has a non-full stack
        return IsEmpty() || quantity < maxStackSize;
    }

    public bool IsSameItem(Item item)
    {
        // Slot has the same item if it's not null, the names match, AND the stack isn't full.
        return item != null && !IsEmpty() && item.itemName == itemName && quantity < maxStackSize;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    public void ClearSlot()
    {
        itemName = null;
        quantity = 0;
        itemSprite = null;
        itemDesc = null;
        UpdateUI();
    }

    public bool DecreaseQuantity(int amount)
    {
        if (quantity - amount < 0)
        {
            return false;
        }
        quantity -= amount;
        if (quantity < 0)
        {
            quantity = 0;
        }
        UpdateUI();
        return true;
    }

    public void OnLeftClick()
    {
        if (thisItemSelected)
        {
            PlayerInventory.instance.UseConsumable(itemName, 1);
            return;
        }
        PlayerInventory.instance.DeselectAllSlots();
        selectedShader.SetActive(true);
        thisItemSelected = true;
        ItemDescriptionNameText.text = itemName;
        ItemDescriptionText.text = itemDesc;
        itemDescriptionIcon.sprite = itemSprite;
        if (itemSprite == null)
        {
            itemDescriptionIcon.enabled = false;
        }
        else
        {
            itemDescriptionIcon.enabled = true;
        }
    }

    public void OnRightClick()
    {
        return;
    }
}