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
			// Only set quantity from itemSO if it hasn't been set yet (e.g., by Initialize)
			// This prevents Start() from overwriting a restored quantity
			if (quantity == 0)
			{
				quantity = itemSO.quantity;
			}
			dropIcon = itemSO.dropIcon;
			physicalItemModel.Load();
		}
		
		// Register with checkpoint system
		if (CheckpointManager.Instance != null)
		{
			CheckpointManager.Instance.RegisterItem(this);
		}
		
		StartCoroutine(WaitToEnablePickable(1.0f));
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
		
		// Ensure inventoryManager is set (in case Initialize is called before Start)
		if (inventoryManager == null)
		{
			inventoryManager = PlayerInventory.instance;
		}
		
		// Ensure WaitToEnablePickable coroutine is started (in case Initialize is called before Start)
		// Stop any existing coroutine first to avoid duplicates
		StopAllCoroutines();
		StartCoroutine(WaitToEnablePickable(1.0f));
		
		// Register with checkpoint system (in case Initialize is called after Start)
		if (CheckpointManager.Instance != null)
		{
			CheckpointManager.Instance.RegisterItem(this);
		}
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
			
			// Unregister from checkpoint system before destroying
			if (CheckpointManager.Instance != null)
			{
				CheckpointManager.Instance.UnregisterItem(this);
			}
			
			Destroy(gameObject);
		}
	}
	
	void OnDestroy()
	{
		// Safety: unregister from checkpoint system if still registered
		if (CheckpointManager.Instance != null)
		{
			CheckpointManager.Instance.UnregisterItem(this);
		}
	}
}