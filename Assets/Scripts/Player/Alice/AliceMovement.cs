using System.Collections;
using UnityEngine;

public class AliceMovement2D : BasePlayerMovement2D
{
    // --- AUDIO ---
    private AliceAudioManager audioMgr;

    [Header("Audio – Run Loop")]
    [SerializeField] private float runMinSpeed = 0.2f; // min horiz speed to count as running

    [Header("Throw Settings")]
    [SerializeField] private float throwDuration = 0.5f; // safety timer so throw never freezes

    protected override void Awake()
    {
        base.Awake();
        audioMgr = GetComponent<AliceAudioManager>() ?? GetComponentInChildren<AliceAudioManager>();
    }

    // Alice-specific sizes (kept from your code)
    protected override float CharWidth => 0.6f;
    protected override Vector2 CrouchOffset => new(0, -0.1238286f);
    protected override Vector2 CrouchSize => new(CharWidth, 0.7002138f);
    protected override Vector2 StandOffset => new(0, 0.1042647f);
    protected override Vector2 StandSize => new(CharWidth, 1.1564f);

    [Header("Alice Specific Combat Settings")]
    [SerializeField] protected int melee1Damage = 4;
    [SerializeField] protected float melee1Size = 0.8f;
    [SerializeField] protected Vector2 melee1Offset = new(0.5f, 0f);
    [SerializeField] protected Vector2 melee1Knockback = new(1f, 5f);
    [SerializeField] protected int melee2Damage = 5;
    [SerializeField] protected float melee2Size = 0.8f;
    [SerializeField] protected Vector2 melee2Offset = new(0.5f, 0f);
    [SerializeField] protected Vector2 melee2Knockback = new(1f, -3f);
    [SerializeField] protected int crouchAttackDamage = 2;
    [SerializeField] protected Vector2 crouchAttackSize = new(1f, 0.55f);
    [SerializeField] protected Vector2 crouchAttackOffset = new(0.3f, -0.25f);
    [SerializeField] protected Vector2 crouchAttackKnockback = new(2f, 1f);
    [SerializeField] protected int aerialAttackDamage = 4;
    [SerializeField] protected float aerialAttackSize = 1f;
    [SerializeField] protected Vector2 aerialAttackOffset = new(0f, 0f);
    [SerializeField] protected Vector2 aerialAttackKnockback = new(3f, 0f);



    // ---------- Update & FixedUpdate ----------
    protected override void Update()
    {
        // Play jump SFX exactly when jump is initiated (same as Pete)
        if (!PauseController.IsGamePaused
            && isGrounded
            && !isDashing
            && Input.GetKeyDown(KeyCode.W))
        {
            audioMgr?.PlayJump();
        }

        base.Update();
        UpdateRunLoopSound();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        // (Optional) Move UpdateRunLoopSound() here if you prefer physics-timed checks.
    }

    private void UpdateRunLoopSound()
    {
        if (!audioMgr) return;

        bool shouldRunLoop =
            !isDead &&
            !isHurt &&
            !isReloading &&
            !isAttacking &&
            !isDashing &&
            isGrounded &&
            !isCrouching &&
            Mathf.Abs(rb.linearVelocity.x) > runMinSpeed;

        if (shouldRunLoop) audioMgr.StartRunLoop();
        else audioMgr.StopRunLoop();
    }

