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

	public ItemSO itemSO;
	private PlayerInventory inventoryManager;
	private Rigidbody2D rb;
	PhysicalItemModel physicalItemModel;

	void Awake()
	{
		physicalItemModel = GetComponent<PhysicalItemModel>();
		rb = GetComponent<Rigidbody2D>();
	}
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
			physicalItemModel.Load();
		}
		StartCoroutine(WaitToEnablePickable(5.0f));
	}

	public void Initialize(Vector2 beginningVelocity, ItemSO itemData)
	{
		rb.linearVelocity = beginningVelocity;
		itemSO = itemData;
		itemName = itemSO.itemName;
		icon = itemSO.icon;
		maxStackSize = itemSO.maxStackSize;
		itemDesc = itemSO.itemDesc;
		quantity = itemSO.quantity;
		dropIcon = itemSO.dropIcon;
		physicalItemModel.Load();
	}

	protected IEnumerator WaitToEnablePickable(float timeToPickUp)
	{
		yield return new WaitForSeconds(timeToPickUp);
		isPickable = true;
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag("Player") && isPickable)
		{
			Debug.Log("picking item up");
			inventoryManager.AddItem(this);
			Destroy(gameObject);
		}
	}
}