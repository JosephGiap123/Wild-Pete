// Assets/Scripts/Vending/KeypadUI.cs
using UnityEngine;

public class KeypadUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private GameObject vendingPopup; // set at runtime so Hide() can return to vending

    void Reset()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public void SetVendingPopup(GameObject go) => vendingPopup = go;

    public void Show()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        gameObject.SetActive(true);
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
    }

    public void Hide()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        gameObject.SetActive(false);

        // Return to vending screen
        if (vendingPopup) vendingPopup.SetActive(true);
    }
}
