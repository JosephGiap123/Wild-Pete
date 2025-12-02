using System;
using UnityEngine;

public class DropItemsOnDeath : MonoBehaviour
{
    public GameObject itemPrefab;
    [SerializeField] public ItemSO[] items;
    [SerializeField] public ItemSO[] aliceSpecificItems;
    [SerializeField] public ItemSO[] peteSpecificItems;
    [SerializeField] public float[] itemDropChances;
    [SerializeField] public float[] aliceSpecificItemDropChances;
    [SerializeField] public float[] peteSpecificItemDropChances;
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
        if (GameManager.Instance.selectedCharacter == GameManager.Characters.Alice)
        {
            for (int i = 0; i < aliceSpecificItems.Length; i++)
            {
                if (DropItem(aliceSpecificItemDropChances[i]))
                {
                    GameObject itemToDrop = Instantiate(itemPrefab, transform.position, Quaternion.identity);
                    itemToDrop.GetComponent<Item>().Initialize(new(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(2f, 5f)), aliceSpecificItems[i]);
                }
            }
        }
        if (GameManager.Instance.selectedCharacter == GameManager.Characters.Pete)
        {
            for (int i = 0; i < peteSpecificItems.Length; i++)
            {
                if (DropItem(peteSpecificItemDropChances[i]))
                {
                    GameObject itemToDrop = Instantiate(itemPrefab, transform.position, Quaternion.identity);
                    itemToDrop.GetComponent<Item>().Initialize(new(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(2f, 5f)), peteSpecificItems[i]);
                }
            }
        }
    }
}
