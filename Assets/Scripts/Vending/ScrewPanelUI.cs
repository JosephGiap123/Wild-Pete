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

    [Header("Wire Game (appears after screws removed)")]
    [SerializeField] private WireConnectionGame wireGame; // Wire game that appears after panel opens
    [SerializeField] private GameObject wireGameContainer; // Container for wire game UI

    public void SetVendingPopup(GameObject go) => vendingPopup = go;

    public Action OnPanelOpened;

    private int removedCount;
    private bool isOpen;
    private bool wasOpenedBefore = false; // Persist state - remember if already opened
    private Vector2 originalClosedPosition; // Store the original closed panel position
    private bool hasStoredOriginalPosition = false; // Track if we've stored the original position

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

        // If already opened before, show opened state immediately
        if (wasOpenedBefore)
        {
            isOpen = true;
            removedCount = screwsNeededToOpen;
            
            // Show opened sprite - preserve size AND position (with slight upward adjustment)
            if (panelBGImage && openedSprite)
            {
                RectTransform panelRect = panelBGImage.rectTransform;
                
                // Store ALL original properties before changing sprite
                Vector2 originalSize = panelRect.sizeDelta;
                Vector2 originalAnchorMin = panelRect.anchorMin;
                Vector2 originalAnchorMax = panelRect.anchorMax;
                Vector2 originalPivot = panelRect.pivot;
                Vector3 originalScale = panelRect.localScale;
                
                // Change sprite
                panelBGImage.sprite = openedSprite;
                
                // Restore ALL original properties to keep exact same position and size
                panelRect.sizeDelta = originalSize;
                // Use the stored original closed position + offset (not current position)
                // This prevents it from moving up every time
                if (hasStoredOriginalPosition)
                {
                    panelRect.anchoredPosition = new Vector2(originalClosedPosition.x, originalClosedPosition.y + 20f);
                }
                else
                {
                    // Fallback if position wasn't stored yet
                    panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, panelRect.anchoredPosition.y + 20f);
                }
                panelRect.anchorMin = originalAnchorMin;
                panelRect.anchorMax = originalAnchorMax;
                panelRect.pivot = originalPivot;
                panelRect.localScale = originalScale;
            }
            
            // Hide screws (already removed)
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
            
            // Show wire game if it exists
            if (wireGame != null && wireGameContainer != null)
            {
                wireGameContainer.SetActive(true);
                // Always call Show() - it will handle completed state and recreate connections
                wireGame.Show();
            }
            
            return;
        }

        // First time - reset everything
        removedCount = 0;
        isOpen = false;

        if (panelBGImage && closedSprite)
        {
            panelBGImage.sprite = closedSprite;
            
            // Store the original closed position (only once, on first show)
            if (!hasStoredOriginalPosition && panelBGImage.rectTransform != null)
            {
                originalClosedPosition = panelBGImage.rectTransform.anchoredPosition;
                hasStoredOriginalPosition = true;
            }
        }

        // Hide wire game initially
        if (wireGameContainer != null) wireGameContainer.SetActive(false);

        // Re-enable / reset screws
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
        // CRITICAL: Hide the wire game first to clear all wires and drag lines
        if (wireGame != null)
        {
            wireGame.Hide();
        }
        
        // Hide the wire game container
        if (wireGameContainer != null)
        {
            wireGameContainer.SetActive(false);
        }
        
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
        wasOpenedBefore = true; // Remember this state

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

        // Swap art to the opened sprite - preserve size AND position (with slight upward adjustment)
        if (panelBGImage && openedSprite)
        {
            RectTransform panelRect = panelBGImage.rectTransform;
            
            // Store ALL original properties before changing sprite
            Vector2 originalSize = panelRect.sizeDelta;
            Vector2 originalAnchorMin = panelRect.anchorMin;
            Vector2 originalAnchorMax = panelRect.anchorMax;
            Vector2 originalPivot = panelRect.pivot;
            Vector3 originalScale = panelRect.localScale;
            
            // Store original closed position if we haven't yet
            if (!hasStoredOriginalPosition)
            {
                originalClosedPosition = panelRect.anchoredPosition;
                hasStoredOriginalPosition = true;
            }
            
            // Change sprite
            panelBGImage.sprite = openedSprite;
            
            // Restore ALL original properties to keep exact same position and size
            panelRect.sizeDelta = originalSize;
            // Use the stored original closed position + offset (not current position)
            // This prevents it from moving up every time
            panelRect.anchoredPosition = new Vector2(originalClosedPosition.x, originalClosedPosition.y + 20f);
            panelRect.anchorMin = originalAnchorMin;
            panelRect.anchorMax = originalAnchorMax;
            panelRect.pivot = originalPivot;
            panelRect.localScale = originalScale;
        }

        // Show wire game
        if (wireGame != null && wireGameContainer != null)
        {
            wireGameContainer.SetActive(true);
            wireGame.SetVendingPopup(vendingPopup);
            
            // Always call Show() - it will handle completed state and recreate connections
            wireGame.Show();
        }

        OnPanelOpened?.Invoke();
    }
}
