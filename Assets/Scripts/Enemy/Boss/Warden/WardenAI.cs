using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class WardenAI : EnemyBase
{

    [Header("Animations")]
    [SerializeField] private Animator anim;
    private string currentState = "Idle";

    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] private BoxCollider2D groundCheckBox;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat Setting")]
    public int phaseNum = 1;
    protected bool isDead = false;
    protected bool isInAir = false;
    protected bool inAttackState = true;
    protected int isAttacking = 0;

    [Header("Combat Stats")]
    [SerializeField] protected int rangedDmg = 6;
    [SerializeField] protected float rangedSpeed = 20f;
    [SerializeField] protected float rangedLifeSpan = 4f;
    [SerializeField] protected Vector2 rangedKnockback = new(4f, 1f);
    [SerializeField] protected Vector2 laserKnockback;
    [SerializeField] protected int laserDmg;

    [SerializeField] protected float ultimateCooldown = 5f;
    [SerializeField] protected float regularAttackCooldown = 1f;
    [SerializeField] protected float rangedAttackCooldown = 3f;
    private float ultimateTimer = 0f;
    private float regularTimer = 0f;
    private float rangedTimer = 0f;
    [SerializeField] protected float meleeDistance = 1.5f;
    [SerializeField] protected float laserDistance = 4f;
    [SerializeField] protected float rangedDistance = 15f;
    private bool isInvincible = true;

    [Header("Combat References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private BoxCollider2D boxAttackHitbox;
    [SerializeField] private GenericAttackHitbox attackHitboxScript;
    [SerializeField] private GameObject slamParticlePrefab;
    [SerializeField] private GameObject phaseChangeParticles;
    private int ult1ChainCount = 0;
    protected float distanceToPlayer = 100f;

    [SerializeField] BossHPBarInteractor hpBarInteractor;
    private Transform player;
    
    protected override void Awake()
    {
        base.Awake(); // This will try to register with CheckpointManager
        
        // Ensure boss is registered with CheckpointManager (in case it wasn't ready during base.Awake())
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RegisterEnemy(this);
        }
    }
    
    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
        
        // Try to register with CheckpointManager again (in case it wasn't ready during Awake())
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RegisterEnemy(this);
        }
    }
    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
    }
    private void HandlePlayerSet(GameObject playerObj)
    {
        if (playerObj != null) this.player = playerObj.transform;
    }

    public void Update()
    {
        if (player != null)
        {
            distanceToPlayer = Vector2.Distance(player.position, transform.position);
        }
        else
        {
            distanceToPlayer = 100f; // Default to far away if player not set
        }
        ultimateTimer -= Time.deltaTime;
        regularTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;
        AnimationControl();
        IsGroundedCheck();
        if (inAttackState || isDead) return;
        //testing inputs;
        DecideAttack();
    }

    public void Start()
    {
        StartCoroutine(WaitForNearbyPlayer());
    }
    public IEnumerator WaitForNearbyPlayer()
    {
        ChangeAnimationState("EntranceIdle");
        yield return new WaitForSeconds(0.5f);
        
        // Wait for player reference to be set
        while (player == null)
        {
            // Try to get player from GameManager
            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                player = GameManager.Instance.player.transform;
            }
            ChangeAnimationState("EntranceIdle");
            yield return null;
        }
        
        // Keep animation state updated while waiting (since AnimationControl won't run when inAttackState is true)
        while (distanceToPlayer > 3f)
        {
            ChangeAnimationState("EntranceIdle");
            yield return null;
        }

        // Play entrance animation when player is nearby
        ChangeAnimationState("Entrance");
        isInvincible = false;
        hpBarInteractor.ShowHealthBar(true);
        // Update health bar visual when showing it (to reflect current health)
        hpBarInteractor.UpdateHealthVisual();

        // The entrance animation will call EndAttackState() via animation event
        // which will set inAttackState = false when the animation completes
        // No need to manually set it here
    }

    public void ChangeAnimationState(string newState)
    {
        if (newState == currentState) return;
        anim.Play(newState, 0, 0f);
        currentState = newState;
    }

    void IsGroundedCheck()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheckBox.bounds.center, groundCheckBox.bounds.size, 0f, groundLayer);
        isInAir = colliders.Length == 0;
    }

    private void MoveTowards(Vector2 target, float speed = -1f)
    {
        // Use default moveSpeed if no speed specified
        if (speed < 0) speed = moveSpeed;
        float phaseSpeedMultiplier = 1f + (phaseNum - 1) * 0.2f;
        float effectiveSpeed = speed * phaseSpeedMultiplier;

        float deltaX = target.x - transform.position.x;
        if (Mathf.Abs(deltaX) > 0.5f)
        {
            float direction = Mathf.Sign(deltaX);
            rb.linearVelocity = new Vector2(direction * effectiveSpeed, rb.linearVelocity.y);

            if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
                FlipSprite();
        }
        else
        {
            ZeroVelocity();
        }
    }

    protected void AnimationControl()
    {
        if (isDead)
        {
            ChangeAnimationState("Death");
            return;
        }
        else if (inAttackState)
        {
            return;
        }
        else if (isInAir)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                ChangeAnimationState("Rising");
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                ChangeAnimationState("Falling");
            }
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.25f)
            {
                ChangeAnimationState("Run");
            }
            else
            {
                ChangeAnimationState("Idle");
            }
        }
    }

    public override void Hurt(int dmg, Vector2 knockbackForce)
    {
        if (isInvincible) return;
        StartCoroutine(base.DamageFlash(0.2f));
        health -= dmg;
        hpBarInteractor.UpdateHealthVisual();
        if (damageText != null)
        {
            GameObject dmgText = Instantiate(damageText, transform.position, transform.rotation);
            dmgText.GetComponentInChildren<DamageText>().Initialize(new(knockbackForce.x, 5f), dmg, new Color(0.8862745f, 0.3660145f, 0.0980392f, 1f), Color.red);
        }
        if (health <= 0)
        {
            isInvincible = true;
            StartCoroutine(Death());
        }
        else if (health <= maxHealth * 0.66f && phaseNum < 2) //swap phases
        {
            phaseNum = 2;
            Instantiate(phaseChangeParticles, transform.position, Quaternion.identity);
        }
        else if (health <= maxHealth * 0.33f && phaseNum < 3)
        {
            phaseNum = 3;
            Instantiate(phaseChangeParticles, transform.position, Quaternion.identity);
        }
    }
    public void FaceTowardsPlayer()
    {
        if (isFacingRight && player.position.x < transform.position.x)
        {
            FlipSprite();
        }
        else if (!isFacingRight && player.position.x > transform.position.x)
        {
            FlipSprite();
        }
    }

    protected IEnumerator Death()
    {
        isDead = true;
        ZeroVelocity(); // Stop all movement
        dropItemsOnDeath.DropItems();
        yield return new WaitForSeconds(2f); //wait for death animation to finish
        hpBarInteractor.ShowHealthBar(false);
        Die();
    }

    protected override void Die()
    {
        base.Die();
    }

    public override void Respawn(Vector2? position = null, bool? facingRight = null)
    {
        // Boss always returns to spawn point, ignoring checkpoint position
        base.Respawn(null, facingRight);

        // Reset all AI state variables
        isDead = false;
        isAttacking = 0;
        inAttackState = true; // Keep in attack state until entrance animation completes
        currentState = "Idle";
        ult1ChainCount = 0;
        isInvincible = true; // Will be set to false by WaitForNearbyPlayer coroutine

        // Reset phase to 1 (phase should not persist across respawns)
        phaseNum = 1;

        // Reset all timers
        ultimateTimer = 0f;
        regularTimer = 0f;
        rangedTimer = 0f;

        // Reset movement and physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 3f; // Reset gravity scale (in case teleport attack was interrupted)
        }

        // Stop any active coroutines
        StopAllCoroutines();

        // Also stop coroutines on WardenAnimRelay (which runs Ult2LaserSpawn)
        WardenAnimRelay animRelay = GetComponentInChildren<WardenAnimRelay>();
        if (animRelay != null)
        {
            animRelay.StopAllCoroutines();
        }

        // Reset animation state
        if (anim != null)
        {
            anim.Play("Idle");
        }

        // Hide health bar initially
        if (hpBarInteractor != null)
        {
            hpBarInteractor.ShowHealthBar(false);
            // Update health bar visual to reflect full health (will be shown when entrance completes)
            hpBarInteractor.UpdateHealthVisual();
        }

        // Clean up any remaining lasers that might have been spawned (safety check)
        GroundLaserBeam[] remainingLasers = FindObjectsByType<GroundLaserBeam>(FindObjectsSortMode.None);
        foreach (GroundLaserBeam laser in remainingLasers)
        {
            if (laser != null && laser.gameObject != null)
            {
                Destroy(laser.gameObject);
            }
        }

        // Restart the entrance sequence
        StartCoroutine(WaitForNearbyPlayer());
    }

    public override void FlipSprite()
    {
        base.FlipSprite(); // Handle isFacingRight and sprite flip
        // Rotate projectile spawn point to match facing direction
        if (projectileSpawnPoint != null)
        {
            projectileSpawnPoint.localRotation = Quaternion.Euler(0, 0, isFacingRight ? 0 : 180);
        }
    }
    public void EndAttack()
    {
        isAttacking = 0;
    }

    public void ChangeAttackNumber(int newAttackNum)
    {
        isAttacking = newAttackNum;
    }

    public void EndAttackState()
    {
        inAttackState = false;
    }

    public void ActivateHitbox()
    {
        attackHitboxScript.ActivateHitbox();
    }

    public void DisableHitbox()
    {
        attackHitboxScript.DisableHitbox();
    }

    public void AddVelocity(float addedVelocity)
    {
        float direction = isFacingRight ? 1f : -1f;
        rb.linearVelocity += new Vector2(direction * addedVelocity, 0f);
    }
    public void ZeroVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    //in phase 1, melee 1 chains into melee 1 recovery
    // in phase 2 onwards, melee 1 chains straight into melee 2
    //in phase 2 or higher, all ultimates are available.
    //in phase 3, the warden will chain 3 ultimate 1's in a row.
    //in phase 3, the warden will have faster movement speed

    public void DecideAttack()
    {

        // --- PHASE 3 ULTIMATE CHAIN PRIORITY ---
        if (phaseNum == 3 && ult1ChainCount < 3 && ult1ChainCount != 0)
        {
            // If in phase 3 and haven't finished the Ult 1 chain, force Ult 1 (Teleport)
            // Note: We don't check ultimateTimer here because the chain overrides the cooldown.
            Teleport();
            return;
        }

        List<int> availableAttacks = new List<int>();

        // 1. Regular Attacks (Melee/Ranged)
        if (regularTimer <= 0 || rangedTimer <= 0)
        {
            if (distanceToPlayer <= meleeDistance && regularTimer <= 0)
            {
                availableAttacks.Add(1); // Melee 1 (Attack 1)
            }
            // Check for Ranged if Melee wasn't an option or if the enemy wants mixed combat
            else if (distanceToPlayer <= rangedDistance && rangedTimer <= 0)
            {
                availableAttacks.Add(3); // Ranged Attack (Attack 3)
            }
        }

        // 2. Ultimate Attacks
        if (ultimateTimer <= 0 && phaseNum != 1)
        {
            // Ultimate 1 (Teleport/Slam) is always an option when off cooldown
            availableAttacks.Add(4);

            // Ultimate 2 (Ground Lasers) is an option
            availableAttacks.Add(6);

            // Ultimate 3 (Laser Beam) is only an option if the player is within range
            if (distanceToPlayer <= laserDistance)
            {
                availableAttacks.Add(7);
            }
        }
        if (availableAttacks.Count > 0)
        {
            // if an Ultimate is available, prioritize it (Ultimate takes precedence over regular attacks)
            if (ultimateTimer <= 0 && phaseNum != 1)
            {
                // filter down to just the ultimate options (4, 6, 7)
                List<int> availableUlts = availableAttacks.FindAll(a => a >= 4 && a <= 7);

                // For Ultimates, choose one randomly from the available options
                int randomIndex = UnityEngine.Random.Range(0, availableUlts.Count);
                int chosenUlt = availableUlts[randomIndex];

                // Check for Ult 1 (Teleport) specifically, which needs to call Teleport() first
                if (chosenUlt == 4)
                {
                    Teleport(); // This will call SetUpAttackHitboxes(4) via Ult1Teleport coroutine
                }
                else
                {
                    SetUpAttackHitboxes(chosenUlt);
                }
            }
            // B. If no Ultimate is available, choose the regular attack (Melee/Ranged)
            else if (regularTimer <= 0 || rangedTimer <= 0)
            {
                // Priority: Melee (1) > Ranged (3)
                if (availableAttacks.Contains(1) && regularTimer <= 0)
                {
                    SetUpAttackHitboxes(1);
                }
                else if (availableAttacks.Contains(3) && rangedTimer <= 0)
                {
                    SetUpAttackHitboxes(3);
                }
            }
        }
        else
        {
            // If no attacks are available, move towards the player if they are not too close
            if (player != null && distanceToPlayer > meleeDistance)
            {
                MoveTowards(player.position);
            }
            else
            {
                ZeroVelocity(); // Stop moving if the player is within melee range or if the cooldowns are active
            }
        }
    }
    public void SetUpAttackHitboxes(int attackNum)
    {
        FaceTowardsPlayer();
        ZeroVelocity();
        isAttacking = attackNum;
        inAttackState = true;
        switch (attackNum)
        {
            case 1: //melee chain 1
                attackHitboxScript.CustomizeHitbox(attackHitboxes[0]);
                ChangeAnimationState("Attack1");
                break;
            case 2: // melee chain 2
                attackHitboxScript.CustomizeHitbox(attackHitboxes[1]);
                ChangeAnimationState("Attack2");
                break;
            case 3:
                ChangeAnimationState("RangedAttack");
                break;
            case 4: // ultimate 1 (slam down)
                attackHitboxScript.CustomizeHitbox(attackHitboxes[2]);
                ChangeAnimationState("Ultimate1Falling");
                break;
            case 5: //ultimate 1 (landing slam)
                attackHitboxScript.CustomizeHitbox(attackHitboxes[3]);
                ChangeAnimationState("Ultimate1Landing");
                break;
            case 6:
                ChangeAnimationState("Ultimate2");
                break;
            case 7: //ultimate 3 (laser beam)
                attackHitboxScript.CustomizeHitbox(attackHitboxes[4]);
                ChangeAnimationState("Ultimate3");
                break;
            case 8:
                ChangeAnimationState("Attack1Recovery");
                break;
            case 9:
                ChangeAnimationState("Ultimate2Recovery");
                break;
            case 10:
                ChangeAnimationState("Teleport");
                break;
            default:
                break;
        }
    }

    public IEnumerator Ult2LaserSpawn() //summons lasers that "chase" the player every half second.
    {
        int count = 4 + 2 * phaseNum;
        for (int i = 0; i < count; i++)
        {
            // Check if boss is still active and not dead before spawning each laser
            if (isDead || !gameObject.activeSelf || !enabled)
            {
                yield break; // Stop spawning if boss is dead or inactive
            }

            Vector3 newPos = new(player.position.x, transform.position.y, 0f);
            GroundLaserBeam laserScript = Instantiate(laserPrefab, newPos, Quaternion.identity).GetComponent<GroundLaserBeam>();
            laserScript.Initialize(newPos, laserDmg, laserKnockback, 0.3f);
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }
    public void Ult1LaserSpawn()
    {
        int laserRowsCount = 3;
        float spaceBetweenLasers = 1.2f;
        for (int i = 1; i < laserRowsCount + 1; i++)
        {
            GroundLaserBeam laserScript = Instantiate(laserPrefab, new((rb.position.x + spaceBetweenLasers * i), rb.position.y, 0f), Quaternion.identity).GetComponent<GroundLaserBeam>();
            laserScript.Initialize(new((rb.position.x + spaceBetweenLasers * i), rb.position.y, 0f), laserDmg, laserKnockback, 0.2f * (i - 1));
            laserScript = Instantiate(laserPrefab, new((rb.position.x - spaceBetweenLasers * i), rb.position.y, 0f), Quaternion.identity).GetComponent<GroundLaserBeam>();
            laserScript.Initialize(new((rb.position.x - spaceBetweenLasers * i), rb.position.y, 0f), laserDmg, laserKnockback, 0.2f * (i - 1));
        }
    }

    //Ult 1, teleport above player, slam down and cause lasers to sprout from ground.
    public void Teleport()
    {
        isAttacking = 4;
        inAttackState = true;
        ChangeAnimationState("Teleport");
    }
    public IEnumerator Ult1Teleport()
    {
        // Capture player position at the start of the teleport to prevent teleporting to respawn location
        if (player == null || isDead || !gameObject.activeSelf || !enabled)
        {
            yield break; // Stop if player is null, boss is dead, or boss is inactive
        }
        
        float targetX = player.position.x; // Capture player X position at start of teleport
        
        ResetUltimateTimer(); // Reset the ultimate cooldown timer when the attack sequence begins

        rb.gravityScale = 0f;
        rb.transform.position = new(targetX, transform.position.y + 10f, 0f);
        yield return new WaitForSeconds(0.2f);
        
        // Check again after wait to ensure boss is still in correct state
        if (isDead || !gameObject.activeSelf || !enabled || inAttackState == false)
        {
            // Reset gravity and exit if boss state changed
            rb.gravityScale = 3f;
            yield break;
        }

        // Attack 4: Falling Slam
        SetUpAttackHitboxes(4);
        rb.gravityScale = 6f;

        // Wait until the slam hits the ground, but check state periodically
        while (isInAir)
        {
            // Check if boss state changed (e.g., respawned)
            if (isDead || !gameObject.activeSelf || !enabled || inAttackState == false)
            {
                // Reset gravity and exit if boss state changed
                rb.gravityScale = 3f;
                yield break;
            }
            yield return null;
        }

        // Check again before landing attack
        if (isDead || !gameObject.activeSelf || !enabled || inAttackState == false)
        {
            // Reset gravity and exit if boss state changed
            rb.gravityScale = 3f;
            yield break;
        }

        // Attack 5: Landing Slam
        SetUpAttackHitboxes(5);
        rb.gravityScale = 3f;
        SpawnLandingCloudParticle();
        GetComponentInChildren<CinemachineImpulseSource>()?.GenerateImpulse(1.0f);
        Ult1LaserSpawn();

        // --- NEW CHAIN LOGIC ---
        if (phaseNum == 3)
        {
            // Check state before chaining
            if (isDead || !gameObject.activeSelf || !enabled || inAttackState == false)
            {
                ult1ChainCount = 0; // Reset chain count if boss state changed
                yield break;
            }
            
            ult1ChainCount++;
            if (ult1ChainCount < 3)
            {
                // Chain directly into the next Ult 1 (Teleport)
                // Wait for a brief moment before chaining
                yield return new WaitForSeconds(0.5f);
                
                // Final check before chaining
                if (isDead || !gameObject.activeSelf || !enabled || inAttackState == false)
                {
                    ult1ChainCount = 0; // Reset chain count if boss state changed
                    yield break;
                }
                
                Teleport();
                yield break; // Exit the coroutine, the next Teleport will start a new one
            }
            else
            {
                // Chain is complete, reset the counter
                ult1ChainCount = 0;
            }
        }
    }

    public void SpawnLandingCloudParticle()
    {
        Instantiate(slamParticlePrefab, transform.position, Quaternion.identity);
    }

    public void EndMelee1Chain()
    {
        if (phaseNum == 1)
        {
            SetUpAttackHitboxes(8);
        }
        else
        {
            SetUpAttackHitboxes(2); //chain into melee 2
        }
    }

    public void EndUltimate1And2()
    {
        SetUpAttackHitboxes(9);
    }
    public void ResetUltimateTimer()
    {
        ultimateTimer = ultimateCooldown / Mathf.Max(phaseNum - 1, 1);
    }

    public void ResetAttackTimer()
    {
        regularTimer = regularAttackCooldown;
    }

    public void ResetRangedTimer()
    {
        rangedTimer = rangedAttackCooldown;
    }

    public void InstBullet()
    {
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        GuardBullet projScript = projectile.GetComponent<GuardBullet>();
        projScript.Initialize(rangedDmg, rangedSpeed, rangedLifeSpan);
        return;
    }

    private void OnDrawGizmosSelected()
    {
        // // Ensure this runs only in the editor when the object is selected
        // if (!Application.isEditor) return;

        // Get the Warden's position for the start of the rays
        Vector3 wardenPosition = transform.position;

        // --- Melee Distance Gizmo (e.g., Yellow) ---
        Gizmos.color = Color.yellow;
        // Draw the line for the positive X direction (right)
        Gizmos.DrawLine(wardenPosition, wardenPosition + Vector3.right * meleeDistance);
        // Draw the line for the negative X direction (left)
        Gizmos.DrawLine(wardenPosition, wardenPosition + Vector3.left * meleeDistance);

        Gizmos.color = Color.magenta;
        // Draw the line for the positive X direction (right)
        Gizmos.DrawLine(wardenPosition, wardenPosition + Vector3.right * laserDistance);
        // Draw the line for the negative X direction (left)
        Gizmos.DrawLine(wardenPosition, wardenPosition + Vector3.left * laserDistance);

        // --- Ranged Distance Gizmo (e.g., Blue) ---
        Gizmos.color = Color.blue;
        // Draw the line for the positive X direction (right)
        Gizmos.DrawLine(wardenPosition, wardenPosition + Vector3.right * rangedDistance);
        // Draw the line for the negative X direction (left)
        Gizmos.DrawLine(wardenPosition, wardenPosition + Vector3.left * rangedDistance);

        // Optional: Draw a small sphere at the end to make it more visible
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(wardenPosition + Vector3.right * meleeDistance, 0.1f);
        Gizmos.DrawWireSphere(wardenPosition + Vector3.left * meleeDistance, 0.1f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wardenPosition + Vector3.right * laserDistance, 0.15f);
        Gizmos.DrawWireSphere(wardenPosition + Vector3.left * laserDistance, 0.15f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(wardenPosition + Vector3.right * rangedDistance, 0.2f);
        Gizmos.DrawWireSphere(wardenPosition + Vector3.left * rangedDistance, 0.2f);
    }
}
