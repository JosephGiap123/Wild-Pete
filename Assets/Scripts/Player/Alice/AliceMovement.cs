using System.Collections;
using UnityEngine;

public class AliceMovement2D : BasePlayerMovement2D
{
    // --- AUDIO ---
    private AliceAudioManager audioMgr;

    [Header("Audio – Run Loop")]
    [SerializeField] private float runMinSpeed = 0.2f; // min horiz speed to count as running

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

    public override void FlipSprite()
    {
        base.FlipSprite();
    }

    protected override void HandleInput()
    {
        // Safety check: ensure ControlManager is initialized
        if (ControlManager.instance == null || ControlManager.instance.inputMapping == null)
            return;

        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Interact]) && !isDashing && isGrounded)
        {
            CallInputInvoke(PlayerControls.Interact, ControlManager.instance.inputMapping[PlayerControls.Interact]);
            audioMgr?.StopRunLoop();
            interactor.OnInteract();
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Melee]) && !isWallSliding)
        {
            Attack();
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Throw]) && isGrounded && PlayerInventory.instance.HasItem("Dynamite") > 0)
        {

            attackCoroutine = StartCoroutine(ThrowAttack());
            PlayerInventory.instance.UseItem("Dynamite", 1);
        }

        // R = SHOOT (use base method so ammo is decremented there, like Pete)
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Ranged]) && isGrounded && !isAttacking && ammoCount > 0)
        {
            StartCoroutine(RangedAttack());
        }

        // T = RELOAD (play reload SFX at start)
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Reload]) && isGrounded && !isAttacking && !isReloading && ammoCount < maxAmmo && PlayerInventory.instance.HasItem("Ammo") > 0)
        {
            reloadCoroutine = StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Dash]) && !isAttacking && canDash && !isWallSliding)
        {
            isDashing = true;

            // stop run loop and play dash SFX immediately
            audioMgr?.StopRunLoop();


            if (!isCrouching)
            {
                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                emitParams.rotation3D = new(0f, isFacingRight ? 0f : 180f);
                dashParticle.Emit(emitParams, 1);
                slideCoroutine = StartCoroutine(Dash());
                dashCooldownCoroutine = StartCoroutine(DashCooldown(dashingCooldown));
                audioMgr?.PlayDash();
            }
            else
            {
                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                emitParams.rotation3D = new(0f, isFacingRight ? 0f : 180f);
                slideParticle.Emit(emitParams, 1);
                audioMgr?.PlaySlide();
                StartCoroutine(Slide());
                dashCooldownCoroutine = StartCoroutine(DashCooldown(slidingCooldown));
            }
        }

        if (!isAttacking && isGrounded && Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Unequip]))
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
                attackTimer = attackCooldown;
                break;
            default:
                break;
        }
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.CustomizeHitbox(attackHitboxes[4]);
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
        audioMgr?.PlaySweep();
        attackTimer = attackCooldown / 3;
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.CustomizeHitbox(attackHitboxes[5]);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
        aerialTimer = aerialCooldown;
    }

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

        for (int i = 0; i < num; i++)
        {
            bulletInstance = Instantiate(bullet, bulletOrigin.position, bulletOrigin.rotation);
        }
    }
    protected override IEnumerator ThrowAttack()
    {
        // audioMgr.PlayThrow();
        // audioMgr?.StopRunLoop();
        yield return base.ThrowAttack();
    }

    private void OnDisable() { audioMgr?.StopRunLoop(); }
    private void OnDestroy() { audioMgr?.StopRunLoop(); }
}
