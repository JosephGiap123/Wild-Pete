using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int HasItem(string itemName)
    {
        // Stub implementation - return 0 if not found
        return 0;
    }

    public void UseItem(string itemName, int amount)
    {
        // Stub implementation
    }
}

