using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum to represent player states
public enum PlayerState
{
    Idle,
    Walking,
    Running,
    Jumping,
    DoubleJumping
}

public class PlayerMovement : MonoBehaviour
{
    // Movement variables
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;  // The force applied when jumping
    public float friction = 10f;    // The friction factor that slows the character down
    private float currentSpeed;     // Current speed of the player (walk or run)
    private float moveDirection;    // Stores the direction of the input
    private bool isGrounded;        // Checks if the player is on the ground
    private bool wasGrounded;       // Tracks the previous grounded state

    private Rigidbody2D rb;
    public LayerMask groundLayer;   // Layer to check if the player is on the ground
    public Transform groundCheck;    // Empty GameObject positioned at the player's feet for ground checking
    public float groundCheckRadius = 0.2f; // Radius for the ground check

    private ContactFilter2D groundFilter;   // Filter to ignore the player's own collider

    // Jump variables
    public int maxJumps = 2;   // The maximum number of jumps allowed (1 for regular jump, 2 for double jump)
    private int remainingJumps; // The number of jumps left

    // Player state
    public PlayerState currentState = PlayerState.Idle; // Default state is Idle
    private PlayerState previousState; // Tracks the previous player state

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Setup ContactFilter2D to filter ground layer
        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useLayerMask = true;
        groundFilter.useTriggers = false;   // Ignore triggers

        // Initialize remaining jumps
        remainingJumps = maxJumps;

        // Initialize previous grounded and player state
        wasGrounded = false;
        previousState = currentState;
    }

    void Update()
    {
        // Get player input on horizontal axis
        moveDirection = Input.GetAxisRaw("Horizontal");

        // Determine if the player is running or walking
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Check if the player is grounded
        GroundCheck();

        // Update player state based on movement and jump conditions
        UpdatePlayerState();

        // Check if the player can jump
        if (Input.GetKeyDown(KeyCode.Space) && CanPlayerJump())
        {
            Jump();
            remainingJumps--; // Decrease the jump count after each jump
        }

        // Reset jump counter if grounded
        if (isGrounded)
        {
            remainingJumps = maxJumps; // Reset jump counter when grounded
        }
    }

    void FixedUpdate()
    {
        // Call the MovePlayer function to handle player movement
        MovePlayer(moveDirection, currentSpeed);
    }

    void GroundCheck()
    {
        // Store colliders found in this array
        Collider2D[] results = new Collider2D[5]; // Increased size for potential multiple collisions

        // Check for collisions using the ContactFilter2D to ignore the player's collider
        int numFound = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundFilter, results);

        // isGrounded is true if at least one collider (other than the player's own) is found
        isGrounded = numFound > 0;

        // Only log the change when the grounded state switches
        if (isGrounded != wasGrounded)
        {
            if (isGrounded)
            {
                Debug.Log("Grounded");
            }
            else
            {
                Debug.Log("Not grounded");
            }

            // Update the previous grounded state
            wasGrounded = isGrounded;
        }

        // Nested loop to check each collider found
        for (int i = 0; i < numFound; i++)
        {
            for (int j = 0; j < 1; j++) // This inner loop could be used for further checks or actions
            {
                Debug.Log("Detected collider: " + results[i].gameObject.name);
            }
        }
    }

    // Function to check if the player can jump (returns a bool)
    bool CanPlayerJump()
    {
        // The player can jump if they are grounded or if they have jumps left
        return isGrounded || remainingJumps > 0;
    }

    void Jump()
    {
        // Apply jump force using AddForce for a more natural jump with physics
        rb.velocity = new Vector2(rb.velocity.x, 0f); // Reset vertical velocity for consistent jumps
        rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
    }

    void UpdatePlayerState()
    {
        // Determine the player state based on grounded, jumping, or movement status
        if (isGrounded)
        {
            if (moveDirection != 0)
            {
                currentState = Input.GetKey(KeyCode.LeftShift) ? PlayerState.Running : PlayerState.Walking;
            }
            else
            {
                currentState = PlayerState.Idle;
            }
        }
        else
        {
            if (remainingJumps == 1)
            {
                currentState = PlayerState.Jumping;
            }
            else if (remainingJumps == 0)
            {
                currentState = PlayerState.DoubleJumping;
            }
        }

        // Only log the state change when the player state switches
        if (currentState != previousState)
        {
            Debug.Log("Player State: " + currentState);
            previousState = currentState; // Update the previous state to the current one
        }
    }

    // Function that takes 2 parameters to process movement
    void MovePlayer(float direction, float speed)
    {
        // Apply movement logic using the passed direction and speed
        if (direction != 0)
        {
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        }
        else
        {
            // Apply friction to slow down movement when no input is given
            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, friction * Time.fixedDeltaTime), rb.velocity.y);
        }
    }

    // Function that takes two parameters and returns the product of them (example of a return value function)
    float Multiply(float a, float b)
    {
        return a * b;
    }

    void OnDrawGizmos()
    {
        // Draw a sphere at the ground check position to visualize the ground check radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
