using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [System.Serializable]
    public class InventorySlotData
    {
        public string itemName;
        public int quantity;

        public InventorySlotData(string name, int qty)
        {
            itemName = name;
            quantity = qty;
        }
    }

    [System.Serializable]
    public class ItemData
    {
        public Vector2 position;
        public string itemName;
        public int quantity;

        public ItemData(Vector2 pos, string name, int qty)
        {
            position = pos;
            itemName = name;
            quantity = qty;
        }
    }

    [System.Serializable]
    public class CheckpointData
    {
        public Vector2 position;
        public string sceneName;
        public Dictionary<int, bool> enemyStates = new Dictionary<int, bool>(); // instanceID -> isAlive
        public Dictionary<int, Vector2> enemyPositions = new Dictionary<int, Vector2>(); // instanceID -> position (only for alive enemies)
        public Dictionary<int, bool> enemyFacing = new Dictionary<int, bool>(); // instanceID -> isFacingRight (only for alive enemies)
        public Dictionary<int, bool> staticStates = new Dictionary<int, bool>(); // instanceID -> isAlive
        public Dictionary<int, Vector2> staticPositions = new Dictionary<int, Vector2>(); // instanceID -> position (only for alive statics)
        public int playerHealth;
        public int playerAmmo;
        public int playerMaxAmmo;
        public List<InventorySlotData> inventorySlots = new List<InventorySlotData>(); // Inventory state
        public List<ItemData> items = new List<ItemData>(); // Dropped items on the map (both placed and dropped)
    }

    private CheckpointData currentCheckpoint;
    private Dictionary<int, EnemyBase> trackedEnemies = new Dictionary<int, EnemyBase>();
    private Dictionary<int, BreakableStatics> trackedStatics = new Dictionary<int, BreakableStatics>();
    private Dictionary<int, Item> trackedItems = new Dictionary<int, Item>(); // instanceID -> Item

    public static event Action<Vector2> OnCheckpointSaved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// Saves a checkpoint at the given position and captures current game state.
    public void SaveCheckpoint(Vector2 position)
    {
        currentCheckpoint = new CheckpointData
        {
            position = position,
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        };

        // Capture enemy states, positions, and facing directions
        currentCheckpoint.enemyStates.Clear();
        currentCheckpoint.enemyPositions.Clear();
        currentCheckpoint.enemyFacing.Clear();
        foreach (var kvp in trackedEnemies)
        {
            if (kvp.Value != null)
            {
                bool isAlive = kvp.Value.gameObject.activeSelf && kvp.Value.IsAlive();
                currentCheckpoint.enemyStates[kvp.Key] = isAlive;

                // Save position and facing direction if enemy is alive
                if (isAlive)
                {
                    currentCheckpoint.enemyPositions[kvp.Key] = kvp.Value.transform.position;
                    currentCheckpoint.enemyFacing[kvp.Key] = kvp.Value.IsFacingRight;
                }
            }
        }

        // Capture static states and positions
        currentCheckpoint.staticStates.Clear();
        currentCheckpoint.staticPositions.Clear();
        foreach (var kvp in trackedStatics)
        {
            if (kvp.Value != null)
            {
                bool isAlive = kvp.Value.gameObject.activeSelf;
                currentCheckpoint.staticStates[kvp.Key] = isAlive;

                // Save position if static is alive
                if (isAlive)
                {
                    currentCheckpoint.staticPositions[kvp.Key] = kvp.Value.transform.position;
                }
            }
        }

        // Capture player state
        GameObject player = GameManager.Instance?.player;
        if (player != null)
        {
            BasePlayerMovement2D playerScript = player.GetComponent<BasePlayerMovement2D>();
            if (playerScript != null)
            {
                currentCheckpoint.playerHealth = HealthManager.instance.GetCurrentHealth();
                currentCheckpoint.playerAmmo = playerScript.ammoCount;
                currentCheckpoint.playerMaxAmmo = playerScript.maxAmmo;
            }
        }

        // Capture inventory state
        currentCheckpoint.inventorySlots.Clear();
        if (PlayerInventory.instance != null && PlayerInventory.instance.itemSlots != null)
        {
            foreach (ItemSlot slot in PlayerInventory.instance.itemSlots)
            {
                if (slot != null && !slot.IsEmpty() && slot.quantity > 0)
                {
                    currentCheckpoint.inventorySlots.Add(new InventorySlotData(slot.itemName, slot.quantity));
                }
                else
                {
                    // Save empty slot
                    currentCheckpoint.inventorySlots.Add(new InventorySlotData(null, 0));
                }
            }
        }

        // Capture dropped items on the map (both placed in scene and dropped)
        currentCheckpoint.items.Clear();
        foreach (var kvp in trackedItems)
        {
            if (kvp.Value != null && kvp.Value.gameObject.activeSelf)
            {
                // Item is still on the map
                currentCheckpoint.items.Add(new ItemData(
                    kvp.Value.transform.position,
                    kvp.Value.itemName,
                    kvp.Value.quantity
                ));
            }
        }

        GameRestartManager.checkPointLocation = position;
        OnCheckpointSaved?.Invoke(position);
        Debug.Log($"Checkpoint saved at {position}");
    }
    /// Restores game state to the last saved checkpoint.
    public void RestoreCheckpoint()
    {
        if (currentCheckpoint == null)
        {
            Debug.LogWarning("No checkpoint saved! Cannot restore.");
            return;
        }

        // Clear inventory first before restoring
        if (PlayerInventory.instance != null)
        {
            PlayerInventory.instance.ClearInventory();
        }

        // Delete all current items on the map (both placed and dropped)
        Item[] allItems = FindObjectsByType<Item>(FindObjectsSortMode.None);
        foreach (Item item in allItems)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        trackedItems.Clear();

        // Restore enemy states, positions, and facing directions
        foreach (var kvp in trackedEnemies)
        {
            if (kvp.Value != null && currentCheckpoint.enemyStates.ContainsKey(kvp.Key))
            {
                bool wasAlive = currentCheckpoint.enemyStates[kvp.Key];
                if (wasAlive)
                {
                    // Enemy was alive at checkpoint - respawn it at saved position and facing direction
                    // This restores enemies that were alive when checkpoint was set,
                    // even if they died after the checkpoint but before player death
                    Vector2 savedPosition = currentCheckpoint.enemyPositions.ContainsKey(kvp.Key)
                        ? currentCheckpoint.enemyPositions[kvp.Key]
                        : kvp.Value.transform.position; // Fallback to current position if not saved
                    bool savedFacing = currentCheckpoint.enemyFacing.ContainsKey(kvp.Key)
                        ? currentCheckpoint.enemyFacing[kvp.Key]
                        : kvp.Value.IsFacingRight; // Fallback to current facing if not saved
                    kvp.Value.Respawn(savedPosition, savedFacing);
                }
                else
                {
                    // Enemy was dead at checkpoint - keep it dead
                    kvp.Value.gameObject.SetActive(false);
                }
            }
        }

        // Restore static states and positions
        foreach (var kvp in trackedStatics)
        {
            if (kvp.Value != null && currentCheckpoint.staticStates.ContainsKey(kvp.Key))
            {
                bool wasAlive = currentCheckpoint.staticStates[kvp.Key];
                if (wasAlive)
                {
                    // Static was alive at checkpoint - restore it at saved position
                    Vector2 savedPosition = currentCheckpoint.staticPositions.ContainsKey(kvp.Key)
                        ? currentCheckpoint.staticPositions[kvp.Key]
                        : kvp.Value.transform.position; // Fallback to current position if not saved
                    kvp.Value.Restore(savedPosition);
                }
                else
                {
                    // Static was broken at checkpoint - keep it broken
                    kvp.Value.gameObject.SetActive(false);
                }
            }
        }

        // Restore inventory state
        if (PlayerInventory.instance != null && currentCheckpoint.inventorySlots != null && currentCheckpoint.inventorySlots.Count > 0)
        {
            PlayerInventory.instance.RestoreInventory(currentCheckpoint.inventorySlots);
        }

        // Restore dropped items on the map
        if (currentCheckpoint.items != null && currentCheckpoint.items.Count > 0)
        {
            RestoreItems(currentCheckpoint.items);
        }

        Debug.Log($"Checkpoint restored at {currentCheckpoint.position}");
    }

    /// Registers an enemy to be tracked by the checkpoint system.
    /// Uses the GameObject's instance ID as the unique identifier.
    public void RegisterEnemy(EnemyBase enemy)
    {
        if (enemy != null)
        {
            int instanceID = enemy.gameObject.GetInstanceID();
            trackedEnemies[instanceID] = enemy;
        }
    }

    /// Registers a static to be tracked by the checkpoint system.
    /// Uses the GameObject's instance ID as the unique identifier.
    public void RegisterStatic(BreakableStatics statics)
    {
        if (statics != null)
        {
            int instanceID = statics.gameObject.GetInstanceID();
            trackedStatics[instanceID] = statics;
        }
    }


    /// Unregisters an enemy from tracking.

    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (enemy != null)
        {
            trackedEnemies.Remove(enemy.gameObject.GetInstanceID());
        }
    }


    /// Unregisters a static from tracking.

    public void UnregisterStatic(BreakableStatics statics)
    {
        if (statics != null)
        {
            trackedStatics.Remove(statics.gameObject.GetInstanceID());
        }
    }


    /// Gets the current checkpoint position, or Vector2.zero if none saved.

    public Vector2 GetCheckpointPosition()
    {
        return currentCheckpoint?.position ?? Vector2.zero;
    }


    /// Gets the current checkpoint data.

    public CheckpointData GetCheckpointData()
    {
        return currentCheckpoint;
    }


    /// Checks if a checkpoint has been saved.

    public bool HasCheckpoint()
    {
        return currentCheckpoint != null;
    }

    /// <summary>
    /// Registers an item to be tracked by the checkpoint system.
    /// Uses the GameObject's instance ID as the unique identifier.
    /// </summary>
    public void RegisterItem(Item item)
    {
        if (item != null)
        {
            int instanceID = item.gameObject.GetInstanceID();
            trackedItems[instanceID] = item;
        }
    }

    /// <summary>
    /// Unregisters an item from tracking.
    /// </summary>
    public void UnregisterItem(Item item)
    {
        if (item != null)
        {
            trackedItems.Remove(item.gameObject.GetInstanceID());
        }
    }

    /// <summary>
    /// Restores items on the map from checkpoint data.
    /// Uses PlayerInventory's item atlas (itemSOs) to find item data.
    /// </summary>
    private void RestoreItems(List<ItemData> savedItems)
    {
        if (savedItems == null || savedItems.Count == 0) return;

        // Get item prefab from PlayerInventory's ItemSlot (the atlas)
        GameObject itemPrefab = null;
        if (PlayerInventory.instance != null && PlayerInventory.instance.itemSlots != null)
        {
            foreach (ItemSlot slot in PlayerInventory.instance.itemSlots)
            {
                if (slot != null)
                {
                    // Use reflection to get the private itemPrefab field
                    var itemPrefabField = typeof(ItemSlot).GetField("itemPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (itemPrefabField != null)
                    {
                        itemPrefab = itemPrefabField.GetValue(slot) as GameObject;
                        if (itemPrefab != null) break;
                    }
                }
            }
        }

        // Fallback: try to get from DropItemsOnDeath if not found in ItemSlot
        if (itemPrefab == null)
        {
            DropItemsOnDeath dropScript = FindFirstObjectByType<DropItemsOnDeath>();
            if (dropScript != null && dropScript.itemPrefab != null)
            {
                itemPrefab = dropScript.itemPrefab;
            }
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning("CheckpointManager: Could not find item prefab to restore items!");
            return;
        }

        // Get ItemSOs from PlayerInventory (the atlas)
        ItemSO[] allItemSOs = null;
        ConsumableSO[] allConsumableSOs = null;

        if (PlayerInventory.instance != null)
        {
            allItemSOs = PlayerInventory.instance.itemSOs;
            allConsumableSOs = PlayerInventory.instance.consumableSOs;
        }

        // Restore each item
        foreach (ItemData itemData in savedItems)
        {
            if (string.IsNullOrEmpty(itemData.itemName) || itemData.quantity <= 0) continue;

            // Find the ItemSO by name from PlayerInventory's atlas
            ItemSO foundItemSO = null;

            // Check consumables first
            if (allConsumableSOs != null)
            {
                foreach (ConsumableSO consumableSO in allConsumableSOs)
                {
                    if (consumableSO != null && consumableSO.itemName == itemData.itemName)
                    {
                        foundItemSO = consumableSO;
                        break;
                    }
                }
            }

            // If not found, check regular items
            if (foundItemSO == null && allItemSOs != null)
            {
                foreach (ItemSO itemSO in allItemSOs)
                {
                    if (itemSO != null && itemSO.itemName == itemData.itemName)
                    {
                        foundItemSO = itemSO;
                        break;
                    }
                }
            }

            if (foundItemSO != null)
            {
                // Instantiate item at saved position
                GameObject itemObj = Instantiate(itemPrefab, itemData.position, Quaternion.identity);
                Item item = itemObj.GetComponent<Item>();
                if (item != null)
                {
                    // Initialize item with saved data (no velocity since it's placed, not dropped)
                    item.Initialize(Vector2.zero, foundItemSO);
                    item.quantity = itemData.quantity;

                    // Register with checkpoint system
                    RegisterItem(item);
                }
            }
            else
            {
                Debug.LogWarning($"CheckpointManager: Could not find ItemSO for item '{itemData.itemName}' in PlayerInventory atlas");
            }
        }
    }
}

