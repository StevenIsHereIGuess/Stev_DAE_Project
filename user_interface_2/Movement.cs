using UnityEngine;

public class SimpleWalking : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private float moveInput;

    void Start()
    {
        rb =  GetComponent<Rigidbody2D>();

    }

    void Update ()
    {
        moveInput = Input.GetAxis("Horizontal");
    }
    
    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y)
    }
}