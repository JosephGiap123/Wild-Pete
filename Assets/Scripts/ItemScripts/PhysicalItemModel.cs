using UnityEngine;

public class PhysicalItemModel : MonoBehaviour
{
    private ItemSO itemSO;

    private Item itemComponent;

    void Start()
    {
        itemComponent = GetComponent<Item>();
        if (itemComponent != null)
        {
            itemSO = itemComponent.itemSO;
        }
        GetComponentInChildren<SpriteRenderer>().sprite = itemSO.dropIcon;
    }

}
