using UnityEngine;

public class DoorTransitionAnimRelay : MonoBehaviour
{
    [SerializeField] private GameObject doorTransitionCanvas;
    public void CallDisableCanvas()
    {
        doorTransitionCanvas.SetActive(false);
    }
}
