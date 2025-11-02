using UnityEngine;

public class PhysicalItemModel : MonoBehaviour
{
    private ItemSO itemSO;

    private Item itemComponent;

    public void Load()
    {
        itemComponent = GetComponent<Item>();
        itemSO = itemComponent.itemSO;
        GetComponentInChildren<SpriteRenderer>().sprite = itemSO.dropIcon;
        Debug.Log(itemSO.dropIcon);
    }

}
