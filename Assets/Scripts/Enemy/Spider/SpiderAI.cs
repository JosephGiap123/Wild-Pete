using UnityEngine;

public class SpiderAI : EnemyBase
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 2.5f;
    private bool isInAir = false;
    public LayerMask groundLayer;
    [SerializeField] private BoxCollider2D groundCheckBox;

    [Header("Combat Settings")]
    private int isAttacking = 0; // 0 = no attack, 1 = melee1, 2 = dash, 3 = string shot

    [Header("Attack Settings")]
    [SerializeField] private int rangedDamage = 2;
    [SerializeField] private float rangedAttackCooldown = 5f;
    [SerializeField] private float bulletSpeed = 14f;
    [SerializeField] private float bulletLifeTime = 3f;
    [SerializeField] private float dashAttackSpeed = 8f;
    [Header("Attack References")]
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] private GenericAttackHitbox attackHitboxScript;

    [Header("Animation References")]
    [SerializeField] Animator anim;
    private string currentState = "SpiderIdle";

    private Transform player;

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
        AnimationControl();
        if (isDead || isHurt) return;
        IsGroundedCheck();
        if (Input.GetKeyDown(KeyCode.K))
        { //ATTACK 1
            SetUpAttackHitbox(1);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        { //ATTACK DASH
            SetUpAttackHitbox(2);
        }
    }
    public void ChangeAnimationState(string newState)
    {
        if (newState == currentState) return;
        anim.Play(newState, 0, 0f);
        currentState = newState;
    }

    protected virtual void HandleFlip()
    {
        if (isAttacking != 0 || isHurt || isDead) return;
        if ((rb.linearVelocity.x > 0.25f && !isFacingRight) || (rb.linearVelocity.x < 0.25f && isFacingRight))
            FlipSprite();
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
    public void FaceTowardsPlayer()
    {
        // Debug.Log(player.position.x + " " + this.gameObject.transform.position.x);
        if (isFacingRight && player.position.x < transform.position.x)
        {
            FlipSprite();
        }
        else if (!isFacingRight && player.position.x > transform.position.x)
        {
            FlipSprite();
        }
    }
    protected void AnimationControl()
    {
        if (isDead)
        {
            ChangeAnimationState("SpiderDeath");
            return;
        }
        else if (isAttacking != 0 || isHurt)
        {
            //played outside of animcontrol
            return;
        }
        else if (isInAir)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                ChangeAnimationState("SpiderRising");
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                ChangeAnimationState("SpiderFalling");
            }
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed + 0.2f)
            {
                ChangeAnimationState("SpiderWalk");
            }
            else
            {
                ChangeAnimationState("SpiderIdle");
            }
        }
    }

    void IsGroundedCheck()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheckBox.bounds.center, groundCheckBox.bounds.size, 0f, groundLayer);
        bool wasInAir = isInAir;
        isInAir = colliders.Length == 0;
    }

    //attacks
    public void SetUpAttackHitbox(int attackNum)
    {
        FaceTowardsPlayer();
        switch (attackNum)
        {
            case 1: //melee
                isAttacking = 1;
                attackHitboxScript.CustomizeHitbox(attackHitboxes[0]);
                ChangeAnimationState("SpiderMelee");
                break;
            case 2: //lunge
                isAttacking = 2;
                attackHitboxScript.CustomizeHitbox(attackHitboxes[1]);
                ChangeAnimationState("SpiderLunge");
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

    public void StopMoving()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void EndAttack()
    {
        isAttacking = 0;
    }
    public void InstBullet()
    {
        return;
    }
}
