using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f; // Adjustable movement speed
    private Rigidbody2D rb;
    float horizontalInput;
    bool isFacingRight = true;
    bool isJumping = false;
    float jumpPower = 6f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
    }

    void Update()
    {
        // Get input for horizontal and vertical movement
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow keys
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            isJumping = true;
        }
        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
        {
            FlipSprite();
        }
    }

    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics-based movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void FlipSprite()
    {
        isFacingRight = !isFacingRight;

        // Multiply the player's X local scale by -1 to flip
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isJumping = false;
    }
}