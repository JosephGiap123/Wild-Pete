using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrewPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;   // assign on root
    [SerializeField] private Image panelBGImage;        // assign PanelBG's Image
    [SerializeField] private Sprite closedSprite;       // closed art
    [SerializeField] private Sprite openedSprite;       // openscrew.png

    [Header("Screws")]
    [SerializeField] private List<Screw> screws;        // add both screws
    [SerializeField] private int screwsNeededToOpen = 2;

    [Header("Back-to-vending")]
    [SerializeField] private GameObject vendingPopup;   // set at runtime by controller

    public void SetVendingPopup(GameObject go) => vendingPopup = go;

    public Action OnPanelOpened;

    private int removedCount;
    private bool isOpen;

    void Awake()
    {
        // subscribe safely (match Action<Screw>)
        foreach (var s in screws)
        {
            if (s == null) continue;
            s.onRemoved -= HandleScrewRemoved;   // avoid duplicate adds
            s.onRemoved += HandleScrewRemoved;
        }

        HideInstant();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        removedCount = 0;
        isOpen = false;

        if (panelBGImage && closedSprite) panelBGImage.sprite = closedSprite;

        // Re-enable / reset screws if this panel is reused
        foreach (var s in screws)
        {
            if (s == null) continue;
            var img = s.GetComponent<Image>();
            if (img)
            {
                img.enabled = true;
                img.raycastTarget = true;
            }
            s.ResetScrew();
        }
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        gameObject.SetActive(false);

        // Return to vending screen
        if (vendingPopup) vendingPopup.SetActive(true);
    }

    private void HideInstant()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        gameObject.SetActive(false);
    }

    // ✅ now matches Action<Screw>
    private void HandleScrewRemoved(Screw _)
    {
        removedCount++;
        if (!isOpen && removedCount >= screwsNeededToOpen)
            OpenNow();
    }

    private void OpenNow()
    {
        isOpen = true;

        // Hide screws so nothing remains on top
        foreach (var s in screws)
        {
            if (s == null) continue;
            var img = s.GetComponent<Image>();
            if (img)
            {
                img.enabled = false;
                img.raycastTarget = false;
            }
        }

        // Swap art to the opened sprite
        if (panelBGImage && openedSprite)
            panelBGImage.sprite = openedSprite;

        OnPanelOpened?.Invoke();
    }
}
