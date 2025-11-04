using System.Collections;
using UnityEngine;

public class PeteMovement2D : BasePlayerMovement2D
{
    // --- AUDIO ---
    private PeteAudioManager audioMgr;

    [Header("Audio – Run Loop")]
    [SerializeField] private float runMinSpeed = 0.2f; // min horiz speed to count as running

    [Header("Throw Settings")]
    [SerializeField] private float throwDuration = 0.5f; // how long throw locks movement if no anim event

    protected override void Awake()
    {
        base.Awake();
        audioMgr = GetComponent<PeteAudioManager>() ?? GetComponentInChildren<PeteAudioManager>();
    }

    protected override float CharWidth => 0.61f;
    protected override Vector2 CrouchOffset => new(0, -0.1238286f);
    protected override Vector2 CrouchSize => new(CharWidth, 0.7002138f);
    protected override Vector2 StandOffset => new(0, 0.1042647f);
    protected override Vector2 StandSize => new(CharWidth, 1.1564f);

    [Header("Pete Specific Combat Settings")]
    [SerializeField] protected int melee1Damage = 2;
    [SerializeField] protected Vector2 melee1Size = new(1.3f, 0.55f);
    [SerializeField] protected Vector2 melee1Offset = new(0.4f, 0f);
    [SerializeField] protected Vector2 melee1Knockback = new(1f, 0f);
    [SerializeField] protected int melee2Damage = 2;
    [SerializeField] protected Vector2 melee2Size = new(1.3f, 0.55f);
    [SerializeField] protected Vector2 melee2Offset = new(0.25f, 0f);
    [SerializeField] protected Vector2 melee2Knockback = new(-2f, 0f);

    [SerializeField] protected int melee3Damage = 3;
    [SerializeField] protected Vector2 melee3Size = new(1.75f, 0.55f);
    [SerializeField] protected Vector2 melee3Offset = new(0.5f, 0f);
    [SerializeField] protected Vector2 melee3Knockback = new(5f, 0f);
    [SerializeField] protected int crouchAttackDamage = 3;
    [SerializeField] protected Vector2 crouchAttackSize = new(1.35f, 0.55f);
    [SerializeField] protected Vector2 crouchAttackOffset = new(0.4f, -0.25f);
    [SerializeField] protected Vector2 crouchAttackKnockback = new(2f, 1f);
    [SerializeField] protected int aerialAttackDamage = 4;
    [SerializeField] protected Vector2 aerialAttackSize = new(1.9f, 1f);
    [SerializeField] protected Vector2 aerialAttackOffset = new(0f, 0f);
    [SerializeField] protected Vector2 aerialAttackKnockback = new(0f, 0f);

    // ---------- Update & FixedUpdate ----------
    protected override void Update()
    {
        // Play jump SFX exactly when jump is initiated
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
        // (Optional) Move UpdateRunLoopSound() here if you want physics-timed checks.
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

        // R = SHOOT (use base method so ammo is decremented there)
        if (Input.GetKeyDown(KeyCode.R) && isGrounded && !isAttacking && ammoCount > 0)
        {
            StartCoroutine(RangedAttack());
        }

        // T = RELOAD
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

            if (!isCrouching)
            {
                slideCoroutine = StartCoroutine(Dash());
                dashCooldownCoroutine = StartCoroutine(DashCooldown(dashingCooldown));
                audioMgr?.PlayDash();
            }
            else
            {
                audioMgr?.PlaySlide();
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
        if (!isInvincible)
        {
            audioMgr?.StopRunLoop();
            audioMgr?.PlayHurt();
        }
        base.HurtPlayer(damage, knockbackDirection, knockbackForce);
    }

    protected override void Die()
    {
        audioMgr?.StopRunLoop();
        audioMgr?.PlayDeath();
        base.Die();
    }

    // ---------- Melee attacks (SFX mandatory on all three) ----------
    protected override void SetupGroundAttack(int attackIndex)
    {
        switch (attackIndex)
        {
            case 0:
                hitboxManager.ChangeHitboxBox(melee1Offset, melee1Size, melee1Knockback, melee1Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.ChangeHitboxBox(melee2Offset, melee2Size, melee2Knockback, melee2Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                break;
            case 2:
                hitboxManager.ChangeHitboxBox(melee3Offset, melee3Size, melee3Knockback, melee3Damage);
                animatorScript.ChangeAnimationState(playerStates.Melee3);
                attackTimer = attackCooldown;
                break;
        }
        audioMgr?.PlayMelee();
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.ChangeHitboxBox(crouchAttackOffset, crouchAttackSize, crouchAttackKnockback, crouchAttackDamage);
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
        audioMgr?.PlayMelee();
        attackTimer = attackCooldown / 2;
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.ChangeHitboxBox(aerialAttackOffset, aerialAttackSize, aerialAttackKnockback, aerialAttackDamage);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
        audioMgr?.PlayMelee();
        aerialTimer = aerialCooldown;
    }

    protected override void HandleAerialAttackMovement()
    {
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, 0.3f), 0);
    }

    // ---------- Ranged (base decrements ammo) ----------
    protected override IEnumerator AerialAttack()
    {
        aerialTimer = aerialCooldown;
        SetupAerialAttack();
        isAttacking = true;
        float oldGravity = rb.linearVelocity.y > 0 ? -1f : rb.linearVelocity.y;
        yield return new WaitWhile(() => isAttacking);
        rb.linearVelocity = new(rb.linearVelocity.x, oldGravity);
    }

    protected override IEnumerator RangedAttack()
    {
        if (isCrouching)
        {
            bulletOrigin.transform.localPosition = new(bulletOrigin.transform.localPosition.x, -0.12f, 0f);
            animatorScript.ChangeAnimationState(playerStates.CrouchRangedAttack);
        }
        else
        {
            bulletOrigin.transform.localPosition = new(bulletOrigin.transform.localPosition.x, 0.2f, 0f);
            animatorScript.ChangeAnimationState(playerStates.RangedAttack);
        }
        yield return base.RangedAttack();
    }

    public void InstBullet()
    {
        // ammo already decremented by base.RangedAttack()
        bulletInstance = Instantiate(bullet, bulletOrigin.position, bulletOrigin.rotation);
        audioMgr?.PlayRevolver(); // gunshot timed to muzzle flash / bullet spawn
    }

    // ---------- Throw (fix: end by time so it never freezes) ----------
    protected override IEnumerator ThrowAttack()
    {
        isAttacking = true;

        // Play Throw animation
        animatorScript.ChangeAnimationState(playerStates.Throw);

        // Stop run loop while throwing (optional)
        audioMgr?.StopRunLoop();

        // If you also spawn a thrown object, call it via an Animation Event (e.g., InstThrow())
        // Then end after a fixed duration (if you also add an EndAttack() event, this is just a backup)
        yield return new WaitForSeconds(throwDuration);

        EndAttack();
    }

    // safety: stop loop if object disables/destroys
    private void OnDisable() { audioMgr?.StopRunLoop(); }
    private void OnDestroy() { audioMgr?.StopRunLoop(); }
}
