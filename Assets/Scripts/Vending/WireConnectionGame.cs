using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// Wire connection game like Among Us - drag wires to connect them
public class WireConnectionGame : MonoBehaviour
{
    [System.Serializable]
    public class Wire
    {
        public Image wireImage;        // The wire visual (left side)
        public Color wireColor;       // Color of this wire
        public int correctTargetIndex; // Which target this wire should connect to (0-based)
    }

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject vendingPopup; // Return to this when done
    
    [Header("Wires (Left Side)")]
    [Tooltip("List of wires - each wire has a color and connects to a target")]
    [SerializeField] private List<Wire> wires = new List<Wire>();
    
    [Header("Wire Connection Points (Right Side)")]
    [Tooltip("List of target connection points on the right side (in order)")]
    [SerializeField] private List<Image> targetPoints = new List<Image>();
    
    [Header("Wire Line Renderer (Optional)")]
    [Tooltip("Prefab for drawing lines between wire and target (optional - uses Image if not set)")]
    [SerializeField] private GameObject wireLinePrefab;
    
    [Header("Settings")]
    [SerializeField] private float dragSnapDistance = 100f; // How close to target to snap (increased for easier connection)
    
    private Dictionary<Image, int> wireToTargetMap = new Dictionary<Image, int>(); // wire -> target index
    private Dictionary<Image, GameObject> wireToLineMap = new Dictionary<Image, GameObject>(); // wire -> line visual
    private Dictionary<Image, GameObject> wireToDragLineMap = new Dictionary<Image, GameObject>(); // wire -> temporary drag line
    private bool isComplete = false;
    private bool isActive = false;
    
    public System.Action<bool> OnWireGameComplete;
    
    void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        
        // Set up wire drag handlers
        foreach (var wire in wires)
        {
            if (wire.wireImage != null)
            {
                // Add WireDragHandler component to make wire draggable
                var dragHandler = wire.wireImage.GetComponent<WireDragHandler>();
                if (dragHandler == null)
                    dragHandler = wire.wireImage.gameObject.AddComponent<WireDragHandler>();
            }
        }
        
