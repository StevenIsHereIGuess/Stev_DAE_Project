using UnityEngine;

public class Timer : MonoBehaviour
{
    private float timeElapsed = 0f; // Tracks the elapsed time
    private bool isRunning = true; // Controls whether the timer is running

    private void Update()
    {
        if (isRunning)
        {
            timeElapsed += Time.deltaTime; // Increment the timer
        }
    }

    private void OnGUI()
    {
        // Format time into minutes and seconds
        int minutes = Mathf.FloorToInt(timeElapsed / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);

        // Display the timer on the screen
        GUIStyle style = new GUIStyle();
        style.fontSize = 22; // Set the font size
        style.normal.textColor = Color.white; // Set the text color

        GUI.Label(new Rect(10, 10, 200, 50), string.Format("{0:00}:{1:00}", minutes, seconds), style);
    }

    public void ResetTimer()
    {
        timeElapsed = 0f; // Reset the elapsed time
        Debug.Log("Timer reset to 0.");
    }

    public void StopTimer()
    {
        isRunning = false; // Stops the timer
    }

    public void StartTimer()
    {
        isRunning = true; // Starts or resumes the timer
    }
}
