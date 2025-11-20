using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "VoidEvents", menuName = "Events/VoidEvents")]
public class VoidEvents : ScriptableObject
{
	public UnityEvent onEventRaised;

	public void RaiseEvent()
	{
		onEventRaised?.Invoke();
	}
}
