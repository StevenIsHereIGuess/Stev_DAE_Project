using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Movement variables
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;  // The force applied when jumping
    public float friction = 10f;   // The friction factor that slows the character down
    private float currentSpeed;    // Current speed of the player (walk or run)
    private float moveDirection;   // Stores the direction of the input
    private bool isGrounded;       // Checks if the player is on the ground

    private Rigidbody2D rb;
    public LayerMask groundLayer;  // Layer to check if the player is on the ground
    public Transform groundCheck;  // Empty GameObject positioned at the player's feet for ground checking
    public float groundCheckRadius = 0.2f; // Radius for the ground check

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get player input on horizontal axis
        moveDirection = Input.GetAxisRaw("Horizontal");

        // Determine if the player is running or walking
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Check if the player is grounded
        GroundCheck();

        // Debugging: Draw the ground check circle in the Scene view
        Debug.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckRadius, Color.red);

        // Jump input
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // Apply movement
        if (moveDirection != 0)
        {
            rb.velocity = new Vector2(moveDirection * currentSpeed, rb.velocity.y);
        }
        else
        {
            // Apply friction to slow down movement when no input is given
            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, friction * Time.fixedDeltaTime), rb.velocity.y);
        }

        // Update ground check in FixedUpdate
        GroundCheck();
    }

    void GroundCheck()
    {
        // Check if the player is on the ground using the groundCheck position and radius
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Jump()
    {
        // Apply jump force using AddForce for a more natural jump with physics
        rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
    }

    void OnDrawGizmos()
    {
        // Draw a sphere at the ground check position to visualize the ground check radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
