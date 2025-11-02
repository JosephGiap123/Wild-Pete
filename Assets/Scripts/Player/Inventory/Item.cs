using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Item : MonoBehaviour
{
	bool isPickable = false;
	public string itemName;
	public Sprite defaultIcon;
	public Sprite icon = null;
	public Sprite dropIcon = null;
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
			dropIcon = itemSO.dropIcon;
		}
		StartCoroutine(WaitToEnablePickable(5.0f));
	}

	protected IEnumerator WaitToEnablePickable(float timeToPickUp)
	{
		yield return new WaitForSeconds(timeToPickUp);
		isPickable = true;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player") && isPickable)
		{
			inventoryManager.AddItem(this);
			Destroy(gameObject);
		}
	}
}