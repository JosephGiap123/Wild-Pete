using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableSO", menuName = "Items/ConsumableSO")]
public class ConsumableSO : ItemSO
{
	public enum ConsumableType
	{
		None,
		Health,

	}
	public ConsumableType consumableType;
	public int restoreAmount;

	public void ConsumeItem()
	{
		if (consumableType == ConsumableType.Health)
		{
			HealthManager.instance.Heal(restoreAmount);
		}
		Debug.Log("Consumed item: " + itemName);
	}

}
