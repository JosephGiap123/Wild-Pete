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
    protected override Vector2 CrouchOffset => new Vector2(0, -0.1238286f);
    protected override Vector2 CrouchSize => new Vector2(CharWidth, 0.7002138f);
    protected override Vector2 StandOffset => new Vector2(0, 0.1042647f);
    protected override Vector2 StandSize => new Vector2(CharWidth, 1.1564f);

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
        else               audioMgr.StopRunLoop();
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
        if (Input.GetKeyDown(KeyCode.T) && isGrounded && !isAttacking && !isReloading && ammoCount < maxAmmo)
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
    protected override void HurtPlayer(int damage, float knockbackDirection)
    {
        audioMgr?.StopRunLoop();
        audioMgr?.PlayHurt();
        base.HurtPlayer(damage, knockbackDirection);
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
                hitboxManager.ChangeHitboxCircle(new Vector2(0.5f, 0f), 0.8f);
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.ChangeHitboxCircle(new Vector2(0.5f, 0f), 0.8f);
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                break;
            default:
                // If you later add a 3rd, keep the last state as fallback
                hitboxManager.ChangeHitboxCircle(new Vector2(0.5f, 0f), 0.8f);
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                break;
        }
        audioMgr?.PlayHammer(); // REQUIRED on every ground melee
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.ChangeHitboxBox(new Vector2(0.3f, -0.25f), new Vector2(1f, 0.55f));
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
        audioMgr?.PlaySweep(); 
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.ChangeHitboxCircle(new Vector2(0f, 0f), 1f);
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
        audioMgr?.PlayHammer(); 
    }

    // ---------- Grounding behavior (kept from your code) ----------
    protected override void CheckGround()
    {
        bool wasGrounded = isGrounded;
        base.CheckGround();

        if (!wasGrounded && isGrounded && isAttacking && animatorScript.returnCurrentState() == playerStates.AerialAttack)
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
            bulletOrigin.transform.localPosition = new Vector3(bulletOrigin.transform.localPosition.x, -0.08f, 0f);
            animatorScript.ChangeAnimationState(playerStates.CrouchRangedAttack);
        }
        else
        {
            bulletOrigin.transform.localPosition = new Vector3(bulletOrigin.transform.localPosition.x, 0.2f, 0f);
            animatorScript.ChangeAnimationState(playerStates.RangedAttack);
        }
        return base.RangedAttack();
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
