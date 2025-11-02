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
        GetComponentInChildren<SpriteRenderer>().sprite = itemSO.icon; //fine. for now, but will need another model later due to being 32x32, being far too big.
    }

}
