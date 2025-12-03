using UnityEngine;

public class CheatManager : MonoBehaviour
{
    public static CheatManager Instance { get; private set; }

    [Tooltip("If true, player takes no damage.")]
    public bool invulnerable = false;
    [SerializeField] private EquipmentSO adminBoots;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ToggleInvulnerability()
    {
        invulnerable = !invulnerable;
        if (invulnerable)
        {
            PlayerInventory.instance.AddItemFromItemSO(adminBoots, 1);
        }
        Debug.Log($"CheatMode Invulnerable = {invulnerable}");
    }
}