    // ---------- Input (R = shoot, T = reload) ----------
    protected override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isDashing && isGrounded)
        {
            interactor.OnInteract();
        }
        if (Input.GetKeyDown(KeyCode.E) && !isWallSliding)
        {
            Attack();
        }
        if (Input.GetKeyDown(KeyCode.F) && isGrounded)
        {
            StartCoroutine(ThrowAttack());
        }

        // R = SHOOT (use base method so ammo is decremented there, like Pete)
        if (Input.GetKeyDown(KeyCode.R) && isGrounded && !isAttacking && ammoCount > 0)
        {
            StartCoroutine(RangedAttack());
        }

        // T = RELOAD (play reload SFX at start)
        if (Input.GetKeyDown(KeyCode.T) && isGrounded && !isAttacking && !isReloading && ammoCount < maxAmmo && PlayerInventory.instance.HasItem("Ammo") > 0)
        {
            audioMgr?.PlayReload();
            reloadCoroutine = StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking && canDash && !isWallSliding)
        {
            isDashing = true;

            // stop run loop and play dash SFX immediately
            audioMgr?.StopRunLoop();
            audioMgr?.PlayDash();

            if (!isCrouching)
            {
                slideCoroutine = StartCoroutine(Dash());
                dashCooldownCoroutine = StartCoroutine(DashCooldown(dashingCooldown));
            }
            else
            {
                StartCoroutine(Slide());
                dashCooldownCoroutine = StartCoroutine(DashCooldown(slidingCooldown));
            }
        }

        if (!isAttacking && isGrounded && Input.GetKeyDown(KeyCode.Z))
        {
            weaponEquipped = !weaponEquipped;
        }
    }

    // ---------- SFX for hurt & death ----------
    public override void HurtPlayer(int damage, float knockbackDirection, Vector2 knockbackForce)
    {
        audioMgr?.StopRunLoop();
        audioMgr?.PlayHurt();
        base.HurtPlayer(damage, knockbackDirection, knockbackForce);
    }

    protected override void Die()
    {
        audioMgr?.StopRunLoop();
        audioMgr?.PlayDeath();
        base.Die();
    }

    // ---------- Melee attacks (hammer SFX mandatory) ----------
    protected override void SetupGroundAttack(int attackIndex)
    {
        // Alice uses circular hitboxes (kept from your code)
        switch (attackIndex)
        {
            case 0:
                hitboxManager.ChangeHitboxCircle(melee1Offset, melee1Size, melee1Knockback, melee1Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.ChangeHitboxCircle(melee2Offset, melee2Size, melee2Knockback, melee2Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                attackTimer = attackCooldown;
                break;
            default:
                break;
        }
        audioMgr?.PlayHammer(); // REQUIRED on every ground melee
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.ChangeHitboxBox(crouchAttackOffset, crouchAttackSize, crouchAttackKnockback, crouchAttackDamage);
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
        audioMgr?.PlaySweep();
        attackTimer = attackCooldown / 3;
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.ChangeHitboxCircle(aerialAttackOffset, aerialAttackSize, aerialAttackKnockback, aerialAttackDamage);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
        audioMgr?.PlayHammer();
        aerialTimer = aerialCooldown;
    }

    // ---------- Grounding behavior (kept from your code) ----------
    protected override void CheckGround()
    {
        bool wasGrounded = isGrounded;
        base.CheckGround();

        if (!wasGrounded && isGrounded && isAttacking && animatorScript.ReturnCurrentState() == playerStates.AerialAttack)
        {
            CancelAerialAttack();
        }
    }

    private void CancelAerialAttack()
    {
        StopCoroutine("AerialAttack");
        hitboxManager.DisableHitbox();
        isAttacking = false;
        AnimationControl();
    }

    // ---------- Ranged (base decrements ammo; adjust origin like your code) ----------
    protected override IEnumerator RangedAttack()
    {
        if (isCrouching)
        {
            bulletOrigin.transform.localPosition = new(bulletOrigin.transform.localPosition.x, -0.08f, 0f);
            if (isCrouching)
            {
                bulletOrigin.transform.localPosition = new Vector3(bulletOrigin.transform.localPosition.x, -0.08f, 0f);
                animatorScript.ChangeAnimationState(playerStates.CrouchRangedAttack);
            }
            else
            {
                bulletOrigin.transform.localPosition = new(bulletOrigin.transform.localPosition.x, 0.2f, 0f);
            }
        }
        else
        {
            bulletOrigin.transform.localPosition = new Vector3(bulletOrigin.transform.localPosition.x, 0.2f, 0f);
            animatorScript.ChangeAnimationState(playerStates.RangedAttack);
        }
        yield return base.RangedAttack();
    }

    // Multi-pellet shotgun: play the shotgun SFX ONCE per shot, then spawn N pellets
    public void InstBullet(int num)
    {
        // Shotgun blast SFX (once)
        audioMgr?.PlayShotgun();

        for (int i = 0; i < num; i++)
        {
            bulletInstance = Instantiate(bullet, bulletOrigin.position, bulletOrigin.rotation);
        }
    }

    // ---------- Throw (match Pete: avoid freeze with timer) ----------
    protected override IEnumerator ThrowAttack()
    {
        isAttacking = true;

        animatorScript.ChangeAnimationState(playerStates.Throw);

        // Stop run loop while throwing (optional, keeps footsteps clean)
        audioMgr?.StopRunLoop();

        // Safety timer in case the animation doesn't call EndAttack()
        yield return new WaitForSeconds(throwDuration);

        EndAttack();
    }

    // safety: stop loop if object disables/destroys
    private void OnDisable() { audioMgr?.StopRunLoop(); }
    private void OnDestroy() { audioMgr?.StopRunLoop(); }
}
