using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "IntEventSO", menuName = "Events/IntEventSO")]
public class IntEventSO : ScriptableObject
{
	public UnityEvent<int> onEventRaised;

	public void RaiseEvent(int value)
	{
		onEventRaised?.Invoke(value);
	}
}
