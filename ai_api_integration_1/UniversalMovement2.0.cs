using System.Collections;
using UnityEngine;

// Enum to represent player states
public enum PlayerState
{
    Idle,
    Walking,
    Jumping,
    AirDashing
}

public class UniversalMovement : MonoBehaviour
{
    public PlayerStats stats; // Reference to ScriptableObject for player stats
    public StateLogger logger; // Reference to the logger component

    // Movement variables
    private float moveDirection;
    private bool isGrounded;
    private bool isJumpingLocked;
    private Rigidbody2D rb;

    // Ground check variables
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private ContactFilter2D groundFilter;

    // Player state
    public PlayerState currentState = PlayerState.Idle;
    private PlayerState previousState;

    // Input buffer variables
    private bool jumpInputBuffered;
    private float bufferTime = 0.2f;
    private float bufferTimer;

    // Air dash variables
    private bool hasAirDashed;

    // Friction variables
    public float groundFriction = 5f; // Friction applied when grounded and player stops moving

    private bool isStopping = false; // Tracks if the player is stopping

    public delegate void PlayerAction();
    public event PlayerAction OnJump;
    public event PlayerAction OnAirDash;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useLayerMask = true;
        groundFilter.useTriggers = false;

        isJumpingLocked = false;
        hasAirDashed = false;

        if (logger == null)
        {
            logger = gameObject.AddComponent<StateLogger>();
        }
    }

    void Update()
    {
        if (!isJumpingLocked)
        {
            moveDirection = Input.GetAxisRaw("Horizontal");
        }

        Debug.Log("Move Direction: " + moveDirection);  // Debug log for move direction

        GroundCheck();
        UpdatePlayerState();

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            Jump();
            isJumpingLocked = true;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpInputBuffered = true;
        }

        if (jumpInputBuffered && isGrounded)
        {
            Jump();
            jumpInputBuffered = false;
        }

        if (jumpInputBuffered)
        {
            bufferTimer += Time.deltaTime;
            if (bufferTimer > bufferTime)
            {
                jumpInputBuffered = false;
                bufferTimer = 0f;
            }
        }

        if (isGrounded)
        {
            isJumpingLocked = false;
            hasAirDashed = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isGrounded && !hasAirDashed)
        {
            StartCoroutine(AirDash());
        }

        // Track stopping state
        if (isGrounded && moveDirection == 0 && Mathf.Abs(rb.velocity.x) > 0.01f)
        {
            isStopping = true;
        }
        else
        {
            isStopping = false;
        }
    }

    void FixedUpdate()
    {
        if (!isStopping)
        {
            MovePlayer(moveDirection, isGrounded ? stats.walkSpeed : stats.airSpeed);
        }
        else
        {
            ApplyFriction();
        }

        Debug.Log("Velocity: " + rb.velocity);  // Debug log for velocity in FixedUpdate
    }

    void GroundCheck()
    {
        Collider2D[] results = new Collider2D[5];
        int numFound = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundFilter, results);

        bool wasGrounded = isGrounded;
        isGrounded = numFound > 0;

        if (isGrounded && !wasGrounded)
        {
            logger.LogState("Grounded");
            currentState = PlayerState.Idle;
        }
        else if (!isGrounded && wasGrounded)
        {
            logger.LogState("Not grounded");
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(moveDirection * stats.airSpeed, 0f);
        rb.AddForce(new Vector2(0f, stats.jumpForce), ForceMode2D.Impulse);
        currentState = PlayerState.Jumping;
        OnJump?.Invoke();

        Debug.Log("Jumping! Velocity: " + rb.velocity);  // Debug log for jump action
    }

    IEnumerator AirDash()
    {
        currentState = PlayerState.AirDashing;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.velocity = new Vector2(moveDirection * stats.airDashSpeed, 0f);

        hasAirDashed = true;
        OnAirDash?.Invoke();
        yield return new WaitForSeconds(stats.airDashDuration);

        rb.velocity = new Vector2(0f, rb.velocity.y);
        rb.gravityScale = originalGravity;
        currentState = PlayerState.Jumping;

        Debug.Log("Air Dash! Velocity: " + rb.velocity);  // Debug log for air dash
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
            logger.LogState(currentState.ToString());
            previousState = currentState;
        }
    }

    void MovePlayer(float direction, float speed)
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
            Debug.Log("Moving with direction: " + direction + " speed: " + speed);  // Debug log for movement details
        }
        else
        {
            Debug.Log("In air, no horizontal movement applied.");  // Debug log if in air
        }
    }

    void ApplyFriction()
    {
        if (isGrounded && Mathf.Abs(rb.velocity.x) > 0.01f)
        {
            Vector2 velocity = rb.velocity;
            velocity.x = Mathf.Lerp(velocity.x, 0, groundFriction * Time.fixedDeltaTime);
            rb.velocity = velocity;
        }
    }

    void OnDrawGizmos()
    {
        // Null check for groundCheck
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        else //hola
        {
            Debug.LogWarning("GroundCheck is not assigned in the inspector.");
        }
    }
}
