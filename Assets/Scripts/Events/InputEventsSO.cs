using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "VoidEvents", menuName = "Events/InputEvents")]
public class InputEvent : ScriptableObject
{
	public UnityEvent<string, PlayerControls, KeyCode> onEventRaised;

	public void RaiseEvent(string inputName, PlayerControls playerControls, KeyCode keyCode)
	{
		onEventRaised?.Invoke(inputName, playerControls, keyCode);
	}
}
