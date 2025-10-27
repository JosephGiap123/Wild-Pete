using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAI : EnemyBase
{

    private enum GuardState
    {
        Idle,
        Patrol,
        Alert,
        Attack,
        Dead
    }
    [Header("Animations")]
    [SerializeField] private Animator anim;
    private string currentState = "Idle";

    [Header("Guard Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private BoxCollider2D groundCheckBox;
    [SerializeField] private LayerMask groundLayer;
    public bool isFacingRight = true;
    public bool isInAir = false;
    private bool moving = false;

    [Header("Combat Settings")]
    private int isAttacking = 0; // 0 = no attack, 1 = melee1, 2 = melee2, 3 = ranged, 4 = dash attack
    private int attackChain = 0;
    private bool isHurt = false;
    private bool isDead = false;
    [SerializeField] private float meleeRange = 1f;
    [SerializeField] private float dashRange = 5f;

    [Header("Attack Settings")]
    [SerializeField] private int melee1Damage = 2;
    [SerializeField] private Vector2 melee1HitboxSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 melee1HitboxOffset = new Vector2(0.5f, 0f);
    [SerializeField] private Vector2 melee1Knockback = new Vector2(2f, 1f);
    [SerializeField] private int melee2Damage = 3;
    [SerializeField] private Vector2 melee2HitboxSize = new Vector2(1.2f, 1f);
    [SerializeField] private Vector2 melee2HitboxOffset = new Vector2(0.6f, 0f);
    [SerializeField] private Vector2 melee2Knockback = new Vector2(3f, 1f);
    [SerializeField] private int rangedDamage = 2;
    [SerializeField] private float rangedAttackCooldown = 5f;
    [SerializeField] private float bulletSpeed = 14f;
    [SerializeField] private float bulletLifeTime = 3f;
    [SerializeField] private int dashAttackDamage = 4;
    [SerializeField] private Vector2 dashAttackHitboxSize = new Vector2(1.5f, 1f);
    [SerializeField] private Vector2 dashAttackHitboxOffset = new Vector2(0.75f, 0f);
    [SerializeField] private Vector2 dashAttackKnockback = new Vector2(4f, 1f);
    [SerializeField] private float dashAttackSpeed = 8f;

    [Header("Combat References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private BoxCollider2D boxAttackHitbox;
    [SerializeField] private AttackHitBoxGuard attackHitboxScript;

    protected override void Awake()
    {
        base.Awake();
        // Additional initialization if needed
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y) && isAttacking == 0 && !isDead && !isHurt)
        {
            SetUpAttackHitbox(1);
        }
        else if (Input.GetKeyDown(KeyCode.U) && isAttacking == 0 && !isDead && !isHurt)
        {
            SetUpAttackHitbox(2);
        }
        else if (Input.GetKeyDown(KeyCode.I) && isAttacking == 0 && !isDead && !isHurt)
        {
            SetUpAttackHitbox(4);
        }
        else if (Input.GetKeyDown(KeyCode.O) && isAttacking == 0 && !isDead && !isHurt)
        {
            RangedAttack();
        }
        else if (Input.GetKeyDown(KeyCode.P) && !isInAir && !isDead && !isHurt && isAttacking == 0)
        {
            DoAttack();
        }
        IsGroundedCheck();
        if (moving)
            HandleFlip();
        AnimationControl();
    }
    protected IEnumerator Death()
    {
        isDead = true;
        yield return new WaitForSeconds(2f); //wait for death animation to finish
        base.Die();
    }

    protected void Jump()
    {
        if (!isInAir && !isDead && !isHurt && isAttacking == 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isInAir = true;
        }
    }
    public void EndAttack()
    {
        isAttacking = 0;
    }

    public void InstBullet()
    {
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        GuardBullet projScript = projectile.GetComponent<GuardBullet>();
        projScript.Initialize(rangedDamage, bulletSpeed, bulletLifeTime);
        return;
    }
    public override void Hurt(int dmg, Vector2 knockbackForce)
    {
        if (isDead) return;

        health -= dmg;
        Debug.Log(health);
        rb.linearVelocity += knockbackForce;

        StartHurtAnim(dmg);

        if (!IsAlive())
        {
            StartCoroutine(Death());
        }
    }
    public override void EndHurtState()
    {
        isHurt = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void StartHurtAnim(int dmg)
    {
        float damageFlashTime = Mathf.Max(0.2f, (dmg / maxHealth));
        isHurt = true;
        ChangeAnimationState("Hurt");
        StartCoroutine(DamageFlash(damageFlashTime));
    }

    void IsGroundedCheck()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheckBox.bounds.center, groundCheckBox.bounds.size, 0f, groundLayer);
        isInAir = colliders.Length == 0;
    }

    public void ChangeAnimationState(string newState)
    {
        if (newState == currentState) return;
        anim.Play(newState, 0, 0f);
        currentState = newState;
    }

    protected void AnimationControl()
    {
        if (isDead)
        {
            ChangeAnimationState("Death");
            return;
        }
        else if (isHurt)
        {
            //played outside of this controller.
            return;
        }
        else if (isAttacking != 0)
        {
            switch (isAttacking)
            {
                case 1:
                    ChangeAnimationState("Attack1");
                    break;
                case 2:
                    ChangeAnimationState("Attack2");
                    break;
                case 3:
                    ChangeAnimationState("RangedAttack");
                    break;
                case 4:
                    ChangeAnimationState("AttackDash");
                    break;
            }
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
            if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed)
            {
                ChangeAnimationState("Run");
            }
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                ChangeAnimationState("Walk");
            }
            else
            {
                ChangeAnimationState("Idle");
            }
        }
    }

    protected virtual void HandleFlip()
    {
        if (isAttacking != 0 || isHurt || isDead) return;
        if ((rb.linearVelocity.x > 0.25f && !isFacingRight) || (rb.linearVelocity.x < 0.25f && isFacingRight))
            FlipSprite();
    }

    public virtual void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        projectileSpawnPoint.localRotation = Quaternion.Euler(0, 0, isFacingRight ? 0 : 180);
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    //Attacks
    public void DoAttack()
    {
        //Choose an attack based on what is done.
        //1 = melee1, 2 =melee2, 3 = ranged, 4 = dash
        //melee2 MUST be called after melee1.
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (!playerTransform || isDead || isHurt) return;
        float distanceToPlayer = Vector2.Distance(playerTransform.position, transform.position);
        Debug.Log(distanceToPlayer);
        if (distanceToPlayer <= dashRange * 3f)
        {
            //Decide between melee or dash attack
            //flip towards player if not facing them.
            if ((playerTransform.position.x > transform.position.x && !isFacingRight) || (playerTransform.position.x < transform.position.x && isFacingRight))
            {
                Debug.Log("Flip");
                FlipSprite();
            }
            if (distanceToPlayer <= meleeRange)
            {
                int attackChoice = Random.Range(0, 2); //0 = nothing ,1 = melee, 2 = dash
                if (attackChoice < 2)
                {
                    int meleeChoice = Random.Range(1, 5); //1 = melee1, 2 = dash.
                    Debug.Log(meleeChoice);
                    if (meleeChoice == 4)
                    {
                        Debug.Log("Random Dash");
                        SetUpAttackHitbox(4);
                        //choose to dash
                    }
                    else
                    {
                        Debug.Log("Melee");
                        //choose to melee attack
                        MeleeAttack();
                    }
                }
            }
            else if (distanceToPlayer <= dashRange)
            {
                //Dash attack
                Debug.Log("Forced Dash");
                SetUpAttackHitbox(4);
            }
            else if (distanceToPlayer <= dashRange * 3.5f)
            {
                Debug.Log("Ranged");
                //Ranged attack if player is far but not too far
                RangedAttack();
            }
        }
        else
        {
            //Out of range, do nothing or move closer
        }
    }

    public void MeleeAttack()
    {
        if (attackChain == 0) //first melee Attack
        {
            isAttacking = 1;
            SetUpAttackHitbox(1);
            attackChain++;
        }
        else
        { //second melee attack
            isAttacking = 2;
            SetUpAttackHitbox(2);
            attackChain = 0;
        }
    }

    public void RangedAttack()
    {
        isAttacking = 3;
        ChangeAnimationState("RangedAttack");
    }

    public void SetUpAttackHitbox(int attackNum)
    {
        switch (attackNum)
        {
            case 1:
                isAttacking = 1;
                attackHitboxScript.ChangeHitboxBox(melee1HitboxOffset, melee1HitboxSize, melee1Damage, melee1Knockback);
                ChangeAnimationState("Attack1");
                break;
            case 2:
                isAttacking = 2;
                attackHitboxScript.ChangeHitboxBox(melee2HitboxOffset, melee2HitboxSize, melee2Damage, melee2Knockback);
                ChangeAnimationState("Attack2");
                break;
            case 4:
                isAttacking = 4;
                attackHitboxScript.ChangeHitboxBox(dashAttackHitboxOffset, dashAttackHitboxSize, dashAttackDamage, dashAttackKnockback);
                ChangeAnimationState("AttackDash");
                break;
            default:
                break;
        }
    }

    public void DashVelocityIncrease()
    {
        float dashSpeed = isFacingRight ? dashAttackSpeed : -1 * dashAttackSpeed;
        rb.linearVelocity = new Vector2(dashSpeed, rb.linearVelocity.y);
    }

    public void EndDash()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}
