using System.Collections;
using UnityEngine;

public class PeteMovement2D : BasePlayerMovement2D
{
    // --- AUDIO ---
    private PeteAudioManager audioMgr;

    [Header("Audio – Run Loop")]
    [SerializeField] private float runMinSpeed = 0.2f; // min horiz speed to count as running


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
        if (LockpickFiveInARow.IsLockpickActive)
        {
            audioMgr.StopRunLoop();
            return;
        }

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
    protected override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isDashing && isGrounded)
        {
            audioMgr?.StopRunLoop();
            interactor.OnInteract();
        }
        if (Input.GetKeyDown(KeyCode.E) && !isWallSliding)
        {
            Attack();
        }
        if (Input.GetKeyDown(KeyCode.F) && isGrounded && PlayerInventory.instance.HasItem("Dynamite") > 0)
        {
            attackCoroutine = StartCoroutine(ThrowAttack());
            PlayerInventory.instance.UseItem("Dynamite", 1);
        }

        // R = SHOOT (use base method so ammo is decremented there)
        if (Input.GetKeyDown(KeyCode.R) && isGrounded && !isAttacking && ammoCount > 0)
        {
            StartCoroutine(RangedAttack());
        }

        // T = RELOAD
        if (Input.GetKeyDown(KeyCode.T) && isGrounded && !isAttacking && !isReloading && ammoCount < maxAmmo && PlayerInventory.instance.HasItem("Ammo") > 0)
        {
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

    public override void HurtPlayer(int damage, Vector2 knockbackForce, float? knockbackDirection = null, Vector2? hitboxCenter = null)
    {
        if (!isInvincible)
        {
            audioMgr?.StopRunLoop();
            audioMgr?.PlayHurt();
        }
        base.HurtPlayer(damage, knockbackForce, knockbackDirection, hitboxCenter);
    }

    protected override void Die()
    {
        audioMgr?.StopRunLoop();
        audioMgr?.PlayDeath();
        base.Die();
    }

    protected override void SetupGroundAttack(int attackIndex)
    {
        switch (attackIndex)
        {
            case 0:
                hitboxManager.CustomizeHitbox(attackHitboxes[2]);
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.CustomizeHitbox(attackHitboxes[3]);
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                break;
            case 2:
                hitboxManager.CustomizeHitbox(attackHitboxes[4]);
                animatorScript.ChangeAnimationState(playerStates.Melee3);
                attackTimer = attackCooldown;
                break;
        }
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.CustomizeHitbox(attackHitboxes[5]);
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
        attackTimer = attackCooldown / 2;
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.CustomizeHitbox(attackHitboxes[6]);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
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
        bulletInstance = Instantiate(bullet, bulletOrigin.position, bulletOrigin.rotation);
    }
    protected override IEnumerator ThrowAttack()
    {
        // Play Throw animation + stop loop SFX while dynamite throw plays
        audioMgr?.PlayThrow();
        audioMgr?.StopRunLoop();

        yield return base.ThrowAttack();
    }

    // safety: stop loop if object disables/destroys
    private void OnDisable() { audioMgr?.StopRunLoop(); }
    private void OnDestroy() { audioMgr?.StopRunLoop(); }
}
