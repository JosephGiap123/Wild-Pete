using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Vector2[] wayPoints;
    public bool relativePoints = true;
    public int startingPoint = 0;

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
    void Update()
    {
        // Move towards current waypoint every frame
        transform.position = Vector2.MoveTowards(transform.position, wayPoints[i], moveSpeed * Time.deltaTime);

        // Check if we've reached the current waypoint
        if (Vector2.Distance(transform.position, wayPoints[i]) < 0.02f)
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

    void OnDrawGizmos()
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
