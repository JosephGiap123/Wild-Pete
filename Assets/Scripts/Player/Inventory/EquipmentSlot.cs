using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler
{
    // Equipment Data
    private EquipmentSO equippedItem = null;
    
    // The type of equipment this slot accepts
    public EquipmentSO.EquipmentSlot slotType;
    
    // UI References
    [SerializeField] private Image itemIcon;
    [SerializeField] private Sprite defaultIcon;
    public GameObject selectedShader;
    public bool thisItemSelected = false;
    
    // Double-click detection
    private float lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f;
    
    void Start()
    {
        UpdateUI();
    }
    
    public bool IsEmpty()
    {
        return equippedItem == null;
    }
    
    public EquipmentSO GetEquippedItem()
    {
        return equippedItem;
    }
    
    public void EquipItem(EquipmentSO equipment)
    {
        if (equipment == null)
        {
            Debug.LogWarning("EquipmentSlot: Cannot equip null equipment!");
            return;
        }
        
        if (equipment.equipmentType != slotType)
        {
            Debug.LogWarning($"EquipmentSlot: Cannot equip {equipment.equipmentType} equipment in {slotType} slot!");
            return;
        }
        
        // If there's already an item equipped, unequip it first
        if (equippedItem != null)
        {
            UnequipItem();
        }
        
        equippedItem = equipment;
        UpdateUI();
        
        // Notify PlayerInventory to apply stat bonuses
        if (PlayerInventory.instance != null)
        {
            PlayerInventory.instance.OnEquipmentEquipped(equipment);
        }
    }
    
    public void UnequipItem()
    {
        if (equippedItem == null)
        {
            return;
        }
        
        EquipmentSO itemToUnequip = equippedItem;
        equippedItem = null;
        UpdateUI();
        
        // Notify PlayerInventory to remove stat bonuses
        if (PlayerInventory.instance != null)
        {
            PlayerInventory.instance.OnEquipmentUnequipped(itemToUnequip);
            
            // Try to add the unequipped item back to inventory
            PlayerInventory.instance.AddEquipmentToInventory(itemToUnequip);
        }
        
        // Deselect if this slot was selected
        if (thisItemSelected)
        {
            thisItemSelected = false;
            if (selectedShader != null)
            {
                selectedShader.SetActive(false);
            }
            PlayerInventory.instance.ClearDescriptionPanel();
        }
    }
    
    public void UpdateUI()
    {
        if (itemIcon == null)
        {
            Debug.LogWarning("EquipmentSlot: itemIcon is not assigned! Cannot update UI.");
            return;
        }
        
        if (IsEmpty())
        {
            itemIcon.sprite = defaultIcon;
            itemIcon.enabled = false;
        }
        else
        {
            itemIcon.sprite = equippedItem.icon;
            itemIcon.enabled = true;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
    }
    
    private void OnLeftClick()
    {
        if (PlayerInventory.instance == null)
        {
            Debug.LogError("EquipmentSlot: PlayerInventory.instance is null!");
            return;
        }
        
        // Handle double-click to unequip
        float currentTime = Time.time;
        if (currentTime - lastClickTime < DOUBLE_CLICK_TIME && !IsEmpty())
        {
            // Double-click detected - unequip
            UnequipItem();
            lastClickTime = 0f; // Reset to prevent triple-click issues
            return;
        }
        lastClickTime = currentTime;
        
        // Single click - select and show description
        PlayerInventory.instance.DeselectAllSlots();
        PlayerInventory.instance.DeselectAllEquipmentSlots();
        
        if (selectedShader != null)
        {
            selectedShader.SetActive(true);
        }
        
        thisItemSelected = true;
        
        if (!IsEmpty())
        {
            PlayerInventory.instance.FillDescriptionUI(
                equippedItem.itemName, 
                equippedItem.itemDesc, 
                equippedItem.icon
            );
        }
        else
        {
            PlayerInventory.instance.ClearDescriptionPanel();
        }
    }
    
    public void ClearSlot()
    {
        if (equippedItem != null)
        {
            EquipmentSO itemToRemove = equippedItem;
            equippedItem = null;
            
            if (PlayerInventory.instance != null)
            {
                PlayerInventory.instance.OnEquipmentUnequipped(itemToRemove);
            }
        }
        
        UpdateUI();
        
        if (thisItemSelected)
        {
            thisItemSelected = false;
            if (selectedShader != null)
            {
                selectedShader.SetActive(false);
            }
        }
    }
}

