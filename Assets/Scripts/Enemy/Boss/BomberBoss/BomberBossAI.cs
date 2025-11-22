using UnityEngine;
using System.Collections;

public class BomberBoss : EnemyBase, IHasFacing
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
    protected bool isInAir = false;
    protected bool inAttackState = true;
    protected int isAttacking = 0;
    protected bool isInvincible = false;

    [Header("Combat Stats")]


    [Header("Combat References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject rocketJumpExplosionPrefab;
    [SerializeField] private GenericAttackHitbox attackHitboxScript;
    [SerializeField] private GameObject phaseChangeParticles;
    [Header("Rocket Spawn Points")]
    [SerializeField] private Transform standRocketProjSpawnPt;
    [SerializeField] private Transform airDiagonalRocketProjSpawnPt;
    [SerializeField] private Transform airStraightRocketProjSpawnPt;

    protected float distanceToPlayer = 100f;


    [Header("Particles")]
    [SerializeField] private ParticleSystem dashParticle;
    [SerializeField] private ParticleSystem shootStandingParticle;
    [SerializeField] private ParticleSystem shootAirDiagonalParticle;
    [SerializeField] private ParticleSystem shootAirStraightParticle;
    [SerializeField] BossHPBarInteractor hpBarInteractor;
    private Transform player;

    protected override void Awake()
    {
        base.Awake();
        hpBarInteractor.ShowHealthBar(true);
    }

    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
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

        if (Input.GetKeyDown(KeyCode.K))
        {
            SetUpAttackHitboxes(1);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ShootRocket();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            RocketJump();
        }
        AnimationControl();
        IsGroundedCheck();
        if (inAttackState || isDead) return;
    }

    public void SetUpAndSpawnParticle(int particleNum)
    {
        ParticleSystem newParticle = null;
        if (particleNum == 1)
        {
            newParticle = dashParticle;
        }
        else if (particleNum == 2)
        {
            newParticle = shootStandingParticle;
        }
        else if (particleNum == 3)
        {
            newParticle = shootAirDiagonalParticle;
        }
        else if (particleNum == 4)
        {
            newParticle = shootAirStraightParticle;
        }
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.rotation3D = new(0f, isFacingRight ? 0f : 180f);
        newParticle.Emit(emitParams, 1);
        Debug.Log("Spawned particle");
    }

    protected void AnimationControl()
    {
        if (inAttackState || isDead) return;
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
            // UpdateRunLoopAudio(false);
            return;
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.25f)
            {
                ChangeAnimationState("Run");
            }
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.2f)
            {
                ChangeAnimationState("Walk");
            }
            else
            {
                ChangeAnimationState("Idle");
            }
        }
    }

    protected void ChangeAnimationState(string newState)
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
        if (speed < 0) speed = moveSpeed;
    }

    // private void UpdateRunLoopAudio(bool shouldRun)
    // {
    //     if (audioManager != null)
    //     {
    //         if (shouldRun) audioManager.StartRunLoop();
    //         else audioManager.StopRunLoop();
    //     }
    // }

    public override void Hurt(int dmg, Vector2 knockbackForce)
    {
        if (isInvincible) return;
        // audioManager?.PlayHurt();
        StartCoroutine(base.DamageFlash(0.2f));
        base.Hurt(dmg, knockbackForce);
        hpBarInteractor.UpdateHealthVisual();
    }

    public void FaceTowardsPlayer()
    {
        if (isFacingRight && player.position.x < transform.position.x || !isFacingRight && player.position.x > transform.position.x)
        {
            FlipSprite();
        }
    }

    protected IEnumerator Death()
    {
        isDead = true;
        // audioManager?.StopRunLoop();
        // audioManager?.PlayDeath();
        ZeroVelocity(); // Stop all movement
        dropItemsOnDeath.DropItems();
        yield return new WaitForSeconds(4f); //wait for death animation to finish
        hpBarInteractor.ShowHealthBar(false);
        Die();
    }

    protected override void Die()
    {
        base.Die();
    }

    public override void Respawn(Vector2? position = null, bool? facingRight = null)
    {
        base.Respawn(position, facingRight);
    }

    public override void FlipSprite()
    {
        base.FlipSprite();
    }

    public void EndAttack()
    {
        Debug.Log("End attack");
        inAttackState = false;
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

    public void AddVelocity(float addedVelocity)
    {
        rb.linearVelocity += new Vector2(addedVelocity * (isFacingRight ? 1 : -1), 0f);
    }

    public void AddYVelocity(float addedVelocity)
    {
        rb.linearVelocity += new Vector2(0f, addedVelocity);
    }

    public void ZeroVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void SetUpAttackHitboxes(int attackNum)
    {
        FaceTowardsPlayer();
        ZeroVelocity();
        isAttacking = attackNum;
        inAttackState = true;
        if (attackNum == 1)
        {
            attackHitboxScript.CustomizeHitbox(attackHitboxes[0]);
            ChangeAnimationState("Melee1");
        }
        else if (attackNum == 2)
        {
            attackHitboxScript.CustomizeHitbox(attackHitboxes[1]);
            ChangeAnimationState("Dash");
        }
    }

    public void EndMelee1Chain()
    {
        if (phaseNum == 1)
        {
            ChangeAnimationState("Melee1Recovery");
        }
        else
        {
            SetUpAttackHitboxes(2); //chain into dash attack
        }
    }

    public float DetermineXDistanceToPlayer()
    {
        float xDistanceToPlayer = player.position.x - transform.position.x;
        return Mathf.Abs(xDistanceToPlayer);
    }

    public void ShootRocket()
    {
        if (isInAir)
        {
            //if the enemy is in the air can do 2 things: either shoot straight down or shoot diagonally.
            //if the player is to the left of the enemy, shoot diagonally to the right(using the block back to go towards the player)
            //if the enemy is to the right, shoot to the left.
            //if it is relatively below the player in x value, shoot straight down.
            ZeroVelocity();
            FaceTowardsPlayer();
            isAttacking = 3;
            inAttackState = true;

            float xDistanceToPlayer = DetermineXDistanceToPlayer();
            if (xDistanceToPlayer > 1.2f)
            {  //do diagonal.
                FaceTowardsPlayer();
                FlipSprite(); //face away from player.
                ChangeAnimationState("AerialDiagonalShot");
            }
            else
            {
                ChangeAnimationState("AerialDownShot");
            }
        }
        else
        {
            ZeroVelocity();
            FaceTowardsPlayer();
            isAttacking = 3;
            inAttackState = true;
            attackHitboxScript.CustomizeHitbox(attackHitboxes[2]);
            ChangeAnimationState("GroundedRanged");
        }
    }

    public void RocketJump()
    {
        ZeroVelocity();
        FaceTowardsPlayer();
        isAttacking = 4;
        inAttackState = true;
        ChangeAnimationState("RocketJump");
    }

    public void SpawnRocket(float rawAngle)
    {
        Transform spawnPoint = null;
        float angle = rawAngle;
        if (rawAngle == 315f)
        {
            Debug.Log("Spawn diagonal rocket");
            spawnPoint = airDiagonalRocketProjSpawnPt;
            angle = isFacingRight ? 315f : 225f;
        }
        else if (rawAngle == 0f)
        {
            Debug.Log("Spawn standing rocket");
            spawnPoint = standRocketProjSpawnPt;
            angle = isFacingRight ? 0f : 180f;
        }
        else
        {
            Debug.Log("Spawn straight down rocket");
            spawnPoint = airStraightRocketProjSpawnPt;
            // For straight down, use -90 degrees (or 270, both point straight down)
            angle = -90f; // Straight down
        }

        if (spawnPoint == null)
        {
            Debug.LogError($"BomberBoss: Spawn point is null for angle {rawAngle}! Check spawn point assignments.");
            return;
        }

        Vector3 spawnPosition = spawnPoint.position;

        GameObject newRocket = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        newRocket.GetComponent<RPGRocket>().Initialize(10, 10, angle, 3f);
    }

    public void SpawnRocketJumpExplosion()
    {
        GameObject newRocketJumpExplosion = Instantiate(rocketJumpExplosionPrefab, transform.position, Quaternion.identity);
        newRocketJumpExplosion.GetComponent<ExplosionCloud>().Initialize(null);
    }
}
