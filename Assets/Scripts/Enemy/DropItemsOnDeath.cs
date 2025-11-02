using System;
using UnityEngine;

public class DropItemsOnDeath : MonoBehaviour
{
    public GameObject itemPrefab;
    [SerializeField] public ItemSO[] items;
    [SerializeField] public float[] itemDropChances;
    //floats are between 0 - 100
    public bool DropItem(float chance)
    {
        float roll = UnityEngine.Random.Range(0f, 100f);
        return roll <= chance;
    }
    public void DropItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (DropItem(itemDropChances[i]))
            {
                GameObject itemToDrop = Instantiate(itemPrefab, transform.position, Quaternion.identity);
                itemToDrop.GetComponent<Item>().Initialize(new(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(2f, 5f)), items[i]);
            }
        }
    }
}
