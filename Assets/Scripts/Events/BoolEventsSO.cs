using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "BoolEvent", menuName = "Events/BoolEvent")]
public class BoolEventSO : ScriptableObject
{
	public UnityEvent<bool> onEventRaised;

	public void RaiseEvent(bool value)
	{
		onEventRaised?.Invoke(value);
	}
}
