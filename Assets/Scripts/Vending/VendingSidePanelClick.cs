using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class VendingSidePanelClick : MonoBehaviour, IPointerClickHandler
{
    public VendingPopupInteractable parent;

    public void OnPointerClick(PointerEventData e)
    {
        StartCoroutine(OpenNextFrame());
    }

    private IEnumerator OpenNextFrame()
    {
        yield return null;
        parent?.OpenScrewPanel();
        Debug.Log("[SidePanelClick] Screw panel opened next frame");
    }
}
