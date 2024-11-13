using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum to represent player states
public enum PlayerState
{
    Idle,
    Walking,
    Jumping
}

public class PlayerMovement : MonoBehaviour
{
    // Movement variables
    public float walkSpeed = 5f;
    public float airSpeed = 3f;
    public float jumpForce = 10f;
    private float moveDirection;
    private bool isGrounded;
    private bool isJumpingLocked;
    private Rigidbody2D rb;

    // Opponent facing variables
    public Transform opponent;  // Assign your opponent in the Inspector

    // Ground check variables
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private ContactFilter2D groundFilter;

    // Player state
    public PlayerState currentState = PlayerState.Idle;
    private PlayerState previousState;

    // Input Buffer Variables
    private bool jumpInputBuffered;
    private float bufferTime = 0.15f; // Time window for input buffer
    private float bufferTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useLayerMask = true;
        groundFilter.useTriggers = false;

        isJumpingLocked = false;
    }

    void Update()
    {
        if (!isJumpingLocked)
        {
            moveDirection = Input.GetAxisRaw("Horizontal");
        }

        GroundCheck();
        UpdatePlayerState();

        // Buffer jump input when space is pressed
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            isJumpingLocked = true; // Lock jump direction once we initiate a jump
        }

        // Buffer jump if pressed even when not grounded
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpInputBuffered = true;
        }

        // Check for buffered inputs
        if (jumpInputBuffered && isGrounded)
        {
            Jump();
            jumpInputBuffered = false;
        }

        // Buffer time countdown
        if (jumpInputBuffered)
        {
            bufferTimer += Time.deltaTime;
            if (bufferTimer > bufferTime)
            {
                jumpInputBuffered = false; // Reset buffer if the time expires
                bufferTimer = 0f;
            }
        }

        if (isGrounded)
            isJumpingLocked = false; // Reset lock when grounded

        FaceOpponent();
    }

    void FixedUpdate()
    {
        MovePlayer(moveDirection, isGrounded ? walkSpeed : airSpeed);
    }

    void GroundCheck()
    {
        Collider2D[] results = new Collider2D[5];
        int numFound = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundFilter, results);

        bool wasGrounded = isGrounded;
        isGrounded = numFound > 0;

        if (isGrounded && !wasGrounded)
        {
            Debug.Log("Grounded");
            currentState = PlayerState.Idle;
        }
        else if (!isGrounded && wasGrounded)
        {
            Debug.Log("Not grounded");
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(moveDirection * airSpeed, 0f); // Lock in horizontal direction on jump
        rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse); // Apply vertical jump force
        currentState = PlayerState.Jumping;
    }

    void UpdatePlayerState()
    {
        if (currentState == PlayerState.Idle && moveDirection != 0)
            currentState = PlayerState.Walking;
        else if (currentState == PlayerState.Walking && moveDirection == 0)
            currentState = PlayerState.Idle;
        else if (currentState == PlayerState.Jumping && isGrounded)
            currentState = PlayerState.Idle;

        if (currentState != previousState)
        {
            Debug.Log("Player State: " + currentState);
            previousState = currentState;
        }
    }

    void MovePlayer(float direction, float speed)
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        }
        // When in air, horizontal velocity remains locked due to isJumpingLocked
    }

    void FaceOpponent()
    {
        if (opponent != null)
        {
            // Calculate the direction to the opponent
            Vector3 directionToOpponent = opponent.position - transform.position;

            // If the opponent is to the left, face left; if right, face right
            if (directionToOpponent.x < 0 && transform.localScale.x > 0)
            {
                FlipCharacter();
            }
            else if (directionToOpponent.x > 0 && transform.localScale.x < 0)
            {
                FlipCharacter();
            }
        }
    }

    void FlipCharacter()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;  // Invert the x-axis scale to flip
        transform.localScale = scale;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
