using UnityEngine;

/// Base class for enemies that use patrol and detection systems.
/// Provides shared functionality for patrol behavior, multiple raycast detection, and basic state machine.
public abstract class PatrolEnemyAI : EnemyBase
{
	protected enum PatrolState
	{
		Idle,
		Patrol,
		Alert
	}

	[SerializeField] protected PatrolState currentState = PatrolState.Idle;

	[Header("Patrol Settings")]
	[SerializeField] protected Vector2[] patrolPoints;
	[SerializeField] protected bool useRelativePatrolPoints = true; // If true, patrol points are relative to starting position
	protected Vector2 startingPosition; // Store starting position for relative patrol points
	protected int currentPatrolIndex = 0;
	[SerializeField] protected float patrolWaitTime = 2f;
	protected float patrolWaitTimer = 0f;

	[Header("Detection Settings")]
	[SerializeField] protected float detectionRange = 10f;
	[SerializeField] protected float raycastHeightOffset = 0.8f; // Height offset for raycast origin (vertical Y offset)
	[SerializeField] protected int raycastCount = 3; // Number of raycasts to fire (more = more forgiving, but more expensive)
	[SerializeField] protected float raycastVerticalSpread = 0.2f; // Vertical spacing between multiple raycasts
	[SerializeField] protected float nearDetectRadius = 1f; // detect player if extremely close, regardless of facing/LOS
	[SerializeField] protected float loseSightTime = 3f;
	[SerializeField] protected bool checkFacingDirection = true; // Whether to check if player is in facing direction
	[SerializeField] protected LayerMask obstructionLayer; // Layer mask for walls/obstacles that block line of sight
	protected float loseSightTimer = 0f;

	[Header("Movement Settings")]
	[SerializeField] protected float moveSpeed = 3.5f;
	[SerializeField] protected float alertSpeed = 4f; // Faster movement when chasing (can override in child classes)

	protected Transform player;

	[Header("Jump Settings")]
	[SerializeField] protected bool canJump = true; // Whether this enemy can jump over obstacles
	[SerializeField] protected float jumpPower = 6f;
	[SerializeField] protected float jumpCooldown = 1f;
	[SerializeField] protected float obstacleDetectionDistance = 0.5f; // How far ahead to check for obstacles
	[SerializeField] protected float groundCheckDistance = 0.3f; // How far down to check for ground ahead
	[SerializeField] protected LayerMask groundLayer; // Layer mask for ground/walls
	[SerializeField] protected BoxCollider2D groundCheckBox; // Optional: BoxCollider2D for ground checking
	protected float jumpTimer = 0f;
	protected bool isInAir = false;

	protected virtual void Start()
	{
		// Store starting position for relative patrol points
		startingPosition = transform.position;

		// Try to find player immediately if it exists
		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		if (playerObj != null)
		{
			player = playerObj.transform;
		}

		// Try to find ground check box if not assigned
		if (groundCheckBox == null)
		{
			groundCheckBox = GetComponentInChildren<BoxCollider2D>();
		}
	}

	/// <summary>
	/// Update method. Child classes should call base.Update() to get jump timer and ground checking.
	/// </summary>
	protected virtual void Update()
	{
		// Decrement jump timer
		if (jumpTimer > 0f)
		{
			jumpTimer -= Time.deltaTime;
		}

		// Check if grounded
		IsGroundedCheck();
	}

