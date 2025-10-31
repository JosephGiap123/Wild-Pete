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
    protected override Vector2 CrouchOffset => new Vector2(0, -0.1238286f);
    protected override Vector2 CrouchSize => new Vector2(CharWidth, 0.7002138f);
    protected override Vector2 StandOffset => new Vector2(0, 0.1042647f);
    protected override Vector2 StandSize => new Vector2(CharWidth, 1.1564f);

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

        // R = SHOOT (use base method so ammo is decremented there)
        if (Input.GetKeyDown(KeyCode.R) && isGrounded && !isAttacking && ammoCount > 0)
        {
            StartCoroutine(RangedAttack());
        }

        // T = RELOAD
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

    // ---------- Melee attacks (SFX mandatory on all three) ----------
    protected override void SetupGroundAttack(int attackIndex)
    {
        switch (attackIndex)
        {
            case 0:
                hitboxManager.ChangeHitboxBox(new Vector2(0.4f, 0f), new Vector2(1.3f, 0.55f));
                animatorScript.ChangeAnimationState(playerStates.Melee1);
                break;
            case 1:
                hitboxManager.ChangeHitboxBox(new Vector2(0.25f, 0f), new Vector2(1.3f, 0.55f));
                animatorScript.ChangeAnimationState(playerStates.Melee2);
                break;
            case 2:
                hitboxManager.ChangeHitboxBox(new Vector2(0.5f, 0f), new Vector2(1.75f, 0.55f));
                animatorScript.ChangeAnimationState(playerStates.Melee3);
                break;
        }
        audioMgr?.PlayMelee();
    }

    protected override void SetupCrouchAttack()
    {
        hitboxManager.ChangeHitboxBox(new Vector2(0.4f, -0.25f), new Vector2(1.35f, 0.55f));
        animatorScript.ChangeAnimationState(playerStates.CrouchAttack);
        audioMgr?.PlayMelee();
    }

    protected override void SetupAerialAttack()
    {
        hitboxManager.ChangeHitboxBox(new Vector2(0f, 0f), new Vector2(1.9f, 1f));
        animatorScript.ChangeAnimationState(playerStates.AerialAttack);
        audioMgr?.PlayMelee();
    }

    // ---------- Ranged (base decrements ammo) ----------
    protected override IEnumerator RangedAttack()
    {
        if (isCrouching)
        {
            bulletOrigin.transform.localPosition = new Vector3(bulletOrigin.transform.localPosition.x, -0.12f, 0f);
            animatorScript.ChangeAnimationState(playerStates.CrouchRangedAttack);
        }
        else
        {
            bulletOrigin.transform.localPosition = new Vector3(bulletOrigin.transform.localPosition.x, 0.2f, 0f);
            animatorScript.ChangeAnimationState(playerStates.RangedAttack);
        }
        return base.RangedAttack();
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
