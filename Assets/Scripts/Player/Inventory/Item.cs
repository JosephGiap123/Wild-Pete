using UnityEngine;

public class Item : MonoBehaviour
{
	[SerializeField] public string itemName;
	[SerializeField] public Sprite icon;
	[SerializeField] public int maxStackSize;
	[SerializeField] public int quantity;
	[TextArea] public string itemDesc;

	private PlayerInventory inventoryManager;

	void Start()
	{
		inventoryManager = PlayerInventory.instance;
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