	/// <summary>
	/// Checks if the enemy is grounded. Override in child classes if they have custom ground checking.
	/// </summary>
	protected virtual void IsGroundedCheck()
	{
		if (groundCheckBox != null)
		{
			Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheckBox.bounds.center, groundCheckBox.bounds.size, 0f, groundLayer);
			isInAir = colliders.Length == 0;
		}
		else
		{
			// Fallback: use a simple raycast down
			RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, groundLayer);
			isInAir = hit.collider == null;
		}
	}

	/// <summary>
	/// Checks if there's an obstacle (wall or floor) ahead that should be jumped over.
	/// </summary>
	protected virtual bool HasObstacleAhead()
	{
		if (!canJump || isInAir || jumpTimer > 0f)
		{
			return false;
		}

		// Direction we're facing
		Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;

		// Check for wall ahead at multiple heights to catch walls of different sizes
		bool wallDetected = false;
		for (float height = 0.2f; height <= 1.0f; height += 0.3f)
		{
			Vector2 wallCheckOrigin = new Vector2(transform.position.x, transform.position.y + height);
			RaycastHit2D wallHit = Physics2D.Raycast(wallCheckOrigin, direction, obstacleDetectionDistance, groundLayer);

			if (wallHit.collider != null)
			{
				wallDetected = true;
				// There's a wall - check if there's ground above it to jump onto
				Vector2 groundCheckOrigin = wallHit.point + direction * 0.2f + Vector2.up * 0.2f;
				RaycastHit2D groundHit = Physics2D.Raycast(groundCheckOrigin, Vector2.down, groundCheckDistance + 1f, groundLayer);

				// If there's ground above the wall, we can jump over it
				if (groundHit.collider != null)
				{
					return true;
				}
			}
		}

		// If we detected a wall but no ground above it, don't jump (it's too high)
		if (wallDetected)
		{
			return false;
		}

		// Check for gap ahead (no ground to walk on)
		Vector2 gapCheckOrigin = new Vector2(transform.position.x, transform.position.y);
		RaycastHit2D gapHit = Physics2D.Raycast(gapCheckOrigin + direction * obstacleDetectionDistance, Vector2.down, groundCheckDistance, groundLayer);

		if (gapHit.collider == null)
		{
			// No ground ahead - check if there's ground further ahead to jump to
			RaycastHit2D farGroundHit = Physics2D.Raycast(gapCheckOrigin + direction * (obstacleDetectionDistance + 0.5f), Vector2.down, groundCheckDistance + 1f, groundLayer);
			if (farGroundHit.collider != null)
			{
				// There's ground further ahead - we should jump
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Makes the enemy jump. Override in child classes for custom jump behavior.
	/// </summary>
	protected virtual void Jump()
	{
		if (isInAir || jumpTimer > 0f || !canJump)
		{
			return;
		}

		rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
		jumpTimer = jumpCooldown;
	}

	protected virtual void OnEnable()
	{
		GameManager.OnPlayerSet += HandlePlayerSet;
	}

	protected virtual void OnDisable()
	{
		GameManager.OnPlayerSet -= HandlePlayerSet;
	}

	protected virtual void HandlePlayerSet(GameObject playerObj)
	{
		if (playerObj != null)
		{
			player = playerObj.transform;
		}
	}

	/// <summary>
	/// Checks if the enemy can see the player using multiple raycasts.
	/// </summary>
	protected virtual bool CanSeePlayer()
	{
		if (player == null)
		{
			// Try to find player again
			GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
			if (playerObj != null)
			{
				player = playerObj.transform;
			}
			else
			{
				return false;
			}
		}

		Vector2 directionToPlayer = player.position - transform.position;
		float distanceToPlayer = directionToPlayer.magnitude;

		// Immediate proximity detection (anti-hugging): if very close, detect regardless of facing/LOS
		if (distanceToPlayer <= nearDetectRadius)
			return true;

		// Check if player is in detection range
		if (distanceToPlayer > detectionRange)
			return false;

		// Optional: Check facing direction (override in child if needed)
		if (ShouldCheckFacingDirection() && !IsPlayerInFacingDirection(directionToPlayer))
			return false;

		// Fire multiple raycasts at different heights for more robust detection
		// If ANY raycast succeeds (no obstruction), we can see the player
		bool canSee = false;
		float startOffset = raycastHeightOffset - (raycastCount - 1) * raycastVerticalSpread * 0.5f;

		for (int i = 0; i < raycastCount; i++)
		{
			// Calculate raycast origin with height offset (spread vertically)
			float currentHeightOffset = startOffset + (i * raycastVerticalSpread);
			Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y + currentHeightOffset);

			// Raycast ONLY for obstructions - if we hit something that's NOT the player, line of sight is blocked
			RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, directionToPlayer.normalized, distanceToPlayer, obstructionLayer);

			// Draw debug ray to visualize detection
			Debug.DrawRay(raycastOrigin, directionToPlayer.normalized * distanceToPlayer, hit.collider != null ? Color.red : Color.green);

			// If this raycast didn't hit anything, we have clear line of sight
			if (hit.collider == null)
			{
				canSee = true;
			}
		}

		return canSee;
	}

	/// <summary>
	/// Override this to enable/disable facing direction checks.
	/// </summary>
	protected virtual bool ShouldCheckFacingDirection()
	{
		return checkFacingDirection; // Use serialized field value
	}

	/// <summary>
	/// Checks if player is in the direction the enemy is facing.
	/// </summary>
	protected virtual bool IsPlayerInFacingDirection(Vector2 directionToPlayer)
	{
		return !((isFacingRight && directionToPlayer.x < 0) || (!isFacingRight && directionToPlayer.x > 0));
	}

	/// <summary>
	/// Moves towards a target position.
	/// </summary>
	protected virtual void MoveTowards(Vector2 target, float speed = -1f)
	{
		// Use default moveSpeed if no speed specified
		if (speed < 0) speed = moveSpeed;

		float deltaX = target.x - transform.position.x;

		// Higher tolerance for movement to prevent micro-adjustments
		if (Mathf.Abs(deltaX) > 0.5f)
		{
			float direction = Mathf.Sign(deltaX);

			// Check if we're hitting a wall while in the air - stop horizontal movement
			if (isInAir)
			{
				Vector2 wallDirection = isFacingRight ? Vector2.right : Vector2.left;
				RaycastHit2D wallHit = Physics2D.Raycast(transform.position, wallDirection, 0.3f, groundLayer);
				if (wallHit.collider != null)
				{
					// Hitting a wall while jumping - stop horizontal movement
					rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
					return;
				}
			}

			// Check for obstacles ahead before moving (only if grounded)
			if (!isInAir && HasObstacleAhead())
			{
				Jump();
				// After jumping, still apply horizontal movement for the jump
				rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
			}
			else if (!isInAir)
			{
				// Normal movement when grounded and no obstacles
				rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
			}
			else
			{
				// In air but not hitting wall - allow some horizontal movement
				rb.linearVelocity = new Vector2(direction * speed * 0.5f, rb.linearVelocity.y);
			}

			// Face the direction we're moving
			if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
			{
				FlipSprite();
			}
		}
		else
		{
			StopMoving();
		}
	}

	/// <summary>
	/// Stops horizontal movement.
	/// </summary>
	public virtual void StopMoving()
	{
		rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
	}

	/// <summary>
	/// Handles the Idle state. Override to customize behavior.
	/// </summary>
	protected virtual void HandleIdle(bool canSee)
	{
		StopMoving();

		if (canSee)
		{
			currentState = PatrolState.Alert;
			loseSightTimer = 0f;
			return;
		}

		patrolWaitTimer += Time.deltaTime;

		if (patrolWaitTimer >= patrolWaitTime)
		{
			patrolWaitTimer = 0f;
			if (patrolPoints != null && patrolPoints.Length > 0)
			{
				currentState = PatrolState.Patrol;
			}
		}
	}

	/// <summary>
	/// Handles the Patrol state. Override to customize behavior.
	/// </summary>
	protected virtual void HandlePatrol(bool canSee)
	{
		if (patrolPoints == null || patrolPoints.Length == 0)
		{
			currentState = PatrolState.Idle;
			return;
		}

		if (canSee)
		{
			currentState = PatrolState.Alert;
			loseSightTimer = 0f;
			return;
		}

		// Get current patrol target (convert to world space if using relative points)
		Vector2 patrolTarget = patrolPoints[currentPatrolIndex];

		// If using relative patrol points, convert to world space
		if (useRelativePatrolPoints)
		{
			patrolTarget = startingPosition + patrolTarget;
		}

		float distanceToTarget = Vector2.Distance(transform.position, patrolTarget);

		MoveTowards(patrolTarget);

		// Check if reached patrol point (using slightly larger tolerance for more forgiving detection)
		if (distanceToTarget < 1.0f)
		{
			StopMoving();
			patrolWaitTimer += Time.deltaTime;

			if (patrolWaitTimer >= patrolWaitTime)
			{
				patrolWaitTimer = 0f;
				currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
			}
		}
	}

	/// <summary>
	/// Handles the Alert/Chase state. Override to customize behavior.
	/// </summary>
	protected virtual void HandleAlert(bool canSee)
	{
		if (!canSee)
		{
			loseSightTimer += Time.deltaTime;

			// While searching, try to turn around to find player
			if (player != null && loseSightTimer < loseSightTime)
			{
				// Check if player is behind us
				Vector2 directionToPlayer = player.position - transform.position;
				bool playerIsBehind = (isFacingRight && directionToPlayer.x < 0) || (!isFacingRight && directionToPlayer.x > 0);

				if (playerIsBehind)
				{
					// Turn around to look for player
					FlipSprite();
				}
			}

			if (loseSightTimer >= loseSightTime)
			{
				loseSightTimer = 0f;
				StopMoving();
				currentState = PatrolState.Idle;
				return;
			}
		}
		else
		{
			loseSightTimer = 0f;
		}

		if (player == null) return;

		// Chase the player
		MoveTowards(player.position, alertSpeed);
	}

	/// <summary>
	/// Draws gizmos for debugging. Override to add custom gizmos.
	/// </summary>
	protected virtual void OnDrawGizmosSelected()
	{
		// Draw detection range
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		// Draw patrol points
		Gizmos.color = Color.yellow;
		if (patrolPoints != null && patrolPoints.Length > 0)
		{
			Vector2 startPos = useRelativePatrolPoints ? (Application.isPlaying ? startingPosition : transform.position) : Vector2.zero;
			foreach (var p in patrolPoints)
			{
				Vector2 worldPos = useRelativePatrolPoints ? startPos + p : p;
				Gizmos.DrawWireSphere(worldPos, 0.2f);

				// Draw line from enemy to patrol point
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(transform.position, worldPos);
				Gizmos.color = Color.yellow;
			}

			// Draw line connecting patrol points in order
			if (patrolPoints.Length > 1)
			{
				Gizmos.color = Color.green;
				for (int i = 0; i < patrolPoints.Length; i++)
				{
					Vector2 current = useRelativePatrolPoints ? startPos + patrolPoints[i] : patrolPoints[i];
					Vector2 next = useRelativePatrolPoints ? startPos + patrolPoints[(i + 1) % patrolPoints.Length] : patrolPoints[(i + 1) % patrolPoints.Length];
					Gizmos.DrawLine(current, next);
				}
			}
		}

		// Draw raycast gizmos if player exists
		Transform playerToDraw = player;
		if (playerToDraw == null)
		{
			GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
			if (playerObj != null)
			{
				playerToDraw = playerObj.transform;
			}
		}

		if (playerToDraw != null)
		{
			float distanceToPlayer = Vector2.Distance(transform.position, playerToDraw.position);
			Vector2 directionToPlayer = playerToDraw.position - transform.position;

			// Draw all raycast paths
			if (distanceToPlayer <= detectionRange)
			{
				float startOffset = raycastHeightOffset - (raycastCount - 1) * raycastVerticalSpread * 0.5f;

				for (int i = 0; i < raycastCount; i++)
				{
					float currentHeightOffset = startOffset + (i * raycastVerticalSpread);
					Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y + currentHeightOffset);

					// Perform the actual raycast to see if it hits
					RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, directionToPlayer.normalized, distanceToPlayer, obstructionLayer);

					// Draw raycast path
					if (hit.collider != null)
					{
						// Hit something - draw in red to hit point
						Gizmos.color = Color.red;
						Gizmos.DrawLine(raycastOrigin, hit.point);
						// Draw a small sphere at hit point
						Gizmos.DrawWireSphere(hit.point, 0.1f);
					}
					else
					{
						// Clear path - draw in green to player
						Gizmos.color = Color.green;
						Gizmos.DrawLine(raycastOrigin, (Vector2)playerToDraw.position);
					}

					// Draw raycast origin point
					Gizmos.color = Color.cyan;
					Gizmos.DrawWireSphere(raycastOrigin, 0.05f);
				}
			}

			// Draw line to player
			if (distanceToPlayer <= detectionRange)
			{
				Gizmos.color = Color.yellow;
			}
			else
			{
				Gizmos.color = Color.gray;
			}
			Gizmos.DrawLine(transform.position, playerToDraw.position);
		}
	}

	/// Resets patrol and detection state. Call base.Respawn() in overrides.
	public override void Respawn(Vector2? position = null, bool? facingRight = null)
	{
		base.Respawn(position, facingRight);

		// Update starting position if respawned to a new location
		if (position.HasValue)
		{
			startingPosition = position.Value;
		}
		else
		{
			startingPosition = transform.position;
		}

		// Reset patrol state
		currentState = PatrolState.Idle;
		loseSightTimer = 0f;
		patrolWaitTimer = 0f;
		currentPatrolIndex = 0;
	}

	/// Override Hurt to automatically enter Alert state when hit
	public override void Hurt(int dmg, Vector2 knockbackForce)
	{
		// Call base Hurt first (handles health, damage text, etc.)
		base.Hurt(dmg, knockbackForce);

		// Enter Alert state and try to find attacker
		if (IsAlive())
		{
			currentState = PatrolState.Alert;
			loseSightTimer = 0f;

			// Try to find player if we don't have reference
			if (player == null)
			{
				GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
				if (playerObj != null)
				{
					player = playerObj.transform;
				}
			}
		}
	}
}

