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
			rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

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
}

