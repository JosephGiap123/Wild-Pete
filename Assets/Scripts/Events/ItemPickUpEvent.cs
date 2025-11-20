using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ItemPickUpEvent", menuName = "Events/ItemPickUpEvent")]
public class ItemPickUpEvent : ScriptableObject
{
	public UnityEvent<Item> onEventRaised;
	public void RaiseEvent(Item item)
	{
		onEventRaised?.Invoke(item);
	}
}
