using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generalized bullet script that supports multiple movement patterns, piercing, and various behaviors.
/// Can be configured for regular bullets, shotgun pellets, piercing rounds, wave bullets, etc.
/// </summary>
public class GeneralizedBullet : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private float bulletSpeed = 14f;
    [SerializeField] private float bulletLifeTime = 3f;
    [SerializeField] private int damage = 3;

    [Header("Layer Masks")]
    [Tooltip("Layers that ALWAYS destroy the bullet (walls, ground, obstacles). Do NOT include enemies/statics here.")]
    [SerializeField] private LayerMask bulletDestroyMask;

    [Tooltip("Layers for enemies (used by player bullets). These are handled separately with piercing logic.")]
    [SerializeField] private LayerMask enemyMask;

    [Tooltip("Layers for breakable statics (crates, etc.). These are handled separately with piercing logic.")]
    [SerializeField] private LayerMask staticMask;

    [Tooltip("Layers for player (used by enemy bullets). Leave empty for player bullets.")]
    [SerializeField] private LayerMask playerMask;

    [Header("Bullet Type")]
    [Tooltip("True if this is a player bullet (hits enemies), false if enemy bullet (hits player)")]
    [SerializeField] private bool isPlayerBullet = true;

    public enum MovementPattern
    {
        Straight,
        Wave,           // Sine wave pattern
        Spiral,         // Spiral pattern
        Spread          // Random spread (for shotgun)
    }

    [SerializeField] private MovementPattern movementPattern = MovementPattern.Straight;

    [Header("Wave Pattern Settings")]
    [Tooltip("Amplitude of the wave (how far it deviates from center)")]
    [SerializeField] private float waveAmplitude = 1f;
    [Tooltip("Frequency of the wave (how fast it oscillates)")]
    [SerializeField] private float waveFrequency = 2f;
    [Tooltip("Direction of wave oscillation (0 = horizontal, 90 = vertical)")]
    [SerializeField] private float waveDirection = 90f; // Vertical by default

    [Header("Spread Pattern Settings (Shotgun)")]
    [Tooltip("Maximum angle spread in degrees")]
    [SerializeField] private float spreadAngle = 20f;
    [Tooltip("Maximum speed variation")]
    [SerializeField] private float maxSpeedVariation = 3f;

    [Header("Spiral Pattern Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    [SerializeField] private float spiralRotationSpeed = 180f;
    [Tooltip("Radius of spiral")]
    [SerializeField] private float spiralRadius = 0.5f;

    [Header("Piercing Settings")]
    [Tooltip("Number of enemies/statics this bullet can pierce through (0 = no piercing)")]
    [SerializeField] private int maxPierceCount = 0;
    [Tooltip("Damage reduction per pierce (0 = no reduction, 1 = full reduction)")]
    [Range(0f, 1f)]
    [SerializeField] private float pierceDamageReduction = 0f;

    [Header("Knockback")]
    [SerializeField] private Vector2 knockbackForce = Vector2.zero;
    [SerializeField] private bool useDirectionalKnockback = false;
    [SerializeField] private float knockbackStrength = 0f;

    private Rigidbody2D rb;
    private Vector2 initialDirection;
    private float travelDistance = 0f;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // Track hit targets for piercing
    private int currentPierceCount = 0;
    private float currentDamage;
    private float initialSpeed;

    // Wave pattern variables
    private Vector2 baseVelocity;
    private float waveTime = 0f;

    // Spiral pattern variables
    private float spiralAngle = 0f;
    private Vector2 spiralCenter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
    }

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize the bullet with custom parameters (called from weapon or spawner)
    /// </summary>
    public void Initialize(float speed = -1f, int bulletDamage = -1, float lifetime = -1f,
                          MovementPattern pattern = MovementPattern.Straight,
                          float amplitude = -1f, float frequency = -1f,
                          int pierce = -1, float spread = -1f, bool playerBullet = true)
    {
        if (speed > 0f) bulletSpeed = speed;
        if (bulletDamage >= 0) damage = bulletDamage;
        if (lifetime > 0f) bulletLifeTime = lifetime;
        if (pattern != MovementPattern.Straight) movementPattern = pattern;
        if (amplitude >= 0f) waveAmplitude = amplitude;
        if (frequency >= 0f) waveFrequency = frequency;
        if (pierce >= 0) maxPierceCount = pierce;
        if (spread >= 0f) spreadAngle = spread;
        isPlayerBullet = playerBullet;

        Initialize();
    }

    private void Initialize()
    {
        currentDamage = damage;
        initialSpeed = bulletSpeed;
        initialDirection = transform.right;

        // Setup movement based on pattern
        switch (movementPattern)
        {
            case MovementPattern.Straight:
                SetStraightVelocity();
                break;
            case MovementPattern.Wave:
                SetWaveVelocity();
                break;
            case MovementPattern.Spread:
                SetSpreadVelocity();
                break;
            case MovementPattern.Spiral:
                SetSpiralVelocity();
                break;
        }

        SetDestroyTime();
    }

    public void AddDamage(int damage)
    {
        currentDamage += damage;
    }

    private void Update()
    {
        // Update movement patterns that need per-frame updates
        if (movementPattern == MovementPattern.Wave)
        {
            UpdateWaveMovement();
        }
        else if (movementPattern == MovementPattern.Spiral)
        {
            UpdateSpiralMovement();
        }

        // Track travel distance for wave calculations
        travelDistance += bulletSpeed * Time.deltaTime;
    }

    private void SetStraightVelocity()
    {
        rb.linearVelocity = initialDirection * bulletSpeed;
    }

    private void SetSpreadVelocity()
    {
        // Random angle spread (for shotgun pellets)
        float randomAngle = Random.Range(-spreadAngle, spreadAngle);
        transform.Rotate(0, 0, randomAngle);
        float randomSpeed = Random.Range(bulletSpeed - maxSpeedVariation, bulletSpeed + maxSpeedVariation);
        rb.linearVelocity = transform.right * randomSpeed;
    }

    private void SetWaveVelocity()
    {
        // Set base forward velocity
        baseVelocity = initialDirection * bulletSpeed;
        rb.linearVelocity = baseVelocity;
        waveTime = 0f;
    }

    private void UpdateWaveMovement()
    {
        waveTime += Time.deltaTime;

        // Calculate perpendicular direction for wave oscillation
        Vector2 perpendicular = new Vector2(-initialDirection.y, initialDirection.x);

        // Calculate wave offset using sine wave
        float waveOffset = Mathf.Sin(waveTime * waveFrequency) * waveAmplitude;

        // Apply wave offset perpendicular to movement direction
        Vector2 waveOffsetVector = perpendicular * waveOffset;

        // Combine base velocity with wave offset
        rb.linearVelocity = baseVelocity + waveOffsetVector;
    }

    private void SetSpiralMovement()
    {
        spiralCenter = transform.position;
        spiralAngle = 0f;
        baseVelocity = initialDirection * bulletSpeed;
    }

    private void SetSpiralVelocity()
    {
        SetSpiralMovement();
        UpdateSpiralMovement();
    }

    private void UpdateSpiralMovement()
    {
        spiralAngle += spiralRotationSpeed * Time.deltaTime;

        // Calculate spiral offset
        float radians = spiralAngle * Mathf.Deg2Rad;
        Vector2 spiralOffset = new Vector2(
            Mathf.Cos(radians) * spiralRadius,
            Mathf.Sin(radians) * spiralRadius
        );

        // Apply spiral to velocity
        Vector2 perpendicular = new Vector2(-initialDirection.y, initialDirection.x);
        rb.linearVelocity = baseVelocity + perpendicular * spiralOffset.magnitude * Mathf.Sin(radians);
    }

    private void SetDestroyTime()
    {
        Destroy(gameObject, bulletLifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if this is a solid obstacle that always destroys the bullet (walls, ground, etc.)
        bool hitSolidObstacle = ((1 << collision.gameObject.layer) & bulletDestroyMask) != 0;

        // If we hit a solid obstacle, always destroy (no piercing through walls)
        if (hitSolidObstacle)
        {
            Destroy(gameObject);
            return;
        }

        // Handle hits based on bullet type (enemies/statics/player can be pierced)
        bool shouldDestroy = false;

        if (isPlayerBullet)
        {
            // Player bullets hit enemies and statics
            if (((1 << collision.gameObject.layer) & enemyMask) != 0)
            {
                HandleEnemyHit(collision, ref shouldDestroy);
            }
            else if (((1 << collision.gameObject.layer) & staticMask) != 0)
            {
                HandleStaticHit(collision, ref shouldDestroy);
            }
        }
        else
        {
            // Enemy bullets hit player and statics
            if (((1 << collision.gameObject.layer) & playerMask) != 0)
            {
                HandlePlayerHit(collision, ref shouldDestroy);
            }
            else if (((1 << collision.gameObject.layer) & staticMask) != 0)
            {
                HandleStaticHit(collision, ref shouldDestroy);
            }
        }

        // Destroy bullet if it can't pierce or has reached max pierces
        if (shouldDestroy)
        {
            Destroy(gameObject);
        }
    }

    private void HandleEnemyHit(Collider2D collision, ref bool shouldDestroy)
    {
        if (collision.gameObject == null) return;

        GameObject targetRoot = collision.transform.parent != null
            ? collision.transform.parent.gameObject
            : collision.gameObject;

        // Check if we've already hit this target (for piercing)
        if (hitTargets.Contains(targetRoot))
        {
            return; // Already hit this target
        }

        EnemyBase enemy = targetRoot.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            Vector2 knockback = CalculateKnockback(collision.transform.position);
            enemy.Hurt(Mathf.RoundToInt(currentDamage), knockback);

            hitTargets.Add(targetRoot);
            currentPierceCount++;

            // Reduce damage for next pierce
            if (currentPierceCount < maxPierceCount)
            {
                currentDamage *= (1f - pierceDamageReduction);
            }

            Debug.Log($"Bullet hit enemy: {targetRoot.name}, Damage: {Mathf.RoundToInt(currentDamage)}, Pierce: {currentPierceCount}/{maxPierceCount}");

            // Set shouldDestroy based on whether we can still pierce
            shouldDestroy = (maxPierceCount == 0 || currentPierceCount >= maxPierceCount);
        }
    }

    private void HandleStaticHit(Collider2D collision, ref bool shouldDestroy)
    {
        if (collision.gameObject == null) return;

        GameObject targetRoot = collision.transform.parent != null
            ? collision.transform.parent.gameObject
            : collision.gameObject;

        // Check if we've already hit this target (for piercing)
        if (hitTargets.Contains(targetRoot))
        {
            return;
        }

        BreakableStatics statics = targetRoot.GetComponent<BreakableStatics>();
        if (statics != null)
        {
            Vector2 knockback = CalculateKnockback(collision.transform.position);
            statics.Damage(Mathf.RoundToInt(currentDamage), knockback);

            hitTargets.Add(targetRoot);
            currentPierceCount++;

            // Reduce damage for next pierce
            if (currentPierceCount < maxPierceCount)
            {
                currentDamage *= (1f - pierceDamageReduction);
            }

            Debug.Log($"Bullet hit static: {targetRoot.name}, Damage: {Mathf.RoundToInt(currentDamage)}, Pierce: {currentPierceCount}/{maxPierceCount}");

            // Set shouldDestroy based on whether we can still pierce
            shouldDestroy = (maxPierceCount == 0 || currentPierceCount >= maxPierceCount);
        }
    }

    private void HandlePlayerHit(Collider2D collision, ref bool shouldDestroy)
    {
        if (collision.gameObject == null) return;

        // Don't attack if player is dead
        if (HealthManager.instance != null && HealthManager.instance.IsDead())
        {
            shouldDestroy = true;
            return;
        }

        GameObject targetRoot = collision.transform.parent != null
            ? collision.transform.parent.gameObject
            : collision.gameObject;

        BasePlayerMovement2D player = targetRoot.GetComponent<BasePlayerMovement2D>();
        if (player != null)
        {
            Vector2 knockback = CalculateKnockback(collision.transform.position);
            player.HurtPlayer(Mathf.RoundToInt(currentDamage), knockback);

            Debug.Log($"Bullet hit player: {targetRoot.name}, Damage: {Mathf.RoundToInt(currentDamage)}");
        }
    }

    private Vector2 CalculateKnockback(Vector2 hitPosition)
    {
        if (useDirectionalKnockback && knockbackStrength > 0f)
        {
            Vector2 direction = (hitPosition - (Vector2)transform.position).normalized;
            return direction * knockbackStrength;
        }
        return knockbackForce;
    }
}

