using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public Transform player; // Reference to the player
    public float speed = 2f; // Speed of the enemy
    public float stopDistance = 1f; // Distance at which the enemy stops following the player

    private Rigidbody2D rb;
    private Vector2 movement;

    public GameObject enemyPrefab; // Enemy prefab for spawning
    public float spawnDelay = 1f; // Delay before spawning the enemy

    private bool isSpawned = false; // Flag to ensure the enemy spawns only once

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (player == null)
        {
            Debug.LogError("Player reference is missing! Assign the player Transform in the inspector.");
        }

        if (!isSpawned)
        {
            StartCoroutine(SpawnEnemyAfterDelay());
        }
    }

    void Update()
    {
        if (player != null && isSpawned)
        {
            // Calculate direction towards the player
            Vector2 direction = (player.position - transform.position).normalized;

            // Stop moving if within stopDistance
            if (Vector2.Distance(player.position, transform.position) > stopDistance)
            {
                movement = direction;
            }
            else
            {
                movement = Vector2.zero;
            }
        }
    }

    void FixedUpdate()
    {
        if (isSpawned)
        {
            // Move the enemy
            MoveEnemy();
        }
    }

    void MoveEnemy()
    {
        rb.velocity = new Vector2(movement.x * speed, movement.y * speed);
    }

    IEnumerator SpawnEnemyAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        isSpawned = true;
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
