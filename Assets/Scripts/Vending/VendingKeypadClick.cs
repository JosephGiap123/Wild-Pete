using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class VendingKeypadClick : MonoBehaviour, IPointerClickHandler
{
    public VendingPopupInteractable parent;

    public void OnPointerClick(PointerEventData e)
    {
        StartCoroutine(OpenNextFrame());
    }

    private IEnumerator OpenNextFrame()
    {
        yield return null;        // avoid same-frame double-click issues
        parent?.OpenKeypad();
        Debug.Log("[KeypadClick] Opened next frame");
    }
}
