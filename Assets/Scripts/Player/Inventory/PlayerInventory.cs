using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int ammoCount = 0;
    public Item[] inventory;
    public void Awake()
    {
        inventory = new Item[10]; // inventory size
    }
    public void AddAmmo(int amount)
    {
        ammoCount += amount;
        Debug.Log("Added " + amount + " ammo. Total ammo: " + ammoCount);
    }

    public void RemoveAmmo(int amount)
    {
        ammoCount = Mathf.Max(0, ammoCount - amount);
        Debug.Log("Removed " + amount + " ammo. Total ammo: " + ammoCount);
    }


}
