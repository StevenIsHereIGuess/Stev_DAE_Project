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
    public float airSpeed = 3f;
    public float travelSpeed = 10f;
    public float jumpForce = 10f;
    public float friction = 10f;
    public float travelDistance = 5f;   // Distance to travel during the "travel" action
    private float currentSpeed;
    private float moveDirection;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isTraveling;
    private Rigidbody2D rb;

    // Ground check variables
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private ContactFilter2D groundFilter;

    // Jump variables
    public int maxJumps = 2;
    private int remainingJumps;

    // Player state
    public PlayerState currentState = PlayerState.Idle;
    private PlayerState previousState;

    // Data tracking
    private Dictionary<string, int> playerData = new Dictionary<string, int>
    {
        { "Health", 100 },
        { "Stamina", 50 },
        { "Score", 0 }
    };

    // Array for player abilities
    private string[] abilities = { "Dash", "Jump", "Slide" };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useLayerMask = true;
        groundFilter.useTriggers = false;

        remainingJumps = maxJumps;
        wasGrounded = false;
        previousState = currentState;
    }

    void Update()
    {
        moveDirection = Input.GetAxisRaw("Horizontal");
        currentSpeed = isGrounded ? (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) : airSpeed;

        GroundCheck();
        UpdatePlayerState();

        if (Input.GetKeyDown(KeyCode.Space) && CanPlayerJump())
        {
            Jump();
            remainingJumps--;
        }

        if (isGrounded) remainingJumps = maxJumps;

        if (Input.GetKeyDown(KeyCode.Q) && !isGrounded) StartCoroutine(TravelInDirection());

        // Call the function with parameters
        UpdatePlayerData("Score", 10);
    }

    void FixedUpdate()
    {
        if (!isTraveling) MovePlayer(moveDirection, currentSpeed);
    }

    void GroundCheck()
    {
        Collider2D[] results = new Collider2D[5];
        int numFound = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundFilter, results);

        isGrounded = numFound > 0;
        if (isGrounded != wasGrounded)
        {
            Debug.Log(isGrounded ? "Grounded" : "Not grounded");
            wasGrounded = isGrounded;
        }

        // Nested loop to check colliders
        for (int i = 0; i < numFound; i++)
        {
            for (int j = 0; j < results.Length; j++)
            {
                if (results[j] != null)
                {
                    Debug.Log("Detected collider: " + results[i].gameObject.name);
                }
            }
            break;
        }
    }

    bool CanPlayerJump()
    {
        return isGrounded || remainingJumps > 0;
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
    }

    void UpdatePlayerState()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                if (moveDirection != 0)
                {
                    currentState = Input.GetKey(KeyCode.LeftShift) ? PlayerState.Running : PlayerState.Walking;
                }
                break;
            case PlayerState.Walking:
                if (moveDirection == 0) currentState = PlayerState.Idle;
                else if (Input.GetKey(KeyCode.LeftShift)) currentState = PlayerState.Running;
                break;
            case PlayerState.Running:
                if (moveDirection == 0) currentState = PlayerState.Idle;
                else if (!Input.GetKey(KeyCode.LeftShift)) currentState = PlayerState.Walking;
                break;
            case PlayerState.Jumping:
                if (remainingJumps == 0) currentState = PlayerState.DoubleJumping;
                break;
            case PlayerState.DoubleJumping:
                if (isGrounded) currentState = PlayerState.Idle;
                break;
        }

        if (currentState != previousState)
        {
            Debug.Log("Player State: " + currentState);
            previousState = currentState;
        }
    }

    void MovePlayer(float direction, float speed)
    {
        if (direction != 0)
        {
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, friction * Time.fixedDeltaTime), rb.velocity.y);
        }
    }

    float Multiply(float a, float b)
    {
        return a * b;
    }

    // Function with parameters
    void UpdatePlayerData(string key, int value)
    {
        if (playerData.ContainsKey(key))
        {
            playerData[key] += value;
            Debug.Log($"Updated {key}: {playerData[key]}");
        }
    }

    IEnumerator TravelInDirection()
    {
        isTraveling = true;
        Vector2 travelDirection = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow)) travelDirection = Vector2.up;
        else if (Input.GetKey(KeyCode.DownArrow)) travelDirection = Vector2.down;
        else if (Input.GetKey(KeyCode.LeftArrow)) travelDirection = new Vector2(-1, 0.5f);
        else if (Input.GetKey(KeyCode.RightArrow)) travelDirection = new Vector2(1, 0.5f);
        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightArrow)) travelDirection = new Vector2(0.7f, 0.7f);
        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow)) travelDirection = new Vector2(-0.7f, 0.7f);
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.RightArrow)) travelDirection = new Vector2(0.7f, -0.7f);
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftArrow)) travelDirection = new Vector2(-0.7f, -0.7f);

        rb.velocity = travelDirection.normalized * travelSpeed;
        float travelDuration = travelDistance / travelSpeed;

        yield return new WaitForSeconds(travelDuration);

        rb.velocity = Vector2.zero;
        isTraveling = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
