using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class GunUIScript : MonoBehaviour
{
    [SerializeField] private Image shotgunImage, revolverImage;
    [SerializeField] private GameObject shotgunAmmoPrefab, revolverAmmoPrefab;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private Transform ammoContainer;

    [SerializeField] private EquipmentChangeEventSO equipEventSO;
    [SerializeField] private EquipmentChangeEventSO unequipEventSO;
    [SerializeField] private VoidEvents inventoryChangedEventSO;

    private BasePlayerMovement2D playerMovement;
    private int ammo, maxAmmo;

    void Awake()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SubscribeToInventoryEvents();

        // If player is already set, update UI immediately
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            HandlePlayerSet(GameManager.Instance.player);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Menu"))
        {
            this.gameObject.SetActive(false);
            SubscribeToInventoryEvents();
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
            SubscribeToInventoryEvents();
            DrawGunUI();
            DrawAmmoUI();
        }
    }

    private void SubscribeToInventoryEvents()
    {
        if (PlayerInventory.instance != null)
        {
            equipEventSO.onEventRaised.AddListener(UpdateGunUI);
            unequipEventSO.onEventRaised.AddListener(UpdateGunUI);
            inventoryChangedEventSO.onEventRaised.AddListener(UpdateAmmoText);
        }
    }

    void OnDestroy()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

        if (PlayerInventory.instance != null)
        {
            equipEventSO.onEventRaised.RemoveListener(UpdateGunUI);
            unequipEventSO.onEventRaised.RemoveListener(UpdateGunUI);
        }

        // Unsubscribe from ammo event when disabled
        if (playerMovement != null)
        {
            playerMovement.OnAmmoChanged -= UpdateAmmoUI;
        }
    }

    void UpdateAmmoText()
    {
        ammoText.text = 'x' + PlayerInventory.instance.HasItem("Ammo").ToString();
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
        UpdateAmmoText();
    }

    private void DrawGunUI()
    {
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
            ammoText.gameObject.SetActive(false);
            return;
        }

        // Only show gun UI if player is set
        if (playerMovement == null)
        {
            Debug.LogWarning("GunUI: playerMovement is null, cannot determine which gun to show");
            if (shotgunImage != null) shotgunImage.gameObject.SetActive(false);
            if (revolverImage != null) revolverImage.gameObject.SetActive(false);
            ammoText.gameObject.SetActive(false);
            return;
        }

        if (playerMovement is AliceMovement2D)
        {
            if (shotgunImage != null) shotgunImage.gameObject.SetActive(true);
            if (revolverImage != null) revolverImage.gameObject.SetActive(false);
            ammoText.gameObject.SetActive(true);
        }
        else
        {
            if (shotgunImage != null) shotgunImage.gameObject.SetActive(false);
            if (revolverImage != null) revolverImage.gameObject.SetActive(true);
            ammoText.gameObject.SetActive(true);
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
        Debug.Log("Ran UpdateGunUI");
        if (equipment != null)
        {
            if (equipment.equipmentType != EquipmentSO.EquipmentSlot.Ranged)
            {
                return; // Not a ranged weapon, ignore
            }
        }
        else
        {
            // Equipment is null - check if slot 3 is actually empty
            // This handles the unequip case
            if (PlayerInventory.instance != null
                && PlayerInventory.instance.equipmentSlots != null
                && PlayerInventory.instance.equipmentSlots.Length > 3
                && PlayerInventory.instance.equipmentSlots[3] != null
                && !PlayerInventory.instance.equipmentSlots[3].IsEmpty())
            {
                // Slot is not empty, so this unequip event is for a different slot
                return;
            }
        }

        // Force update UI immediately
        // Use a small delay to ensure inventory state is updated
        StartCoroutine(DelayedUIUpdate());
    }

    private IEnumerator DelayedUIUpdate()
    {
        // Wait one frame to ensure inventory state is fully updated
        yield return null;

        // Always update gun UI (will hide if no ranged weapon or player not set)
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