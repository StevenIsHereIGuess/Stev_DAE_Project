using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/PlayerStats", order = 1)]
public class PlayerStats : ScriptableObject
{
    // Player's walking speed
    public float walkSpeed = 5f;

    // Player's air movement speed
    public float airSpeed = 3f;

    // Force applied when the player jumps
    public float jumpForce = 10f;

    // Speed during the air dash
    public float airDashSpeed = 15f;

    // Duration of the air dash
    public float airDashDuration = 0.2f;
}
