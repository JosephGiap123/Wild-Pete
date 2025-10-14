using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Screw : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Sprites")]
    [SerializeField] private Image img;          // assign the same Image this script is on (or leave null and it will auto-grab)
    [SerializeField] private Sprite screwIdle;   // starting sprite
    [SerializeField] private Sprite screwStage1; // optional
    [SerializeField] private Sprite screwStage2; // optional
    [SerializeField] private Sprite screwStage3; // optional
    [SerializeField] private Sprite screwHole;   // optional (final look); if null -> the screw image is hidden on removal

    [Header("Tuning")]
    [Tooltip("How many full turns (360°) before the screw is considered removed.")]
    [SerializeField] private float turnsToRemove = 1.25f;
    [Tooltip("Mouse drag sensitivity; higher = faster turning.")]
    [SerializeField] private float sensitivity = 0.35f;

    public event Action<Screw> onRemoved;

    private bool isHeld;
    private bool isRemoved;
    private Vector2 lastPos;
    private float accAngle; // accumulated rotation in degrees (signed)

    public bool IsRemoved => isRemoved;

    private void Awake()
    {
        if (!img) img = GetComponent<Image>();
        // Ensure we start in a sane visual state
        if (img && screwIdle) img.sprite = screwIdle;
        transform.localRotation = Quaternion.identity;
        isHeld = false;
        isRemoved = false;
        accAngle = 0f;
    }

    private void Reset()
    {
        img = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (isRemoved) return;
        isHeld = true;
        lastPos = e.position;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!isHeld || isRemoved) return;

        Vector2 cur = e.position;
        Vector2 delta = cur - lastPos;
        lastPos = cur;

        // Simple “rotate by drag” feel: horizontal - vertical to bias circular motion
        float add = (delta.x - delta.y) * 5f * sensitivity;
        accAngle += add;

        // Apply visual rotation while turning
        transform.localRotation = Quaternion.Euler(0f, 0f, -accAngle);

        // Progress 0..1
        float p = Mathf.Clamp01(Mathf.Abs(accAngle) / (turnsToRemove * 360f));

        // Stage sprite updates (optional)
        if      (p > 0.75f && screwStage3) img.sprite = screwStage3;
        else if (p > 0.50f && screwStage2) img.sprite = screwStage2;
        else if (p > 0.25f && screwStage1) img.sprite = screwStage1;
        else if (screwIdle)                img.sprite = screwIdle;

        // Finished?
        if (!isRemoved && Mathf.Abs(accAngle) >= turnsToRemove * 360f)
        {
            isRemoved = true;

            // Snap rotation back so the image isn’t left tilted
            transform.localRotation = Quaternion.identity;

            // Two behaviors:
            // 1) If a hole sprite is assigned, use it and stop raycasts
            // 2) Otherwise, hide the image entirely (no white box)
            if (img)
            {
                if (screwHole != null)
                {
                    img.sprite = screwHole;
                    img.SetNativeSize();       // ensure no stretching/white box
                    img.raycastTarget = false; // can’t interact anymore
                }
                else
                {
                    img.enabled = false;       // simply disappears
                    img.raycastTarget = false;
                }
            }

            onRemoved?.Invoke(this);
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        isHeld = false;
    }

    // Optional helper if you ever want to reuse/reset this screw at runtime.
    public void ResetScrew()
    {
        isHeld = false;
        isRemoved = false;
        accAngle = 0f;
        transform.localRotation = Quaternion.identity;

        if (!img) img = GetComponent<Image>();
        if (img)
        {
            img.enabled = true;
            img.raycastTarget = true;
            if (screwIdle) img.sprite = screwIdle;
        }
    }
}
