using UnityEngine;

// A simple logger class to track player state changes
public class StateLogger : MonoBehaviour
{
    // Logs the state of the player to the Unity console
    public void LogState(string state)
    {
        Debug.Log("Player State: " + state);
    }
}
