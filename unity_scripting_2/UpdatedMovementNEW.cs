using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum to represent player states
public enum PlayerState
{
    Idle,
    Walking,
    Jumping,
    AirDashing
}

public class PlayerMovement : MonoBehaviour
{
    // Movement variables stored in ScriptableObject
    public PlayerStats stats;

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

    // Input Buffer Variables
    private bool jumpInputBuffered;
    private float bufferTime = 0.2f;
    private float bufferTimer;

    // Air dash variables
    private bool hasAirDashed;

    // Visual effects for actions
    public GameObject jumpEffectPrefab;
    public GameObject airDashEffectPrefab;

    // Event system for player actions
    public delegate void PlayerAction();
    public event PlayerAction OnJump;
    public event PlayerAction OnAirDash;

    // State logger custom component
    private StateLogger stateLogger;

    void Start()
    {
        // Initialize Rigidbody2D and set up the ground filter
        rb = GetComponent<Rigidbody2D>();
        stateLogger = GetComponentInChildren<StateLogger>();

        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useLayerMask = true;
        groundFilter.useTriggers = false;

        isJumpingLocked = false;
        hasAirDashed = false;
    }

    void Update()
    {
        if (!isJumpingLocked)
        {
            moveDirection = Input.GetAxisRaw("Horizontal");
        }

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
    }

    void FixedUpdate()
    {
        MovePlayer(moveDirection, isGrounded ? stats.walkSpeed : stats.airSpeed);
    }

    void GroundCheck()
    {
        Collider2D[] results = new Collider2D[5];
        int numFound = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundFilter, results);

        bool wasGrounded = isGrounded;
        isGrounded = numFound > 0;

        if (isGrounded && !wasGrounded)
        {
            currentState = PlayerState.Idle;
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(moveDirection * stats.airSpeed, 0f);
        rb.AddForce(new Vector2(0f, stats.jumpForce), ForceMode2D.Impulse);
        currentState = PlayerState.Jumping;

        OnJump?.Invoke();

        if (jumpEffectPrefab)
        {
            Instantiate(jumpEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    IEnumerator AirDash()
    {
        currentState = PlayerState.AirDashing;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.velocity = new Vector2(moveDirection * stats.airDashSpeed, 0f);

        hasAirDashed = true;
        OnAirDash?.Invoke();

        if (airDashEffectPrefab)
        {
            Instantiate(airDashEffectPrefab, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(stats.airDashDuration);

        rb.velocity = new Vector2(0f, rb.velocity.y);
        rb.gravityScale = originalGravity;
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
            stateLogger?.LogState(currentState.ToString());
            previousState = currentState;
        }
    }

    void MovePlayer(float direction, float speed)
    {
        rb.velocity = new Vector2(direction * speed, rb.velocity.y);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
