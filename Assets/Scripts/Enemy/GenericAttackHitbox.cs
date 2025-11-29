using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericAttackHitbox : MonoBehaviour
{
    [Header("Hitbox Configuration")]
    protected AttackHitboxInfo hitboxData;
    [SerializeField] protected BoxCollider2D boxCol;
    [SerializeField] protected CircleCollider2D circleCol;

    [Header("Parent Reference (auto-detected if null)")]
    [SerializeField] protected MonoBehaviour parentScript; // Optional: manually assign parent if auto-detection fails

    protected bool active = false;
    protected readonly HashSet<GameObject> alreadyHit = new();
    protected Vector2 currentKnockbackForce;
    protected int currentDamage;
    protected bool disableAfterFirstHit;
    protected bool hasCheckedInitialOverlap = false; // Track if we've done the initial overlap check

    // Cached parent for facing direction detection
    protected MonoBehaviour parentWithFacing;
    protected IHasFacing parentInterface;
    protected System.Reflection.FieldInfo facingField;
    protected System.Reflection.PropertyInfo facingProperty;

    protected void Awake()
    {
        // Find parent component that implements IHasFacing
        if (parentScript == null)
        {
            // Search directly for IHasFacing interface (handles multiple MonoBehaviour scripts)
            parentInterface = GetComponentInParent<IHasFacing>();

            // If interface found, get the MonoBehaviour component
            if (parentInterface != null)
            {
                parentWithFacing = parentInterface as MonoBehaviour;
            }
            else
            {
                // Fallback: search for any MonoBehaviour and use reflection
                parentWithFacing = GetComponentInParent<MonoBehaviour>();
                if (parentWithFacing != null)
                {
                    CacheFacingDirection(parentWithFacing);
                }
            }
        }
        else
        {
            parentWithFacing = parentScript;
            parentInterface = parentWithFacing as IHasFacing;

            // Fallback to reflection if interface not implemented
            if (parentInterface == null)
            {
                CacheFacingDirection(parentWithFacing);
            }
        }
    }

    private void CacheFacingDirection(MonoBehaviour parent)
    {
        if (parent == null) return;

        System.Type parentType = parent.GetType();

        // All entities using this hitbox have isFacingRight, so only search for that
        // Try property first (most common)
        facingProperty = parentType.GetProperty("isFacingRight",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // If not found as property, try as field
        if (facingProperty == null)
        {
            facingField = parentType.GetField("isFacingRight",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        }
    }

    public virtual void CustomizeHitbox(AttackHitboxInfo hitboxInfo)
    {
        if (hitboxInfo == null)
        {
            Debug.LogWarning("GenericAttackHitbox: hitboxInfo is null!");
            return;
        }

        hitboxData = hitboxInfo;
        currentKnockbackForce = hitboxInfo.knockbackForce;
        currentDamage = hitboxInfo.damage;
        disableAfterFirstHit = false;

        // Auto-detect collider type based on hitboxSize.y

        bool useCircle = hitboxInfo.hitboxSize.y == 0;

        // Configure and enable the appropriate collider
        if (useCircle && circleCol != null)
        {
            // Circle collider: use x as radius
            circleCol.offset = hitboxInfo.hitboxOffset;
            circleCol.radius = hitboxInfo.hitboxSize.x;
            circleCol.enabled = false; // Will be enabled in ActivateHitbox()
            // Disable box collider
            if (boxCol != null) boxCol.enabled = false;
        }
        else if (!useCircle && boxCol != null)
        {
            // Box collider: use full Vector2 size
            boxCol.offset = hitboxInfo.hitboxOffset;
            boxCol.size = hitboxInfo.hitboxSize;
            boxCol.enabled = false; // Will be enabled in ActivateHitbox()
            // Disable circle collider
            if (circleCol != null) circleCol.enabled = false;
        }
    }

    public void ActivateHitbox()
    {
        active = true;
        alreadyHit.Clear();

        // Enable the appropriate collider based on hitboxData (configured in CustomizeHitbox)
        Collider2D activeCollider = null;
        if (hitboxData != null)
        {
            bool useCircle = hitboxData.hitboxSize.y == 0;

            if (useCircle && circleCol != null)
            {
                circleCol.enabled = true;
                activeCollider = circleCol;
            }
            else if (!useCircle && boxCol != null)
            {
                boxCol.enabled = true;
                activeCollider = boxCol;
            }
        }
        else
        {
            Debug.LogWarning("GenericAttackHitbox: ActivateHitbox called but hitboxData is null! Call CustomizeHitbox first.");
            return;
        }

        // Check for objects already inside the trigger when it activates
        // This handles cases where the player is already inside when the hitbox becomes active
        if (activeCollider != null)
        {
            hasCheckedInitialOverlap = false;
            // Check immediately
            CheckOverlappingColliders(activeCollider);
            // Also check after a frame delay to catch any physics updates
            StartCoroutine(CheckOverlapAfterFrame(activeCollider));
        }
    }

    private IEnumerator CheckOverlapAfterFrame(Collider2D collider)
    {
        // Wait for physics to update
        yield return null;
        if (active && collider != null && collider.enabled)
        {
            CheckOverlappingColliders(collider);
            hasCheckedInitialOverlap = true;
        }
    }

    private void CheckOverlappingColliders(Collider2D collider)
    {
        if (hitboxData == null) return;

        // Combine all layers we care about (player, enemy, statics)
        LayerMask combinedLayers = hitboxData.player | hitboxData.enemy | hitboxData.statics;

        // Use OverlapCollider to find all colliders already inside
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(combinedLayers);
        filter.useTriggers = true; // Include trigger colliders

        List<Collider2D> overlappingColliders = new List<Collider2D>();
        collider.Overlap(filter, overlappingColliders);

        // Process each overlapping collider as if it just entered
        foreach (Collider2D other in overlappingColliders)
        {
            if (other != null && other != collider)
            {
                // Simulate OnTriggerEnter2D for objects already inside
                OnTriggerEnter2D(other);
            }
        }
    }

    public void DisableHitbox()
    {
        active = false;
        alreadyHit.Clear();
        hasCheckedInitialOverlap = false;

        if (boxCol != null) boxCol.enabled = false;
        if (circleCol != null) circleCol.enabled = false;
    }

    private bool GetFacingRight()
    {
        // Use interface if available (most elegant)
        if (parentInterface != null)
        {
            return parentInterface.IsFacingRight;
        }

        // Fallback to reflection
        if (parentWithFacing != null)
        {
            // Try property first
            if (facingProperty != null)
            {
                object value = facingProperty.GetValue(parentWithFacing);
                if (value is bool boolValue)
                    return boolValue;
            }

            // Try field
            if (facingField != null)
            {
                object value = facingField.GetValue(parentWithFacing);
                if (value is bool boolValue)
                    return boolValue;
            }
        }

        // Final fallback: check transform scale
        return transform.lossyScale.x > 0;
    }
    private bool HasFacingDirection()
    {
        return parentInterface != null ||
               (parentWithFacing != null && (facingProperty != null || facingField != null));
    }
    public Vector2 CalculateKnockback(Vector2 baseKnockback, Collider2D targetCollider = null, bool useRadialKnockback = false)
    {
        // Radial knockback: direction from hitbox center to target (for explosions)
        if (useRadialKnockback && targetCollider != null)
        {
            Vector2 hitboxPos = transform.position;
            Vector2 targetPos = targetCollider.transform.position;
            Vector2 direction = (targetPos - hitboxPos).normalized;
            float distance = Vector2.Distance(hitboxPos, targetPos);
            float magnitude = baseKnockback.magnitude / Mathf.Max(distance, 0.5f); // Inverse distance scaling
            return direction * magnitude;
        }

        // If no facing direction available and target provided, use radial knockback
        if (!HasFacingDirection() && targetCollider != null)
        {
            Vector2 hitboxPos = transform.position;
            Vector2 targetPos = targetCollider.transform.position;
            Vector2 direction = (targetPos - hitboxPos).normalized;
            return direction * baseKnockback.magnitude;
        }

        // Constant knockback: apply facing direction (for melee attacks)
        if (hitboxData != null && hitboxData.constantKnockback)
        {
            float facingDir = GetFacingRight() ? 1f : -1f;
            return new Vector2(baseKnockback.x * facingDir, baseKnockback.y);
        }

        // Non-constant knockback: calculate direction from hitbox center to target (for projectiles)
        if (targetCollider != null)
        {
            Vector2 hitboxCenter = GetHitboxCenter();
            Vector2 targetPos = targetCollider.transform.position;
            Vector2 direction = (targetPos - hitboxCenter).normalized;
            return direction * baseKnockback.magnitude;
        }

        // Fallback: use facing direction
        float dir = GetFacingRight() ? 1f : -1f;
        return new Vector2(baseKnockback.x * dir, baseKnockback.y);
    }

    private Vector2 GetHitboxCenter()
    {
        Vector2 offset = hitboxData != null ? hitboxData.hitboxOffset : Vector2.zero;
        float facingDir = GetFacingRight() ? 1f : -1f;
        return (Vector2)transform.position + new Vector2(offset.x * facingDir, offset.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Fallback: if we haven't checked initial overlap yet, process any objects that are staying
        // This catches edge cases where the overlap check missed something
        if (!hasCheckedInitialOverlap)
        {
            ProcessHit(other);
        }
    }

    private void ProcessHit(Collider2D other)
    {
        if (!active || hitboxData == null) return;

        GameObject targetRoot = other.transform.parent != null ? other.transform.parent.gameObject : other.gameObject;
        if (alreadyHit.Contains(targetRoot)) return;

        // Calculate knockback for this specific hit
        // Use radial knockback if no facing direction is available (explosions)
        bool useRadial = !HasFacingDirection();
        Vector2 finalKnockback = CalculateKnockback(currentKnockbackForce, other, useRadial);

        // Check player layer
        if (((1 << other.gameObject.layer) & hitboxData.player) != 0)
        {
            BasePlayerMovement2D player = targetRoot.GetComponent<BasePlayerMovement2D>();
            if (player != null)
            {
                // Don't attack if player is dead
                if (HealthManager.instance != null && HealthManager.instance.IsDead())
                {
                    return;
                }

                alreadyHit.Add(targetRoot);
                Debug.Log("GenericAttackHitbox: Hit player");

                // Player uses HurtPlayer(damage, knockbackForce, null, hitboxCenter)
                // Pass hitbox center position (where attack came from) for proper knockback calculation
                Vector2 hitboxCenter = GetHitboxCenter();
                player.HurtPlayer(currentDamage, finalKnockback, null, hitboxCenter);

                if (disableAfterFirstHit) DisableHitbox();
                return;
            }
        }

        // Check enemy layer
        if (((1 << other.gameObject.layer) & hitboxData.enemy) != 0)
        {
            EnemyBase enemy = targetRoot.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                alreadyHit.Add(targetRoot);
                Debug.Log("GenericAttackHitbox: Hit enemy");

                // Enemy uses Hurt(damage, knockbackForce)
                enemy.Hurt(currentDamage, finalKnockback);

                if (disableAfterFirstHit) DisableHitbox();
                return;
            }
        }

        // Check statics layer
        if (((1 << other.gameObject.layer) & hitboxData.statics) != 0)
        {
            BreakableStatics statics = targetRoot.GetComponent<BreakableStatics>();
            if (statics != null)
            {
                alreadyHit.Add(targetRoot);
                Debug.Log("GenericAttackHitbox: Hit static");

                // Statics use Damage(damage, knockbackForce)
                statics.Damage(currentDamage, finalKnockback);

                if (disableAfterFirstHit) DisableHitbox();
                return;
            }
        }
    }

    public void SetDisableAfterFirstHit(bool value)
    {
        disableAfterFirstHit = value;
    }

}
