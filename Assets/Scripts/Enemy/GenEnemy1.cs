using System.Collections;
using UnityEngine;

public class GenEnemy : EnemyBase
{
    [Header("Components")]
    [SerializeField] private Animator anim;

    [Header("Stats")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private LayerMask visionMask;
    [SerializeField] private float viewAngle = 90f; // field of view cone width
    [SerializeField] private float chaseMemoryTime = 3f; // seconds to keep chasing after losing sight

    private float chaseTimer = 0f;
    private int currentPatrolIndex = 0;
    private Transform player;
    private bool facingRight = true;

    private bool hurtStun = false;
    private Coroutine hurtCoroutine;
    private bool waiting = false;

    private EnemyState currentState = EnemyState.Idle;

    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                Debug.Log("Player found by tag.");
            }
        }
    }

    private void OnDisable()
    {
        GameManager.OnPlayerSet -= HandlePlayerSet;

        // optional: clear player reference
        player = null;
    }

    private void HandlePlayerSet(GameObject playerObj)
    {
        Debug.Log("HandlePlayerSet called");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        Debug.Log("Enemy State: " + currentState);

        if (currentState == EnemyState.Dead) return;
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                Patrol();
                if (CanSeePlayer()) ChangeState(EnemyState.Alert);
                break;

            case EnemyState.Patrol:
                Patrol();
                if (CanSeePlayer()) ChangeState(EnemyState.Alert);
                break;

            case EnemyState.Alert:
                if (CanSeePlayer())
                {
                    // reset memory timer if we see player
                    chaseTimer = chaseMemoryTime;
                    ChasePlayer(distanceToPlayer);
                }
                else
                {
                    // tick down timer if we lost sight
                    chaseTimer -= Time.deltaTime;

                    if (chaseTimer > 0f)
                    {
                        // still chase while timer is active
                        ChasePlayer(distanceToPlayer);
                    }
                    else
                    {
                        // out of time → return to patrol
                        ChangeState(EnemyState.Patrol);
                    }
                }
                break;

            case EnemyState.Attack:
                if (distanceToPlayer > attackRange)
                    ChangeState(EnemyState.Alert);
                break;

            case EnemyState.Hurt:
                // handled by coroutine
                break;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        // 1. Distance check
        if (distance > detectionRange) return false;

        // 2. Cone check
        Vector2 facingDir = facingRight ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(facingDir, dirToPlayer);
        if (angle > viewAngle / 2f) return false;

        // 3A. TEST MODE (ignores layers → always checks)
        RaycastHit2D testHit = Physics2D.Raycast(transform.position, dirToPlayer, detectionRange);
        Debug.DrawRay(transform.position, dirToPlayer * detectionRange, Color.cyan);

        if (testHit.collider != null)
        {
            Debug.Log("TEST hit: " + testHit.collider.name + " on layer " +
                      LayerMask.LayerToName(testHit.collider.gameObject.layer));

            if (testHit.collider.CompareTag("Player"))
            {
                Debug.Log("✅ Player spotted (test mode, ignoring visionMask)");
                return true;
            }
        }

        // 3B. NORMAL MODE (with visionMask)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, detectionRange, visionMask);
        Debug.DrawRay(transform.position, dirToPlayer * detectionRange, Color.green);

        if (hit.collider != null)
        {
            Debug.Log("NORMAL hit: " + hit.collider.name + " on layer " +
                      LayerMask.LayerToName(hit.collider.gameObject.layer));

            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("✅ Player spotted in FOV (with visionMask)");
                return true;
            }
        }

        Debug.Log("❌ Ray hit nothing or not the Player");
        return false;
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points set!");
            return;
        }

        if (waiting) return;

        anim.Play("Walk");

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 targetPosition = new Vector2(targetPoint.position.x, transform.position.y);

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            patrolSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, targetPosition) < 0.2f)
        {
            StartCoroutine(WaitAndSwitch());
        }
    }

    private IEnumerator WaitAndSwitch()
    {
        waiting = true;
        anim.Play("Idle");
        yield return new WaitForSeconds(1f);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        Flip();

        waiting = false;
    }

    private void ChasePlayer(float distanceToPlayer)
    {
        anim.Play("Run");

        // Too far → return to patrol
        if (distanceToPlayer > detectionRange * 2f)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            chaseSpeed * Time.deltaTime
        );

        // Flip sprite to face player
        if ((player.position.x > transform.position.x && !facingRight) ||
            (player.position.x < transform.position.x && facingRight))
        {
            Flip();
        }

        // Close enough → attack
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            anim.Play("Attack");
        }
    }

    public override void Hurt(int dmg)
    {
        health -= dmg;
        Debug.Log($"Enemy HP: {health}");

        if (hurtCoroutine != null)
            StopCoroutine(hurtCoroutine);

        hurtCoroutine = StartCoroutine(HurtAnim());

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator HurtAnim()
    {
        hurtStun = true;
        ChangeState(EnemyState.Hurt);
        anim.Play("Hurt");
        StartCoroutine(DamageFlash(0.2f));
        yield return new WaitWhile(() => hurtStun);

        ChangeState(EnemyState.Idle);
        anim.Play("Idle");
    }

    public void EndHurtState()
    {
        hurtStun = false;
    }

    public void EndAttackState()
    {
        ChangeState(EnemyState.Alert);
    }

    public void DoAttackDamage()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            Debug.Log("Player takes damage!");
            // player.GetComponent<PlayerHealth>().TakeDamage(10);
        }
    }

    public void OnDeathAnimationComplete()
    {
        Destroy(gameObject);
    }

    protected override void Die()
    {
        ChangeState(EnemyState.Dead);
        anim.Play("Die");
    }

    private void ChangeState(EnemyState newState)
    {
        Debug.Log($"State changed: {currentState} → {newState}");
        currentState = newState;

        if (newState == EnemyState.Alert)
        {
            chaseTimer = chaseMemoryTime;
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    // 🔎 Debug vision cone in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector2 facingDir = facingRight ? Vector2.right : Vector2.left;
        Vector2 leftBoundary = Quaternion.Euler(0, 0, viewAngle / 2) * facingDir;
        Vector2 rightBoundary = Quaternion.Euler(0, 0, -viewAngle / 2) * facingDir;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftBoundary * detectionRange);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightBoundary * detectionRange);
    }
}

public enum EnemyState
{
    Idle,
    Patrol,
    Alert,
    Attack,
    Hurt,
    Dead
}
