using UnityEngine;
using UnityEngine.UI;

public class GunUIScript : MonoBehaviour
{
    [SerializeField] private Image shotgunImage, revolverImage;
    [SerializeField] private GameObject shotgunAmmoPrefab, revolverAmmoPrefab;
    [SerializeField] private Transform ammoContainer;

    private BasePlayerMovement2D playerMovement;
    private int ammo, maxAmmo;

    void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
        PlayerInventory.instance.OnEquipmentEquippedEvent += UpdateGunUI;
        PlayerInventory.instance.OnEquipmentUnequippedEvent += UpdateGunUI;
    }

    void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;

        PlayerInventory.instance.OnEquipmentEquippedEvent -= UpdateGunUI;
        PlayerInventory.instance.OnEquipmentUnequippedEvent -= UpdateGunUI;
        // Unsubscribe from ammo event when disabled
        if (playerMovement != null)
        {
            playerMovement.OnAmmoChanged -= UpdateAmmoUI;
        }
    }

    private void HandlePlayerSet(GameObject player)
    {
        // Unsubscribe from old player if exists
        if (playerMovement != null)
        {
            playerMovement.OnAmmoChanged -= UpdateAmmoUI;
        }

        playerMovement = player.GetComponent<BasePlayerMovement2D>();
        if (playerMovement == null)
        {
            Debug.LogError("GunUIScript: Player does not have BasePlayerMovement2D!");
            return;
        }

        ammo = playerMovement.ammoCount;
        maxAmmo = playerMovement.maxAmmo;

        // Subscribe to ammo change event
        playerMovement.OnAmmoChanged += UpdateAmmoUI;

        // Always update UI when player is set (in case equipment was equipped before player was set)
        DrawGunUI();
        DrawAmmoUI();
    }

    private void DrawGunUI()
    {
        // Check if slot 3 (Ranged) exists and has equipment
        bool hasRangedWeapon = PlayerInventory.instance != null
            && PlayerInventory.instance.equipmentSlots != null
            && PlayerInventory.instance.equipmentSlots.Length > 3
            && PlayerInventory.instance.equipmentSlots[3] != null
            && !PlayerInventory.instance.equipmentSlots[3].IsEmpty();

        if (!hasRangedWeapon)
        {
            Debug.Log("GunUI: Equipment slot 3 is empty or null");
            if (shotgunImage != null) shotgunImage.gameObject.SetActive(false);
            if (revolverImage != null) revolverImage.gameObject.SetActive(false);
            return;
        }

        // Only show gun UI if player is set
        if (playerMovement == null)
        {
            Debug.LogWarning("GunUI: playerMovement is null, cannot determine which gun to show");
            if (shotgunImage != null) shotgunImage.gameObject.SetActive(false);
            if (revolverImage != null) revolverImage.gameObject.SetActive(false);
            return;
        }

        if (playerMovement is AliceMovement2D)
        {
            if (shotgunImage != null) shotgunImage.gameObject.SetActive(true);
            if (revolverImage != null) revolverImage.gameObject.SetActive(false);
        }
        else
        {
            if (shotgunImage != null) shotgunImage.gameObject.SetActive(false);
            if (revolverImage != null) revolverImage.gameObject.SetActive(true);
        }
    }


    private void DrawAmmoUI()
    {
        Transform parent = ammoContainer != null ? ammoContainer : transform;

        // Clear existing ammo icons
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.name.StartsWith("AmmoIcon_"))
            {
                Destroy(child.gameObject);
            }
        }

        // Check if slot 3 (Ranged) exists and has equipment
        bool hasRangedWeapon = PlayerInventory.instance != null
            && PlayerInventory.instance.equipmentSlots != null
            && PlayerInventory.instance.equipmentSlots.Length > 3
            && PlayerInventory.instance.equipmentSlots[3] != null
            && !PlayerInventory.instance.equipmentSlots[3].IsEmpty();

        if (!hasRangedWeapon)
        {
            return;
        }

        // Only draw ammo UI if player is set
        if (playerMovement == null)
        {
            return;
        }

        GameObject prefab = (playerMovement is AliceMovement2D)
            ? shotgunAmmoPrefab
            : revolverAmmoPrefab;

        for (int i = 0; i < maxAmmo; i++)
        {
            GameObject icon = Instantiate(prefab, parent);
            icon.name = $"AmmoIcon_{i}";

            DynamicAmmoUI ammoUI = icon.GetComponent<DynamicAmmoUI>();
            if (ammoUI != null)
            {
                int spriteIndex = (i < ammo) ? 1 : 0;
                ammoUI.changeSprite(spriteIndex);
            }
        }
    }

    // Event handler - called when ammo changes

    private void UpdateGunUI(EquipmentSO equipment)
    {
        // Only update if this is a ranged weapon (slot 3)
        if (equipment != null && equipment.equipmentType != EquipmentSO.EquipmentSlot.Ranged)
        {
            return; // Not a ranged weapon, ignore
        }

        DrawGunUI();
        
        // Only update ammo UI if player is set
        if (playerMovement != null)
        {
            UpdateAmmoUI(playerMovement.ammoCount, playerMovement.maxAmmo);
        }
    }
    private void UpdateAmmoUI(int currentAmmo, int maximum)
    {
        ammo = currentAmmo;
        maxAmmo = maximum;
        DrawAmmoUI();
    }
}