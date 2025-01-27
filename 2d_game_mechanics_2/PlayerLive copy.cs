using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    private Rigidbody2D rb;
    private Timer timer; // Reference to the Timer script
    private Vector3 respawnPosition; // To store the checkpoint position

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = FindObjectOfType<Timer>(); // Find the Timer script in the scene
        respawnPosition = transform.position; // Initial respawn position is the player's starting position

        if (timer == null)
        {
            Debug.LogError("Timer script not found in the scene!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            Die();
        }
    }

    public void Die()
    {
        rb.bodyType = RigidbodyType2D.Static;

        // Reset the timer when the player dies
        if (timer != null)
        {
            Debug.Log("Resetting timer on player death...");
            timer.ResetTimer();
        }
        else
        {
            Debug.LogError("Timer reference is missing!");
        }

        Respawn();
    }

    private void Respawn()
    {
        rb.bodyType = RigidbodyType2D.Dynamic; // Re-enable movement
        transform.position = respawnPosition;  // Move the player to the respawn position
        Debug.Log("Player respawned at: " + respawnPosition);
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        respawnPosition = checkpointPosition; // Update the respawn position
        Debug.Log("Checkpoint set at: " + checkpointPosition);
    }
}
