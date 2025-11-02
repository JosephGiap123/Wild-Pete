using UnityEngine;

public class Item : MonoBehaviour
{
	public string itemName;
	public Sprite defaultIcon;
	public Sprite icon = null;
	public int maxStackSize;
	public int quantity;
	public string itemDesc;

	[SerializeField] public ItemSO itemSO;
	private PlayerInventory inventoryManager;

	void Start()
	{
		inventoryManager = PlayerInventory.instance;
		if (itemSO != null)
		{
			itemName = itemSO.itemName;
			icon = itemSO.icon;
			maxStackSize = itemSO.maxStackSize;
			itemDesc = itemSO.itemDesc;
			quantity = itemSO.quantity;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			inventoryManager.AddItem(this);
			Destroy(gameObject);
		}
	}
}