using UnityEngine;

public class WardenAI : EnemyBase
{

    [Header("Animations")]
    [SerializeField] private Animator anim;
    private string currentState = "Idle";

    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 2f;

    [Header("Combat Setting")]
    protected bool isDead = false;
    protected bool isInAir = false;
    protected int isAttacking = 0;

    [Header("Combat References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private BoxCollider2D boxAttackHitbox;
    [SerializeField] private AttackHitBoxGuard attackHitboxScript;

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
        else if (isAttacking != 0)
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

}
