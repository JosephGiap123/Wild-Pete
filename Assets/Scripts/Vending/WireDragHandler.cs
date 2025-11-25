using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Component that makes a wire draggable
public class WireDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private WireConnectionGame wireGame;
    private Image wireImage;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        wireImage = GetComponent<Image>();
        
        // Find parent canvas
        canvas = GetComponentInParent<Canvas>();
        
        // Find WireConnectionGame in parent
        wireGame = GetComponentInParent<WireConnectionGame>();
        
        // Store original position
        originalPosition = rectTransform.anchoredPosition;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (wireGame == null) return;
        
        // Keep wire fully opaque while dragging
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        
        // Store original position - wire stays in place!
        originalPosition = rectTransform.anchoredPosition;
        
        // Create temporary drag line that extends from wire
        if (wireGame != null && wireImage != null)
        {
            wireGame.StartDraggingWire(wireImage);
        }
        
        Debug.Log($"[WireDragHandler] Started dragging {wireImage.name}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        
        // DON'T move the wire - keep it in place, just show the line
        // The wire stays where it is, and we draw a line from wire to cursor
        
        // Update drag line visual to follow cursor
        if (wireGame != null && wireImage != null)
        {
            wireGame.UpdateDragLine(wireImage, eventData.position, canvas);
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        if (wireGame != null)
        {
            // Let wire game handle the drop first
            // It will decide whether to keep the drag line or remove it
            wireGame.HandleWireDrop(GetComponent<Image>(), eventData);
            
            // Only stop dragging if wire game didn't convert it to permanent
            // (HandleWireDrop will remove from drag map if connection succeeded)
            if (wireGame != null && wireImage != null)
            {
                wireGame.StopDraggingWire(wireImage);
            }
        }
        else
        {
            // Return to original position if no wire game
            rectTransform.anchoredPosition = originalPosition;
        }
    }
    
    public void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }
    
    public Vector2 GetOriginalPosition()
    {
        return originalPosition;
    }
}

