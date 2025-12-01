using UnityEngine;
using System.Collections;

public class MovingElevator : MovingPlatform
{
    //moves very slowly, and stops at each waypoint for a short time. Will go back to spawn point if player is not on it.

    [Header("Elevator Settings")]
    [Tooltip("Time to wait at each waypoint before moving to the next one")]
    [SerializeField] private float waypointWaitTime = 2f;

    [Tooltip("Time to wait when player is not on the elevator before returning to start")]
    [SerializeField] private float returnWaitTime = 3f;

    private bool isPlayerOnElevator = false;
    private bool isWaitingAtWaypoint = false;
    private bool isReturningToStart = false;
    private Vector2 startingPosition;
    private int currentWaypointIndex;
    private Coroutine waitCoroutine;
    private Coroutine returnCoroutine;

    void Start()
    {
        // Initialize waypoints (same as base class)
        if (relativePoints)
        {
            for (int j = 0; j < wayPoints.Length; j++)
            {
                wayPoints[j] = wayPoints[j] + (Vector2)transform.position;
            }
        }

        // Initialize to starting point
        currentWaypointIndex = startingPoint;
        transform.position = wayPoints[currentWaypointIndex];
        startingPosition = transform.position;
    }

    protected override void Update()
    {
        // Don't move if waiting at waypoint or returning to start
        if (isWaitingAtWaypoint || isReturningToStart)
        {
            return;
        }

        // If player is not on elevator, start return sequence
        if (!isPlayerOnElevator)
        {
            if (returnCoroutine == null)
            {
                returnCoroutine = StartCoroutine(ReturnToStartSequence());
            }
            return;
        }

        // Player is on elevator - stop return sequence and move normally
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
            isReturningToStart = false;
        }

        // Normal waypoint movement
        float distanceToWaypoint = Vector2.Distance(transform.position, wayPoints[currentWaypointIndex]);

        // Calculate speed multiplier based on distance (closer = slower)
        float speedMultiplier = 1f;
        if (distanceToWaypoint < slowdownDistance)
        {
            float normalizedDistance = distanceToWaypoint / slowdownDistance;
            speedMultiplier = Mathf.Lerp(minSpeedMultiplier, 1f, normalizedDistance);
        }

        // Apply speed multiplier to movement
        float currentSpeed = moveSpeed * speedMultiplier;
        transform.position = Vector2.MoveTowards(transform.position, wayPoints[currentWaypointIndex], currentSpeed * Time.deltaTime);

        // Check if we've reached the current waypoint
        if (distanceToWaypoint < 0.02f)
        {
            // Wait at waypoint before moving to next
            if (waitCoroutine == null)
            {
                waitCoroutine = StartCoroutine(WaitAtWaypoint());
            }
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaitingAtWaypoint = true;
        yield return new WaitForSeconds(waypointWaitTime);

        // Move to next waypoint
        currentWaypointIndex++;
        if (currentWaypointIndex >= wayPoints.Length)
        {
            currentWaypointIndex = 0;
        }

        isWaitingAtWaypoint = false;
        waitCoroutine = null;
    }

    private IEnumerator ReturnToStartSequence()
    {
        // Wait for return wait time
        yield return new WaitForSeconds(returnWaitTime);

        // If player still not on elevator, return to start
        if (!isPlayerOnElevator)
        {
            isReturningToStart = true;

            // Move back to starting position
            while (Vector2.Distance(transform.position, startingPosition) > 0.02f && !isPlayerOnElevator)
            {
                float distanceToStart = Vector2.Distance(transform.position, startingPosition);

                // Calculate speed multiplier for smooth slowdown
                float speedMultiplier = 1f;
                if (distanceToStart < slowdownDistance)
                {
                    float normalizedDistance = distanceToStart / slowdownDistance;
                    speedMultiplier = Mathf.Lerp(minSpeedMultiplier, 1f, normalizedDistance);
                }

                float currentSpeed = moveSpeed * speedMultiplier;
                transform.position = Vector2.MoveTowards(transform.position, startingPosition, currentSpeed * Time.deltaTime);
                yield return null;
            }

            // Reset to starting waypoint
            currentWaypointIndex = startingPoint;
            transform.position = startingPosition;
            isReturningToStart = false;
        }

        returnCoroutine = null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerOnElevator = true;
            other.transform.parent.gameObject.transform.SetParent(transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerOnElevator = false;
            other.transform.parent.gameObject.transform.SetParent(null);
        }
    }

    void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
}
