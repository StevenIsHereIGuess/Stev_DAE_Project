using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform teleportDestination; // Destination where the player will be teleported

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Teleport the player to the specified destination
            other.transform.position = teleportDestination.position;

            Debug.Log("Player teleported to: " + teleportDestination.position);
        }
    }
}
