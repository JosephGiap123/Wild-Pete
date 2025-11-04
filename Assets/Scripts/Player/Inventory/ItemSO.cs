using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Items/ItemSO")]
public class ItemSO : ScriptableObject
{
	public string itemName;
	public Sprite icon;
	public Sprite dropIcon;
	public int maxStackSize;
	public int quantity;
	[TextArea] public string itemDesc;

}