        HideInstant();
    }
    
    public void Show()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        
        isActive = true;
        
        // CRITICAL: Ensure all target points are visible and active
        EnsureTargetsVisible();
        
        // Recreate connection lines from saved connections (if any exist)
        RecreateConnectionLines();
        
        // If already complete, show completed state
        if (isComplete)
        {
            ShowCompletedState();
        }
        else
        {
            // Reset visual state but keep connections if any
            UpdateWireVisuals();
        }
        
        Debug.Log("[WireConnectionGame] Wire game shown");
    }
    
    public void Hide()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        gameObject.SetActive(false);
        
        isActive = false;
        
        // Clear all drag lines (temporary)
        foreach (var dragLine in wireToDragLineMap.Values)
        {
            if (dragLine != null)
                Destroy(dragLine);
        }
        wireToDragLineMap.Clear();
        
        // If game is NOT complete, clear visual lines but SAVE connection data
        // This way wires don't stay on screen, but progress is saved
        if (!isComplete)
        {
            // Destroy all visual connection lines
            foreach (var line in wireToLineMap.Values)
            {
                if (line != null)
                    Destroy(line);
            }
            wireToLineMap.Clear();
            
            // Reset wire positions to original
            foreach (var wire in wires)
            {
                if (wire.wireImage != null)
                {
                    var dragHandler = wire.wireImage.GetComponent<WireDragHandler>();
                    if (dragHandler != null)
                    {
                        dragHandler.ResetPosition();
                    }
                }
            }
            
            // DO NOT reset target colors - they will be restored by RecreateConnectionLines() when reopening
            // This preserves the target colors for connected wires
            
            Debug.Log("[WireConnectionGame] Game not complete - cleared visual lines but saved connection progress");
        }
        else
        {
            // Game is complete - keep connections saved (wireToTargetMap stays)
            // But clear visual lines since they shouldn't show on screen
            foreach (var line in wireToLineMap.Values)
            {
                if (line != null)
                    Destroy(line);
            }
            wireToLineMap.Clear();
            
            Debug.Log("[WireConnectionGame] Game complete - connection data saved");
        }
        
        // NOTE: wireToTargetMap is NOT cleared - this saves the progress
        // When Show() is called, RecreateConnectionLines() will restore the connections
        
        // Return to vending popup
        if (vendingPopup) vendingPopup.SetActive(true);
    }
    
    // Recreate connection lines from saved connections
    private void RecreateConnectionLines()
    {
        if (wireToTargetMap.Count == 0)
        {
            Debug.Log("[WireConnectionGame] No saved connections to recreate");
            return;
        }
        
        Debug.Log($"[WireConnectionGame] Recreating {wireToTargetMap.Count} connection lines from saved progress");
        
        foreach (var kvp in wireToTargetMap)
        {
            Image wire = kvp.Key;
            int targetIndex = kvp.Value;
            
            if (wire == null || targetIndex < 0 || targetIndex >= targetPoints.Count) continue;
            
            Image target = targetPoints[targetIndex];
            if (target == null) continue;
            
            // Check if line already exists (shouldn't happen, but safety check)
            if (wireToLineMap.ContainsKey(wire))
            {
                Debug.Log($"[WireConnectionGame] Line for wire {wire.name} already exists, skipping");
                continue;
            }
            
            // Find wire data
            Wire wireData = null;
            foreach (var w in wires)
            {
                if (w.wireImage == wire)
                {
                    wireData = w;
                    break;
                }
            }
            
            if (wireData == null) continue;
            
            // Recreate the connection line visual
            CreateLineVisual(wire, target, wireData.wireColor);
            
            // Make sure target shows the wire color and is visible
            var targetImage = target.GetComponent<Image>();
            if (targetImage != null)
            {
                targetImage.enabled = true;
                targetImage.color = new Color(wireData.wireColor.r, wireData.wireColor.g, wireData.wireColor.b, 1f);
            }
            target.gameObject.SetActive(true);
            
            // Ensure target has proper CanvasGroup settings
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1f;
            canvasGroup.ignoreParentGroups = true;
            
            Debug.Log($"[WireConnectionGame] Recreated line for wire {wire.name} to target {targetIndex}");
        }
    }
    
    private void HideInstant()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        gameObject.SetActive(false);
    }
    
    public void SetVendingPopup(GameObject go) => vendingPopup = go;
    
    // Called by WireDragHandler when wire is dropped
    public void HandleWireDrop(Image wire, PointerEventData eventData)
    {
        if (!isActive || isComplete) return;
        
        // Check if it's one of our wires
        bool isValidWire = false;
        foreach (var w in wires)
        {
            if (w.wireImage == wire)
            {
                isValidWire = true;
                break;
            }
        }
        
        if (!isValidWire) return;
        
        // Find closest target using screen position
        Image closestTarget = null;
        float closestDistance = float.MaxValue;
        
        // Convert drop position to world/canvas space
        Canvas canvas = GetComponentInParent<Canvas>();
        Vector2 dropWorldPos = eventData.position;
        
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            Vector2 localDropPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                canvas.worldCamera,
                out localDropPos))
            {
                dropWorldPos = canvasRect.TransformPoint(localDropPos);
            }
        }
        
        foreach (var target in targetPoints)
        {
            if (target == null) continue;
            
            Vector2 targetPos = target.rectTransform.position;
            float distance = Vector2.Distance(dropWorldPos, targetPos);
            
            if (distance < closestDistance && distance < dragSnapDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }
        
        Debug.Log($"[WireConnectionGame] Drop distance to closest target: {closestDistance}, snap distance: {dragSnapDistance}");
        
        // Find wire index
        int wireIndex = -1;
        Wire wireData = null;
        for (int i = 0; i < wires.Count; i++)
        {
            if (wires[i].wireImage == wire)
            {
                wireIndex = i;
                wireData = wires[i];
                break;
            }
        }
        
        if (wireIndex == -1 || wireData == null) return;
        
        if (closestTarget != null)
        {
            // Check if this is the CORRECT target for this wire (color matching)
            int targetIndex = targetPoints.IndexOf(closestTarget);
            bool isCorrectTarget = (targetIndex == wireData.correctTargetIndex);
            
            if (!isCorrectTarget)
            {
                // Wrong target - don't connect, just return wire
                Debug.Log($"[WireConnectionGame] Wrong target! Wire {wireIndex} should connect to target {wireData.correctTargetIndex}, but tried to connect to {targetIndex}");
                
                var dragHandler = wire.GetComponent<WireDragHandler>();
                if (dragHandler != null)
                {
                    dragHandler.ResetPosition();
                }
                
                // Remove drag line
                StopDraggingWire(wire);
                return;
            }
            
            // CORRECT connection - proceed
            
            // Remove old connection if exists
            if (wireToTargetMap.ContainsKey(wire))
            {
                int oldTarget = wireToTargetMap[wire];
                if (oldTarget >= 0 && oldTarget < targetPoints.Count && targetPoints[oldTarget] != null)
                {
                    targetPoints[oldTarget].color = Color.white; // Reset old target
                }
                
                // Remove old permanent line
                if (wireToLineMap.ContainsKey(wire))
                {
                    Destroy(wireToLineMap[wire]);
                    wireToLineMap.Remove(wire);
                }
            }
            
            // Create new connection
            wireToTargetMap[wire] = targetIndex;
            
            // Visual feedback - color the target with wire color
            // CRITICAL: Ensure target stays visible and active FOREVER
            closestTarget.color = new Color(wireData.wireColor.r, wireData.wireColor.g, wireData.wireColor.b, 1f);
            closestTarget.gameObject.SetActive(true);
            closestTarget.enabled = true;
            
            // Make sure Image component is enabled
            var targetImage = closestTarget.GetComponent<Image>();
            if (targetImage != null)
            {
                targetImage.enabled = true;
                targetImage.raycastTarget = true;
                targetImage.color = new Color(wireData.wireColor.r, wireData.wireColor.g, wireData.wireColor.b, 1f);
            }
            
            // CRITICAL: Add CanvasGroup with ignoreParentGroups to keep target visible
            var targetCanvasGroup = closestTarget.GetComponent<CanvasGroup>();
            if (targetCanvasGroup == null)
            {
                targetCanvasGroup = closestTarget.gameObject.AddComponent<CanvasGroup>();
            }
            targetCanvasGroup.alpha = 1f;
            targetCanvasGroup.ignoreParentGroups = true; // CRITICAL: Ignore parent alpha!
            targetCanvasGroup.blocksRaycasts = true;
            
            // Force target to stay visible - call this to ensure all targets are visible
            EnsureTargetsVisible();
            
            // Convert the drag line to a permanent connection line
            // KEEP THE EXACT DRAG LENGTH - DON'T EXTEND IT
            if (wireToDragLineMap.ContainsKey(wire))
            {
                GameObject dragLine = wireToDragLineMap[wire];
                RectTransform lineRect = dragLine.GetComponent<RectTransform>();
                
                // Keep the drag line at its CURRENT size and position - don't change it!
                // Just make it fully opaque and permanent
                Image lineImage = dragLine.GetComponent<Image>();
                if (lineImage != null)
                {
                    lineImage.color = new Color(wireData.wireColor.r, wireData.wireColor.g, wireData.wireColor.b, 1f);
                }
                
                // Move from drag line map to permanent line map
                wireToLineMap[wire] = dragLine;
                wireToDragLineMap.Remove(wire);
                
                Debug.Log($"[WireConnectionGame] Converted drag line to permanent line for wire {wireIndex} - kept original drag length");
            }
            else
            {
                // Fallback: create new line if drag line doesn't exist
                CreateLineVisual(wire, closestTarget, wireData.wireColor);
            }
            
            // Wire stays at original position - don't move it
            // Only the line visual shows the connection
            
            Debug.Log($"[WireConnectionGame] Wire {wireIndex} correctly connected to target {targetIndex}");
            
            // CRITICAL: Ensure all targets stay visible after connection
            EnsureTargetsVisible();
            
            // Check if all wires are correctly connected
            CheckCompletion();
        }
        else
        {
            // No valid target - return wire to original position and remove drag line
            var dragHandler = wire.GetComponent<WireDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ResetPosition();
            }
            
            // Remove drag line if no connection
            StopDraggingWire(wire);
        }
    }
    
    // Called by WireDragHandler when starting to drag a wire
    public void StartDraggingWire(Image wire)
    {
        if (wire == null) return;
        
        // Find parent canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        // Create temporary drag line as child of canvas (not wire game container)
        GameObject dragLine = new GameObject("DragLine_" + wire.name);
        dragLine.transform.SetParent(canvas.transform, false);
        
        RectTransform lineRect = dragLine.AddComponent<RectTransform>();
        Image lineImage = dragLine.AddComponent<Image>();
        
        // Find wire color
        Color wireColor = Color.white;
        foreach (var w in wires)
        {
            if (w.wireImage == wire)
            {
                wireColor = w.wireColor;
                break;
            }
        }
        
        // Use wire color, make it visible
        lineImage.color = new Color(wireColor.r, wireColor.g, wireColor.b, 0.8f);
        lineImage.raycastTarget = false;
        
        // Set initial properties
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.sizeDelta = new Vector2(100f, 12f);
        lineRect.anchoredPosition = Vector2.zero;
        
        wireToDragLineMap[wire] = dragLine;
        
        Debug.Log($"[WireConnectionGame] Started dragging wire {wire.name}, created drag line");
    }
    
    // Called by WireDragHandler to update drag line while dragging
    public void UpdateDragLine(Image wire, Vector2 screenPos, Canvas canvas)
    {
        if (wire == null || !wireToDragLineMap.ContainsKey(wire)) return;
        
        GameObject dragLine = wireToDragLineMap[wire];
        if (dragLine == null) return;
        
        RectTransform wireRect = wire.rectTransform;
        RectTransform lineRect = dragLine.GetComponent<RectTransform>();
        
        // Get wire position in canvas local space (not world space)
        Vector2 wireLocalPos;
        RectTransform canvasRect = canvas.transform as RectTransform;
        
        // Convert wire position to canvas local space
        Vector3 wireWorldPos = wireRect.position;
        wireLocalPos = canvasRect.InverseTransformPoint(wireWorldPos);
        
        // Convert cursor screen position to canvas local space
        Vector2 cursorLocalPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.worldCamera,
            out cursorLocalPos))
        {
            return; // Couldn't convert
        }
        
        // Calculate line from wire to cursor in local space
        Vector2 direction = cursorLocalPos - wireLocalPos;
        float distance = direction.magnitude;
        
        if (distance < 1f) 
        {
            // Too close, hide line
            dragLine.SetActive(false);
            return;
        }
        
        dragLine.SetActive(true);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Position line at midpoint between wire and cursor (in canvas local space)
        Vector2 midPoint = (wireLocalPos + cursorLocalPos) / 2f;
        
        // Set line position and properties
        lineRect.SetParent(canvasRect, false); // Make sure parent is canvas
        lineRect.localPosition = midPoint;
        lineRect.sizeDelta = new Vector2(distance, 12f); // 12px thick for visibility
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Make sure line is visible
        dragLine.transform.SetAsLastSibling();
    }
    
    // Called by WireDragHandler when stopping drag
    public void StopDraggingWire(Image wire)
    {
        if (wire == null) return;
        
        // Only destroy drag line if it hasn't been converted to permanent line
        if (wireToDragLineMap.ContainsKey(wire))
        {
            GameObject dragLine = wireToDragLineMap[wire];
            if (dragLine != null)
            {
                Destroy(dragLine);
            }
            wireToDragLineMap.Remove(wire);
        }
        // If line was converted to permanent (in wireToLineMap), don't destroy it
    }
    
    // Called by WireDragHandler to update line visual while dragging (legacy - not used)
    public void UpdateWireLine(Image wire)
    {
        if (wireToLineMap.ContainsKey(wire))
        {
            UpdateLineVisual(wire, wireToLineMap[wire]);
        }
    }
    
    private void CreateLineVisual(Image wire, Image target, Color lineColor)
    {
        if (wireLinePrefab != null)
        {
            // Use prefab for line
            GameObject line = Instantiate(wireLinePrefab, transform);
            wireToLineMap[wire] = line;
            UpdateLineVisual(wire, line);
            
            // Set line color
            var lineImage = line.GetComponent<Image>();
            if (lineImage != null)
                lineImage.color = lineColor;
        }
        else
        {
            // Create simple line using UI Image
            GameObject line = new GameObject("WireLine_" + wire.name);
            line.transform.SetParent(transform, false);
            
            RectTransform lineRect = line.AddComponent<RectTransform>();
            Image lineImage = line.AddComponent<Image>();
            lineImage.color = lineColor;
            
            wireToLineMap[wire] = line;
            UpdateLineVisual(wire, line);
        }
    }
    
    private void UpdateLineVisual(Image wire, GameObject line)
    {
        if (wire == null || line == null) return;
        
        RectTransform wireRect = wire.rectTransform;
        RectTransform lineRect = line.GetComponent<RectTransform>();
        
        if (!wireToTargetMap.ContainsKey(wire)) return;
        
        int targetIndex = wireToTargetMap[wire];
        if (targetIndex < 0 || targetIndex >= targetPoints.Count) return;
        
        Image target = targetPoints[targetIndex];
        if (target == null) return;
        
        RectTransform targetRect = target.rectTransform;
        
        // Get center positions
        Vector3 wireCenter = wireRect.position;
        Vector3 targetCenter = targetRect.position;
        
        // Calculate direction and distance
        Vector3 direction = targetCenter - wireCenter;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Extend line to fully connect wire to target
        // Calculate the actual edge-to-edge distance and add padding
        float wireHalfWidth = Mathf.Max(wireRect.sizeDelta.x, wireRect.sizeDelta.y) * 0.5f;
        float targetHalfWidth = Mathf.Max(targetRect.sizeDelta.x, targetRect.sizeDelta.y) * 0.5f;
        // Add extra padding to ensure line extends fully into both wire and target
        float extendedDistance = distance + wireHalfWidth + targetHalfWidth + 20f; // 20px extra padding
        
        // Position line at midpoint
        Vector3 midPoint = (wireCenter + targetCenter) / 2f;
        lineRect.position = midPoint;
        lineRect.sizeDelta = new Vector2(extendedDistance, 12f); // 12px thick
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
        
        // Make sure line is behind wires but visible
        line.transform.SetSiblingIndex(0); // Put line at bottom of hierarchy
    }
    
    private void UpdateWireVisuals()
    {
        // Update line visuals for all connected wires
        foreach (var kvp in wireToLineMap)
        {
            UpdateLineVisual(kvp.Key, kvp.Value);
        }
    }
    
    private void CheckCompletion()
    {
        if (wires.Count == 0 || targetPoints.Count == 0) return;
        
        // Check if all wires are connected
        if (wireToTargetMap.Count < wires.Count) return;
        
        // Check if all connections are correct
        bool allCorrect = true;
        foreach (var wire in wires)
        {
            if (wire.wireImage == null) continue;
            
            if (!wireToTargetMap.ContainsKey(wire.wireImage))
            {
                allCorrect = false;
                break;
            }
            
            int connectedTarget = wireToTargetMap[wire.wireImage];
            if (connectedTarget != wire.correctTargetIndex)
            {
                allCorrect = false;
                break;
            }
        }
        
        if (allCorrect)
        {
            CompleteGame();
        }
    }
    
    private void CompleteGame()
    {
        isComplete = true;
        isActive = false;
        
        ShowCompletedState();
        
        Debug.Log("[WireConnectionGame] All wires connected correctly! Machine is now powered on.");
        OnWireGameComplete?.Invoke(true);
        
        // Clear all visual lines when exiting (wires should not stay on screen)
        // But keep the connection data (wireToTargetMap) so we can recreate them later
        foreach (var line in wireToLineMap.Values)
        {
            if (line != null)
                Destroy(line);
        }
        wireToLineMap.Clear();
        
        // Clear drag lines
        foreach (var dragLine in wireToDragLineMap.Values)
        {
            if (dragLine != null)
                Destroy(dragLine);
        }
        wireToDragLineMap.Clear();
        
        // Immediately exit back to vending popup
        // Find parent ScrewPanelUI and close it
        ScrewPanelUI screwPanel = GetComponentInParent<ScrewPanelUI>();
        if (screwPanel != null)
        {
            screwPanel.Hide(); // This will close the screw panel and return to vending popup
        }
        else
        {
            // Fallback: just hide wire game and return to vending popup
            Hide();
        }
    }
    
    private void ShowCompletedState()
    {
        // DON'T change wire colors - keep them as they are
        // DON'T make lines green - keep wire colors
        // Just mark as complete, wires and targets stay visible with their original colors
        
        // CRITICAL: Ensure all targets stay visible after completion
        EnsureTargetsVisible();
    }
    
    public bool IsComplete() => isComplete;
    
    // Ensure all target points are visible and active
    private void EnsureTargetsVisible()
    {
        foreach (var target in targetPoints)
        {
            if (target != null && target.gameObject != null)
            {
                // Make sure target is active and visible
                target.gameObject.SetActive(true);
                target.enabled = true;
                
                // Make sure target Image component is enabled and visible
                var image = target.GetComponent<Image>();
                if (image != null)
                {
                    image.enabled = true;
                    image.raycastTarget = true;
                    // Ensure image color has full alpha (not transparent)
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
                }
                
                // CRITICAL: Add CanvasGroup to target if it doesn't have one
                // Set ignoreParentGroups = true so targets stay visible even when parent alpha = 0
                var canvasGroup = target.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 1f; // Fully visible
                canvasGroup.ignoreParentGroups = true; // CRITICAL: Ignore parent alpha!
                canvasGroup.blocksRaycasts = true;
            }
        }
        
        Debug.Log($"[WireConnectionGame] Ensured {targetPoints.Count} target points are visible");
    }
    
    // Reset the game (for testing, but state should persist normally)
    public void ResetGame()
    {
        isComplete = false;
        wireToTargetMap.Clear();
        
        // Destroy all lines
        foreach (var line in wireToLineMap.Values)
        {
            if (line != null)
                Destroy(line);
        }
        wireToLineMap.Clear();
        
        // Reset target colors
        foreach (var target in targetPoints)
        {
            if (target != null)
                target.color = Color.white;
        }
        
        UpdateWireVisuals();
    }
}
