using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Vector2[] wayPoints;
    public bool relativePoints = true;
    public int startingPoint = 0;

    [Header("Slowdown Settings")]
    [Tooltip("Distance from waypoint where slowdown begins")]
    public float slowdownDistance = 2f;
    [Tooltip("Minimum speed multiplier when at waypoint (0-1)")]
    [Range(0f, 1f)]
    public float minSpeedMultiplier = 0.2f;

    private int i;

    void Start()
    {
        if (relativePoints)
        {
            for (int j = 0; j < wayPoints.Length; j++)
            {
                wayPoints[j] = wayPoints[j] + (Vector2)transform.position;
            }
        }

        // Initialize i to starting point
        i = startingPoint;
        transform.position = wayPoints[i];
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Calculate distance to current waypoint
        float distanceToWaypoint = Vector2.Distance(transform.position, wayPoints[i]);

        // Calculate speed multiplier based on distance (closer = slower)
        float speedMultiplier = 1f;
        if (distanceToWaypoint < slowdownDistance)
        {
            // Smoothly interpolate from minSpeedMultiplier to 1.0 as distance increases
            // Closer to waypoint (smaller distance) = slower speed
            float normalizedDistance = distanceToWaypoint / slowdownDistance;
            speedMultiplier = Mathf.Lerp(minSpeedMultiplier, 1f, normalizedDistance);
        }

        // Apply speed multiplier to movement
        float currentSpeed = moveSpeed * speedMultiplier;
        transform.position = Vector2.MoveTowards(transform.position, wayPoints[i], currentSpeed * Time.deltaTime);

        // Check if we've reached the current waypoint
        if (distanceToWaypoint < 0.02f)
        {
            i++;
            if (i >= wayPoints.Length)
            {
                i = 0;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.parent.gameObject.transform.SetParent(transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.parent.gameObject.transform.SetParent(null);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < wayPoints.Length; i++)
        {
            if (relativePoints)
            {
                Gizmos.DrawSphere(wayPoints[i] + (Vector2)transform.position, 0.1f);
            }
            else
            {
                Gizmos.DrawSphere(wayPoints[i], 0.1f);
            }
        }
        if (relativePoints)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, wayPoints[i] + (Vector2)transform.position);
        }
        else
        {
            Gizmos.DrawLine(transform.position, wayPoints[i]);
        }
    }
}
