using UnityEngine;
using UnityEngine.Events;
//raises input events to the input event SO
public class InputBroadcaster : MonoBehaviour
{
    public InputEvent inputEvent;

    void OnDestroy()
    {
        if (inputEvent != null)
    {
            inputEvent.onEventRaised.RemoveListener(RaiseInputEvent);
    }
    }

    public void RaiseInputEvent(string inputName, PlayerControls playerControls, KeyCode keyCode)
    {
        if (inputEvent != null)
        {
            inputEvent.RaiseEvent(inputName, playerControls, keyCode);
        }
    }
}
