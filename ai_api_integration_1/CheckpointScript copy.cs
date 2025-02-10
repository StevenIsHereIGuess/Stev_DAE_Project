using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLife playerLife = other.GetComponent<PlayerLife>();
            if (playerLife != null)
            {
                playerLife.SetCheckpoint(transform.position); // Update the player's respawn position
                Debug.Log("Checkpoint triggered at: " + transform.position);
            }
            else
            {
                Debug.LogError("PlayerLife component not found on the Player!");
            }
        }
    }
}
