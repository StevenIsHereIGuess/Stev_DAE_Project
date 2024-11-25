using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum to represent player states
public enum PlayerState
{
    Idle,       // When the player is stationary
    Walking,    // When the player is moving on the ground
    Jumping,    // When the player is in the air after a jump
    AirDashing  // When the player performs an air dash
}

public class PlayerMovement : MonoBehaviour
{
    // Movement variables
    public float walkSpeed = 5f;   // Speed at which the player moves on the ground
    public float airSpeed = 3f;    // Speed at which the player moves in the air
    public float jumpForce = 10f; // Vertical force applied when the player jumps
    private float moveDirection;   // Stores the horizontal movement input
    private bool isGrounded;       // Checks if the player is touching the ground
    private bool isJumpingLocked;  // Prevents changing direction mid-jump
    private Rigidbody2D rb;        // Reference to the Rigidbody2D component for physics interactions

    // Ground check variables
    public LayerMask groundLayer;          // Layer mask for detecting ground objects
    public Transform groundCheck;          // Position from which to check for ground
    public float groundCheckRadius = 0.2f; // Radius of the ground check area
    private ContactFilter2D groundFilter;  // Filter to detect only ground layers

    // Player state
    public PlayerState currentState = PlayerState.Idle; // Current state of the player
    private PlayerState previousState;                 // Previous state of the player for state change detection

    // Input Buffer Variables
    private bool jumpInputBuffered; // Tracks if jump input was pressed within the buffer window
    private float bufferTime = 0.2f; // Time duration for which jump input is buffered
    private float bufferTimer;       // Timer to track the buffer duration

    // Air dash variables
    public float airDashSpeed = 15f;      // Speed of the air dash
    public float airDashDuration = 0.2f; // Duration of the air dash
    private bool hasAirDashed;            // Tracks if the player has already used their air dash

    // Event system for player actions
    public delegate void PlayerAction();
    public event PlayerAction OnJump;
    public event PlayerAction OnAirDash;

    void Start()
    {
        // Initialize Rigidbody2D and set up the ground filter
        rb = GetComponent<Rigidbody2D>();
        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useLayerMask = true;
        groundFilter.useTriggers = false;

        isJumpingLocked = false; // Ensure the player can move at the start
        hasAirDashed = false;    // Air dash is available at the start
    }

    void Update()
    {
        if (!isJumpingLocked)
        {
            // Capture horizontal movement input (-1 for left, 1 for right, 0 for none)
            moveDirection = Input.GetAxisRaw("Horizontal");
        }

        // Check if the player is on the ground and update the state
        GroundCheck();

        // Update the player's state based on movement and conditions
        UpdatePlayerState();

        // Handle jumping when up arrow is pressed
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            Jump();
            isJumpingLocked = true; // Prevent direction change during the jump
        }

        // Buffer jump input for smoother gameplay
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpInputBuffered = true;
        }

        if (jumpInputBuffered && isGrounded)
        {
            Jump();
            jumpInputBuffered = false; // Consume the buffered jump input
        }

        if (jumpInputBuffered)
        {
            // Track how long the jump input has been buffered
            bufferTimer += Time.deltaTime;
            if (bufferTimer > bufferTime)
            {
                jumpInputBuffered = false; // Clear the buffered input after the buffer time expires
                bufferTimer = 0f;
            }
        }

        if (isGrounded)
        {
            // Reset jump locking and air dash availability when grounded
            isJumpingLocked = false;
            hasAirDashed = false;
        }

        // Handle air dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isGrounded && !hasAirDashed)
        {
            StartCoroutine(AirDash()); // Perform the air dash using a coroutine
        }
    }

    void FixedUpdate()
    {
        // Move the player horizontally, adjusting speed based on grounded state
        MovePlayer(moveDirection, isGrounded ? walkSpeed : airSpeed);
    }

    void GroundCheck()
    {
        // Perform a physics overlap check to see if the player is on the ground
        Collider2D[] results = new Collider2D[5];
        int numFound = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundFilter, results);

        // Update grounded state and log state changes
        bool wasGrounded = isGrounded;
        isGrounded = numFound > 0;

        if (isGrounded && !wasGrounded)
        {
            Debug.Log("Grounded");
            currentState = PlayerState.Idle; // Player transitions to idle when grounded
        }
        else if (!isGrounded && wasGrounded)
        {
            Debug.Log("Not grounded");
        }
    }

    void Jump()
    {
        // Lock horizontal direction on jump and apply vertical jump force
        rb.velocity = new Vector2(moveDirection * airSpeed, 0f);
        rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        currentState = PlayerState.Jumping; // Update state to jumping
        OnJump?.Invoke(); // Trigger jump event
    }

    IEnumerator AirDash()
    {
        // Temporarily disable gravity and apply air dash force
        currentState = PlayerState.AirDashing;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0; // Disable gravity during air dash
        rb.velocity = new Vector2(moveDirection * airDashSpeed, 0f);

        hasAirDashed = true; // Mark air dash as used
        OnAirDash?.Invoke(); // Trigger air dash event
        yield return new WaitForSeconds(airDashDuration);

        // Restore gravity and reset horizontal momentum after the dash
        rb.velocity = new Vector2(0f, rb.velocity.y);
        rb.gravityScale = originalGravity;
        currentState = PlayerState.Jumping; // Return to jumping state after air dash
    }

    void UpdatePlayerState()
    {
        // Update the player state based on conditions
        if (currentState == PlayerState.Idle && moveDirection != 0)
            currentState = PlayerState.Walking;
        else if (currentState == PlayerState.Walking && moveDirection == 0)
            currentState = PlayerState.Idle;
        else if (currentState == PlayerState.Jumping && isGrounded)
            currentState = PlayerState.Idle;

        // Log state changes for debugging
        if (currentState != previousState)
        {
            Debug.Log("Player State: " + currentState);
            previousState = currentState;
        }
    }

    void MovePlayer(float direction, float speed)
    {
        // Apply horizontal movement based on the input direction and speed
        if (isGrounded)
        {
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        }
    }

    void OnDrawGizmos()
    {
        // Visualize the ground check area in the Scene view for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
