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
    public Sprite dropSprite = null;
    [TextArea] public string itemDesc = "";
    public int maxStackSize = 1;

    // UI References
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private GameObject itemPrefab;

    private ItemSO itemSO;
    public GameObject selectedShader;
    public bool thisItemSelected = false;

    // Call UpdateUI() when the script starts to ensure the UI matches the data.
    void Start()
    {
        // This ensures the icon and text are correctly hidden when the game starts.
        UpdateUI();
    }
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
            itemSO = item.itemSO;
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

    public void UpdateUI()
    {
        // Safety check for UI references
        if (itemIcon == null || quantityText == null)
        {
            Debug.LogWarning("ItemSlot: itemIcon or quantityText is not assigned! Cannot update UI.");
            return;
        }

        // Check if the slot is empty based on quantity or name
        if (quantity <= 0 || IsEmpty())
        {
            // Reset UI elements to empty state (don't call ClearSlot to avoid recursion)
            itemIcon.sprite = defaultIcon;
            itemIcon.enabled = false;
            quantityText.text = "0";
            quantityText.enabled = false;
        }
        else
        {
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
        itemSO = null;
        itemName = null;
        maxStackSize = 1;
        quantity = 0;
        itemSprite = defaultIcon;
        itemDesc = null;
        dropSprite = defaultIcon;
        PlayerInventory.instance.ClearDescriptionPanel();
        
        // Update UI to reflect cleared state
        UpdateUI();
    }

    // Directly restores slot data from checkpoint. Used for restoring saved inventory state.
    public void RestoreSlot(ItemSO itemData, int savedQuantity)
    {
        if (itemData == null || savedQuantity <= 0)
        {
            ClearSlot();
            return;
        }

        itemSO = itemData;
        itemName = itemData.itemName;
        itemSprite = itemData.icon;
        maxStackSize = itemData.maxStackSize;
        itemDesc = itemData.itemDesc;
        quantity = savedQuantity;
        dropSprite = itemData.dropIcon;

        UpdateUI();
    }

    public bool DecreaseQuantity(int amount)
    {
        if (quantity <= 0)
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
        if (PlayerInventory.instance == null)
        {
            Debug.LogError("ItemSlot: PlayerInventory.instance is null! Cannot process click.");
            return;
        }

        if (thisItemSelected)
        {
            // Find this slot's index in the inventory array
            int slotIndex = -1;
            for (int i = 0; i < PlayerInventory.instance.itemSlots.Length; i++)
            {
                if (PlayerInventory.instance.itemSlots[i] == this)
                {
                    slotIndex = i;
                    break;
                }
            }

            if (slotIndex >= 0)
            {
                PlayerInventory.instance.UseConsumable(itemName, slotIndex);
            }
            else
            {
                Debug.LogWarning("ItemSlot: Could not find this slot in PlayerInventory.itemSlots array!");
            }
            return;
        }

        PlayerInventory.instance.DeselectAllSlots();

        if (selectedShader != null)
        {
            selectedShader.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ItemSlot: selectedShader is not assigned!");
        }

        thisItemSelected = true;
        PlayerInventory.instance.FillDescriptionUI(itemName, itemDesc, itemSprite);
    }

    public void OnRightClick()
    {
        if (IsEmpty())
        {
            return;
        }
        PlayerOrientationPosition playerOrPos = GameManager.Instance.player.GetComponent<BasePlayerMovement2D>().GetPlayerOrientPosition();
        Transform playerPos = playerOrPos.position;
        bool facingRight = playerOrPos.isFacingRight;
        Vector3 newPosition = new(playerPos.position.x, playerPos.position.y, 0f);
        GameObject itemToDrop = Instantiate(itemPrefab, newPosition, facingRight ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0));

        itemToDrop.GetComponent<Item>().Initialize(new(3f * (facingRight ? 1f : -1f), 4f), itemSO);
        DecreaseQuantity(1);
        // itemToDrop.GetComponent<PhysicalItemModel>().Load();
        PlayerInventory.instance.DeselectAllSlots();
        PlayerInventory.instance.ClearDescriptionPanel();
        return;
    }
}