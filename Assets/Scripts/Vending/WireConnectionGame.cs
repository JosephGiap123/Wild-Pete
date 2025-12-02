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
    public System.Action<bool> OnWireConnected; // bool: true = correct, false = wrong

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

        // CRITICAL: Ensure all target points are visible and active FIRST
        EnsureTargetsVisible();

        // First, set each target to its correct wire color (based on which wire should connect to it)
        // This gives the player a visual hint about which wire goes where
        for (int i = 0; i < targetPoints.Count; i++)
        {
            var target = targetPoints[i];
            if (target == null) continue;

            // Find which wire should connect to this target
            Wire correctWire = null;
            foreach (var wire in wires)
            {
                if (wire.correctTargetIndex == i)
                {
                    correctWire = wire;
                    break;
                }
            }

            // If a wire should connect to this target, color it with that wire's color
            // Otherwise, leave it white (or set to a default color)
            Color targetColor;
            if (correctWire != null)
            {
                targetColor = new Color(correctWire.wireColor.r, correctWire.wireColor.g, correctWire.wireColor.b, 1f);
            }
            else
            {
                targetColor = Color.white; // No wire assigned to this target
            }

            var targetImage = target.GetComponent<Image>();
            if (targetImage != null)
            {
                targetImage.color = targetColor;
                targetImage.SetAllDirty();

                // Also set via CanvasRenderer
                var canvasRenderer = target.GetComponent<CanvasRenderer>();
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetColor(targetColor);
                }
            }
            target.color = targetColor;
        }

        // Recreate connection lines from saved connections (if any exist)
        // This will update target colors if wires are already connected (overriding the initial colors)
        RecreateConnectionLines();

        Debug.Log($"[WireConnectionGame] Show() - wireToTargetMap.Count = {wireToTargetMap.Count}");

        // If already complete, show completed state
        if (isComplete)
        {
            ShowCompletedState();
        }
        else
        {
            // Reset visual state but keep connections if any
            UpdateWireVisuals();

            // Targets are already colored with their correct wire colors from the initial setup above
            // RecreateConnectionLines() will have updated colors for already-connected wires
            // No need to reset unconnected targets - they should stay colored with their correct wire color
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

        // Return to vending popup (if it exists)
        // Note: We don't unpause here because the vending popup or screw panel might still be open
        // The pause will be handled when CloseAll() is called on the vending popup
        if (vendingPopup) vendingPopup.SetActive(true);
    }

    // Recreate connection lines from saved connections
    private void RecreateConnectionLines()
    {
        // If wireToTargetMap is empty but wireToLineMap has entries, we need to rebuild wireToTargetMap
        // by finding which target each line connects to
        if (wireToTargetMap.Count == 0 && wireToLineMap.Count > 0)
        {
            Debug.Log($"[WireConnectionGame] wireToTargetMap is empty but {wireToLineMap.Count} lines exist - attempting to rebuild connections");
            RebuildConnectionsFromLines();
        }

        if (wireToTargetMap.Count == 0)
        {
            Debug.Log("[WireConnectionGame] No saved connections to recreate - all targets will remain white");
            return;
        }

        Debug.Log($"[WireConnectionGame] Recreating {wireToTargetMap.Count} connection lines from saved progress");

        // Recreate connections and color the connected targets
        foreach (var kvp in wireToTargetMap)
        {
            Image wire = kvp.Key;
            int targetIndex = kvp.Value;

            if (wire == null || targetIndex < 0 || targetIndex >= targetPoints.Count)
            {
                Debug.LogWarning($"[WireConnectionGame] Invalid wire or targetIndex: wire={wire?.name}, targetIndex={targetIndex}");
                continue;
            }

            Image target = targetPoints[targetIndex];
            if (target == null)
            {
                Debug.LogWarning($"[WireConnectionGame] Target at index {targetIndex} is null");
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

            if (wireData == null)
            {
                Debug.LogWarning($"[WireConnectionGame] Wire data not found for wire {wire.name}");
                continue;
            }

            // Make sure target is active first
            target.gameObject.SetActive(true);

            // Set the wire color on the target - CRITICAL: do this BEFORE creating the line
            Color wireColor = new Color(wireData.wireColor.r, wireData.wireColor.g, wireData.wireColor.b, 1f);

            // Set color on both the Image component and the target directly (target IS an Image)
            var targetImage = target.GetComponent<Image>();
            if (targetImage != null)
            {
                targetImage.enabled = true;
                targetImage.color = wireColor;
                Debug.Log($"[WireConnectionGame] Set target {targetIndex} Image.color to {wireColor} (RGB: {wireColor.r:F2}, {wireColor.g:F2}, {wireColor.b:F2})");
            }
            // Also set on target directly (target IS an Image, so this is the same reference)
            target.color = wireColor;
            Debug.Log($"[WireConnectionGame] Set target {targetIndex} direct color to {wireColor} (RGB: {wireColor.r:F2}, {wireColor.g:F2}, {wireColor.b:F2})");

            // Force update to ensure color is applied
            if (targetImage != null)
            {
                targetImage.SetAllDirty(); // Force Unity to update the visual
            }

            // Ensure target has proper CanvasGroup settings
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1f;
            canvasGroup.ignoreParentGroups = true;

            // Check if line already exists (shouldn't happen, but safety check)
            if (wireToLineMap.ContainsKey(wire))
            {
                Debug.Log($"[WireConnectionGame] Line for wire {wire.name} already exists, skipping line creation");
                continue;
            }

            // Recreate the connection line visual
            CreateLineVisual(wire, target, wireData.wireColor);

            Debug.Log($"[WireConnectionGame] Recreated line for wire {wire.name} to target {targetIndex} with color {wireColor}");
        }
    }

    // Rebuild wireToTargetMap from existing lines by finding the closest target to each line
    private void RebuildConnectionsFromLines()
    {
        foreach (var kvp in wireToLineMap)
        {
            Image wire = kvp.Key;
            GameObject line = kvp.Value;

            if (wire == null || line == null) continue;

            // Find wire data to get the wire color
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

            // Find the closest target to this wire by checking line direction
            RectTransform wireRect = wire.rectTransform;
            Vector3 wireCenter = wireRect.position;

            RectTransform lineRect = line.GetComponent<RectTransform>();
            if (lineRect == null) continue;

            // Get line's end position (line extends from wire center in lineRect.right direction)
            Vector3 lineEnd = wireCenter + lineRect.right * (lineRect.sizeDelta.x * 0.5f);

            Image closestTarget = null;
            int closestTargetIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < targetPoints.Count; i++)
            {
                Image target = targetPoints[i];
                if (target == null) continue;

                RectTransform targetRect = target.rectTransform;
                Vector3 targetCenter = targetRect.position;
                float distance = Vector3.Distance(lineEnd, targetCenter);

                // If this target is closer to the line's end, it's likely the connected target
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                    closestTargetIndex = i;
                }
            }

            if (closestTarget != null && closestTargetIndex >= 0)
            {
                wireToTargetMap[wire] = closestTargetIndex;
                Debug.Log($"[WireConnectionGame] Rebuilt connection: wire {wire.name} -> target {closestTargetIndex} (distance: {closestDistance:F2})");

                // Color the target
                Color wireColor = new Color(wireData.wireColor.r, wireData.wireColor.g, wireData.wireColor.b, 1f);
                var targetImage = closestTarget.GetComponent<Image>();
                if (targetImage != null)
                {
                    targetImage.color = wireColor;
                    targetImage.SetAllDirty();
                }
                closestTarget.color = wireColor;

                // Ensure target has proper CanvasGroup settings
                var canvasGroup = closestTarget.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = closestTarget.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 1f;
                canvasGroup.ignoreParentGroups = true;
            }
        }
    }

    public void HideInstant()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        gameObject.SetActive(false);
        isActive = false;
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
                OnWireConnected?.Invoke(false);

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
            closestTarget.gameObject.SetActive(true);

            // Set the wire color on the target - do this FIRST before any other operations
            Color wireColor = new Color(wireData.wireColor.r, wireData.wireColor.g, wireData.wireColor.b, 1f);

            // Make sure Image component is enabled and colored
            var targetImage = closestTarget.GetComponent<Image>();
            if (targetImage != null)
            {
                targetImage.enabled = true;
                targetImage.raycastTarget = true;
                // Set color on the Image component (closestTarget IS an Image, but we need to be explicit)
                targetImage.color = wireColor;
                targetImage.SetAllDirty(); // Force Unity to update the visual immediately

                // Also try setting via CanvasRenderer (more direct approach)
                var canvasRenderer = closestTarget.GetComponent<CanvasRenderer>();
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetColor(wireColor);
                }
            }
            // Also set on target directly (closestTarget IS an Image)
            closestTarget.color = wireColor;
            closestTarget.enabled = true;

            Debug.Log($"[WireConnectionGame] Set target {targetIndex} color to {wireColor} (RGB: {wireColor.r:F2}, {wireColor.g:F2}, {wireColor.b:F2})");
            Debug.Log($"[WireConnectionGame] Target Image.color is now: {closestTarget.color} (RGB: {closestTarget.color.r:F2}, {closestTarget.color.g:F2}, {closestTarget.color.b:F2})");

            // Force multiple frame updates to ensure color is applied
            StartCoroutine(ForceTargetColorUpdate(closestTarget, wireColor));
            StartCoroutine(ForceTargetColorUpdateDelayed(closestTarget, wireColor, 0.1f));

            // CRITICAL: Add CanvasGroup with ignoreParentGroups to keep target visible
            var targetCanvasGroup = closestTarget.GetComponent<CanvasGroup>();
            if (targetCanvasGroup == null)
            {
                targetCanvasGroup = closestTarget.gameObject.AddComponent<CanvasGroup>();
            }
            targetCanvasGroup.alpha = 1f;
            targetCanvasGroup.ignoreParentGroups = true; // CRITICAL: Ignore parent alpha!
            targetCanvasGroup.blocksRaycasts = true;

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

            // Don't call EnsureTargetsVisible() here - it preserves existing colors which might be white
            // We've already set the target color above, so we don't need to call it
            OnWireConnected?.Invoke(true);

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

        // Clear all drag lines (temporary)
        foreach (var dragLine in wireToDragLineMap.Values)
        {
            if (dragLine != null)
                Destroy(dragLine);
        }
        wireToDragLineMap.Clear();

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

        // Reset target colors
        foreach (var target in targetPoints)
        {
            if (target != null)
                target.color = Color.white;
        }

        UpdateWireVisuals();
    }

    // Coroutine to force target color update after a frame delay
    private System.Collections.IEnumerator ForceTargetColorUpdate(Image target, Color color)
    {
        yield return null; // Wait one frame

        if (target != null)
        {
            var targetImage = target.GetComponent<Image>();
            if (targetImage != null)
            {
                targetImage.color = color;
                targetImage.SetAllDirty();

                // Also set via CanvasRenderer
                var canvasRenderer = target.GetComponent<CanvasRenderer>();
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetColor(color);
                }
            }
            target.color = color;
            Debug.Log($"[WireConnectionGame] Force updated target color to {color} (RGB: {color.r:F2}, {color.g:F2}, {color.b:F2})");
        }
    }

    // Additional delayed color update
    private System.Collections.IEnumerator ForceTargetColorUpdateDelayed(Image target, Color color, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target != null)
        {
            var targetImage = target.GetComponent<Image>();
            if (targetImage != null)
            {
                targetImage.color = color;
                targetImage.SetAllDirty();

                var canvasRenderer = target.GetComponent<CanvasRenderer>();
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetColor(color);
                }
            }
            target.color = color;
            Debug.Log($"[WireConnectionGame] Delayed force updated target color to {color} (RGB: {color.r:F2}, {color.g:F2}, {color.b:F2})");
        }
    }
}
