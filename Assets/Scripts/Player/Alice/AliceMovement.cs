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
        // Play jump SFX exactly when jump is initiated (works with multi-jump)
        if (!PauseController.IsGamePaused
            && !isDashing
            && jumpsRemaining > 0
            && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)))
        {
            // Only play sound if we have jumps remaining
            audioMgr?.PlayJump();
        }

        base.Update();
        UpdateRunLoopSound();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void UpdateRunLoopSound()
    {
        if (!audioMgr) return;
        if (LockpickFiveInARow.IsLockpickActive)
        {
            audioMgr.StopRunLoop();
            return;
        }

        // Only stop the loop when conditions aren't met (safety net)
        // Animation events handle starting the loop for sync
        bool shouldStopLoop =
            isDead ||
            isHurt ||
            isReloading ||
            isAttacking ||
            isDashing ||
            !isGrounded ||
            isCrouching ||
            Mathf.Abs(rb.linearVelocity.x) <= runMinSpeed;

        if (shouldStopLoop)
        {
            audioMgr.StopRunLoop();
        }
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
            CallInputInvoke("Interact", PlayerControls.Interact, ControlManager.instance.inputMapping[PlayerControls.Interact]);
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
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Hotkey1]) && isGrounded && PlayerInventory.instance.HasItem("Bandaid") > 0)
        {
            PlayerInventory.instance.UseItem("Bandaid", 1);
        }
        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Hotkey2]) && isGrounded && PlayerInventory.instance.HasItem("Medkit") > 0)
        {
            PlayerInventory.instance.UseItem("Medkit", 1);
        }

        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Ranged]) && isGrounded && !isAttacking && ammoCount > 0 && PlayerInventory.instance.equipmentSlots[3].GetEquippedItem() != null)
        {
            StartCoroutine(RangedAttack());
        }


        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Reload]) && isGrounded && !isAttacking && !isReloading && ammoCount < maxAmmo && PlayerInventory.instance.HasItem("Ammo") > 0 && PlayerInventory.instance.equipmentSlots[3].GetEquippedItem() != null)
        {
            reloadCoroutine = StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Dash]) && !isAttacking && !isWallSliding)
        {
            if (!isCrouching && EnergyManager.instance.UseEnergy(dashingEnergyCost))
            {
                isDashing = true;

                // stop run loop and play dash SFX immediately
                audioMgr?.StopRunLoop();

                CallInputInvoke("Dash", PlayerControls.Dash, ControlManager.instance.inputMapping[PlayerControls.Dash]);
                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                emitParams.rotation3D = new(0f, isFacingRight ? 0f : 180f);
                dashParticle.Emit(emitParams, 1);
                slideCoroutine = StartCoroutine(Dash());
                // dashCooldownCoroutine = StartCoroutine(DashCooldown(dashingCooldown));
                audioMgr?.PlayDash();
            }
            else if (isCrouching && EnergyManager.instance.UseEnergy(slidingEnergyCost))
            {
                isDashing = true;

                // stop run loop and play dash SFX immediately
                audioMgr?.StopRunLoop();

                CallInputInvoke("Slide", PlayerControls.Dash, ControlManager.instance.inputMapping[PlayerControls.Dash]);
                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                emitParams.rotation3D = new(0f, isFacingRight ? 0f : 180f);
                slideParticle.Emit(emitParams, 1);
                audioMgr?.PlaySlide();
                StartCoroutine(Slide());
                // dashCooldownCoroutine = StartCoroutine(DashCooldown(slidingCooldown));
            }
            else
            {
                Debug.Log("Not enough energy to dash or slide");
            }
        }

        if (!isAttacking && isGrounded && Input.GetKeyDown(ControlManager.instance.inputMapping[PlayerControls.Unequip]) && PlayerInventory.instance.equipmentSlots[2] != null && !PlayerInventory.instance.equipmentSlots[2].IsEmpty())
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
        CallInputInvoke("Melee", PlayerControls.Melee, ControlManager.instance.inputMapping[PlayerControls.Melee]);
        switch (attackIndex)
        {
            case 0:
                hitboxManager.CustomizeHitbox(attackHitboxes[2]);
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.CustomizeHitbox(attackHitboxes[3]);
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                // attackTimer = attackCooldown;
                break;
            default:
                break;
        }
    }

    protected override void SetupCrouchAttack()
    {
        CallInputInvoke("CrouchMelee", PlayerControls.Melee, ControlManager.instance.inputMapping[PlayerControls.Melee]);
        hitboxManager.CustomizeHitbox(attackHitboxes[4]);
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
        audioMgr?.PlaySweep();
        // attackTimer = attackCooldown / 3;
    }

    protected override void SetupAerialAttack()
    {
        CallInputInvoke("AerialMelee", PlayerControls.Melee, ControlManager.instance.inputMapping[PlayerControls.Melee]);
        hitboxManager.CustomizeHitbox(attackHitboxes[5]);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
        // aerialTimer = aerialCooldown;
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
            CallInputInvoke("CrouchRangedAttack", PlayerControls.Ranged, ControlManager.instance.inputMapping[PlayerControls.Ranged]);
            bulletOrigin.transform.localPosition = new(bulletOrigin.transform.localPosition.x, -0.08f, 0f);
            animatorScript.ChangeAnimationState(playerStates.CrouchRangedAttack);
        }
        else
        {
            CallInputInvoke("RangedAttack", PlayerControls.Ranged, ControlManager.instance.inputMapping[PlayerControls.Ranged]);
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
        CallInputInvoke("Throw", PlayerControls.Throw, ControlManager.instance.inputMapping[PlayerControls.Throw]);
        audioMgr.PlayThrow();
        audioMgr?.StopRunLoop();
        yield return base.ThrowAttack();
    }

    protected override void CancelAllActions()
    {
        // Cancel reload audio if reloading was interrupted
        if (isReloading)
        {
            audioMgr?.CancelReload();
        }
        base.CancelAllActions();
    }

    private void OnDisable() { audioMgr?.StopRunLoop(); }
    private void OnDestroy() { audioMgr?.StopRunLoop(); }
}
