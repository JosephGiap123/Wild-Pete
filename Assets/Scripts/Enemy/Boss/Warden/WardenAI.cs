using UnityEngine;

public class WardenAI : EnemyBase
{

    [Header("Animations")]
    [SerializeField] private Animator anim;
    private string currentState = "Idle";

    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 2f;
    public bool isFacingRight = true;

    [Header("Combat Setting")]
    public int phaseNum = 1;
    protected bool isDead = false;
    protected bool isInAir = false;
    protected bool inAttackState = false;
    protected int isAttacking = 0;

    [Header("Combat Stats")]
    [SerializeField] protected int Melee1Dmg = 5;
    [SerializeField] protected Vector2 Melee1Offset;
    [SerializeField] protected Vector2 Melee1Size;
    [SerializeField] protected Vector2 Melee1Knockback = new(5f, 2f);
    [SerializeField] protected Vector2 Melee2Offset;
    [SerializeField] protected Vector2 Melee2Size;
    [SerializeField] protected int Melee2Dmg = 8;
    [SerializeField] protected Vector2 Melee2Knockback = new(7f, 3f);
    [SerializeField] protected int rangedDmg = 6;
    [SerializeField] protected float rangedSpeed = 20f;
    [SerializeField] protected float rangedLifeSpan = 4f;
    [SerializeField] protected Vector2 RangedKnockback = new(4f, 1f);
    [SerializeField] protected int Ultimate1Dmg = 7; //teleport slam, this dmg represents the initial slam part.
    [SerializeField] protected Vector2 Ultimate1HitboxOffset;
    [SerializeField] protected Vector2 Ultimate1HitboxSize;
    [SerializeField] protected Vector2 Ultimate1Knockback = new(10f, -5f);
    [SerializeField] protected int Ultimate2Dmg = 6; //ground slam, lasers come from ground.
    [SerializeField] protected Vector2 Ultimate2Knockback = new(8f, 4f);
    [SerializeField] protected int Ultimate3Dmg = 3; //laser beam, continuous hits.
    [SerializeField] protected Vector2 Ultimate3Knockback = new(12f, 6f);
    [SerializeField] protected Vector2 Ultimate3HitboxOffset;
    [SerializeField] protected Vector2 Ultimate3HitboxSize;


    [Header("Combat References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private BoxCollider2D boxAttackHitbox;
    [SerializeField] private WardenAttackHitbox attackHitboxScript;

    private Transform player;
    private void OnEnable()
    {
        GameManager.OnPlayerSet += HandlePlayerSet;
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
        if (Input.GetKeyDown(KeyCode.K))
        {
            SetUpAttackHitboxes(1);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            SetUpAttackHitboxes(6);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            SetUpAttackHitboxes(3);
        }
        if (inAttackState) return;
        AnimationControl();
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
        else if (inAttackState)
        {
            switch (isAttacking)
            {
                case 1:
                    ChangeAnimationState("Attack1"); //melee chain 1
                    break;
                case 2:
                    ChangeAnimationState("Attack2"); //melee chain 2
                    break;
                case 3:
                    ChangeAnimationState("RangedAttack"); //laser shot
                    break;
                case 4:
                    ChangeAnimationState("Ultimate1"); //teleport high above player, slam down, cause lasers to rise from ground
                    break;
                case 5:
                    ChangeAnimationState("Ultimate2"); //plunge laser sword into ground, lasers come from ground.
                    break;
                case 6:
                    ChangeAnimationState("Ultimate3"); //laser beam
                    break;
                case 7: //Attack1 Recovery
                    ChangeAnimationState("Attack1Recovery");
                    break;
                case 8: //Slam Recovery
                    ChangeAnimationState("Ultimate2Recovery");
                    break;
                default:
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

    public void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        projectileSpawnPoint.localRotation = Quaternion.Euler(0, 0, isFacingRight ? 0 : 180);
        localScale.x *= -1;
        transform.localScale = localScale;
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
        attackHitboxScript.ActivateBox();
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
    public void SetUpAttackHitboxes(int attackNum)
    {
        isAttacking = attackNum;
        inAttackState = true;
        switch (attackNum)
        {
            case 1: //melee chain 1
                attackHitboxScript.ChangeHitboxBox(new Vector2(1.0f, 0.5f), new Vector2(2.0f, 1.5f), Melee1Dmg, Melee1Knockback);
                ChangeAnimationState("Attack1");
                break;
            case 2: // melee chain 2
                attackHitboxScript.ChangeHitboxBox(new Vector2(1.5f, 0.5f), new Vector2(2.5f, 1.5f), Melee2Dmg, Melee2Knockback);
                ChangeAnimationState("Attack2");
                break;
            case 3:
                ChangeAnimationState("RangedAttack");
                break;
            case 4: // ultimate 1 (slam down)
                attackHitboxScript.ChangeHitboxBox(Ultimate1HitboxOffset, Ultimate1HitboxSize, Ultimate1Dmg, Ultimate1Knockback);
                ChangeAnimationState("Ultimate1");
                break;
            case 5:
                ChangeAnimationState("Ultimate2");
                break;
            case 6: //ultimate 3 (laser beam)
                attackHitboxScript.ChangeHitboxBox(Ultimate3HitboxOffset, Ultimate3HitboxSize, Ultimate3Dmg, Ultimate3Knockback);
                ChangeAnimationState("Ultimate3");
                break;
            case 7:
                ChangeAnimationState("Attack1Recovery");
                break;
            case 8:
                ChangeAnimationState("Ultimate2Recovery");
                break;
            default:
                break;
        }
    }

    public void Ult2LaserSpawn(int count) //summons lasers that "chase" the player every 1 second.
    {

    }

    public void EndMelee1Chain()
    {
        if (phaseNum == 1)
        {
            SetUpAttackHitboxes(7);
        }
        else
        {
            SetUpAttackHitboxes(2); //chain into melee 2
        }
    }

    public void EndUltimate1And2()
    {
        SetUpAttackHitboxes(8);
    }

    public void InstBullet()
    {
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        GuardBullet projScript = projectile.GetComponent<GuardBullet>();
        projScript.Initialize(rangedDmg, rangedSpeed, rangedLifeSpan);
        return;
    }
}
