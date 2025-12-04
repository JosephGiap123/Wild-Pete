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
	public ItemPickUpEvent itemPickUpEvent;
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
		
		// Check for player already in trigger when item becomes pickable
		CheckOverlappingColliders();
		// Also check after a frame delay to catch any physics updates
		StartCoroutine(CheckOverlapAfterFrame());
	}
	
	private IEnumerator CheckOverlapAfterFrame()
	{
		// Wait for physics to update
		yield return null;
		if (isPickable)
		{
			CheckOverlappingColliders();
		}
	}
	
	private void CheckOverlappingColliders()
	{
		if (!isPickable) return;
		
		Collider2D itemCollider = GetComponent<Collider2D>();
		if (itemCollider == null) return;
		
		// Use OverlapCollider to find all colliders already inside
		ContactFilter2D filter = new ContactFilter2D();
		filter.NoFilter(); // Check all layers
		filter.useTriggers = true; // Include trigger colliders
		
		List<Collider2D> overlappingColliders = new List<Collider2D>();
		itemCollider.Overlap(filter, overlappingColliders);
		
		// Process each overlapping collider as if it just entered
		foreach (Collider2D other in overlappingColliders)
		{
			if (other != null && other != itemCollider && other.gameObject.CompareTag("Player"))
			{
				// Simulate OnTriggerEnter2D for player already inside
				OnTriggerEnter2D(other);
			}
		}
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag("Player") && isPickable)
		{
			Debug.Log("picking item up");
			int quantityBefore = quantity;
			bool fullyAdded = inventoryManager.AddItem(this);

			if (fullyAdded)
			{
				// Item was fully added to inventory - destroy it
			itemPickUpEvent.RaiseEvent(this);

			// Unregister from checkpoint system before destroying
			if (CheckpointManager.Instance != null)
			{
				CheckpointManager.Instance.UnregisterItem(this);
			}

			Destroy(gameObject);
			}
			else
			{
				// Item was partially added or not added at all
				// The item's quantity has been modified by AddItem (reduced by what was added)
				// If quantity is now 0, it was fully added (shouldn't happen, but safety check)
				if (quantity <= 0)
				{
					itemPickUpEvent.RaiseEvent(this);
					if (CheckpointManager.Instance != null)
					{
						CheckpointManager.Instance.UnregisterItem(this);
					}
					Destroy(gameObject);
				}
				else
				{
					// Item still has quantity left - keep it in the world
					Debug.LogWarning($"Inventory full! Could not add {quantity} of {itemName}. Item remains in world.");
					// Optionally: you could add visual/audio feedback here (e.g., play a "full" sound)
				}
			}
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