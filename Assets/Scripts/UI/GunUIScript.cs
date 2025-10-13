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
    }
    
    void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
        
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
        
        DrawGunUI();
        DrawAmmoUI();
    }
    
    private void DrawGunUI()
    {
        if (playerMovement is AliceMovement2D)
        {
            shotgunImage.gameObject.SetActive(true);
            revolverImage.gameObject.SetActive(false);
        }
        else
        {
            shotgunImage.gameObject.SetActive(false);
            revolverImage.gameObject.SetActive(true);
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
    private void UpdateAmmoUI(int currentAmmo, int maximum)
    {
        ammo = currentAmmo;
        maxAmmo = maximum;
        DrawAmmoUI();
    }
}