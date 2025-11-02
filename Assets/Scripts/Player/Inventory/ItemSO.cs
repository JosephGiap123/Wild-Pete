using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
	public string itemName;
	public Sprite icon;
	public int maxStackSize;
	[TextArea] public string itemDesc;

}
