using UnityEngine;

public class Item : ScriptableObject
{
	public string itemName;
	public Sprite icon;
	public int maxStackSize = 1;

	public virtual void Use()
	{
		Debug.Log("Using item: " + itemName);
	}

}
