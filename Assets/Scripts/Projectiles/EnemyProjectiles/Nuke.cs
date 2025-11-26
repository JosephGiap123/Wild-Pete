using UnityEngine;
using Unity.Cinemachine;

public class Nuke : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCol;
    [SerializeField] private LayerMask blowUpMask;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AttackHitboxInfo attackHitboxInfo;
    [SerializeField] private float destroyTimer = 15f;
    [SerializeField] private float timeToTrackPlayer = 5f;
    [SerializeField] private float trackingSpeed = 5f;
    [SerializeField] private float spawnHeightAbovePlayer = 10f;

    private float timerToFall = 1f;
    private bool isTrackingPlayer = true;
    private bool isFalling = false;
    private float trackingTimer;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform playerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        anim = GetComponent<Animator>();
        boxCol.enabled = false;
        trackingTimer = timeToTrackPlayer;
        Destroy(gameObject, destroyTimer);
    }

    private void Start()
    {
        // Get player reference
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            playerTransform = GameManager.Instance.player.transform;
        }
    }

    private void Update()
    {
        if (PauseController.IsGamePaused) return;

        if (isTrackingPlayer)
        {
            // Track player horizontally
            if (playerTransform != null)
            {
                Vector3 targetPosition = new Vector3(
                    playerTransform.position.x,
                    transform.position.y,
                    transform.position.z
                );
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, trackingSpeed * Time.deltaTime);
            }

            trackingTimer -= Time.deltaTime;
            if (trackingTimer <= 0)
            {
                // Move above player and start falling
                if (playerTransform != null)
                {
                    transform.position = new Vector3(
                        playerTransform.position.x,
                        playerTransform.position.y + spawnHeightAbovePlayer,
                        transform.position.z
                    );
                }
                anim.Play("Nuke");
                rb.gravityScale = 4f;
                boxCol.enabled = true; // Enable collider so it can detect ground collisions
                isTrackingPlayer = false;
                isFalling = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTrackingPlayer) return; // Don't explode while tracking
        if (((1 << other.gameObject.layer) & blowUpMask) != 0)
        {
            Explode();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Handle case where nuke is already inside ground when collider is enabled
        if (isTrackingPlayer) return;
        if (((1 << other.gameObject.layer) & blowUpMask) != 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        GetComponentInChildren<CinemachineImpulseSource>()?.GenerateImpulse(1.4f);
        GameObject newExplosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        ExplosionCloud explosionCloud = newExplosion.GetComponent<ExplosionCloud>();
        if (explosionCloud != null)
        {
            explosionCloud.Initialize(attackHitboxInfo);
        }
        Destroy(gameObject);
    }

    //Tracks player until a certain amount of time, then it will spawn above the player and then fall and explode.
}
