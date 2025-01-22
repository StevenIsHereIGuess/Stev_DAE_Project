using UnityEngine;
using UnityEngine.UI; 

public class Timer : MonoBehaviour
{
    [SerializeField] private Text timerText; 
    
    private float timeElapsed = 0f;
    private bool isRunning = true; 

    private void Start()
    {
        
        if (timerText == null)
        {
            timerText = GameObject.Find("TimerText")?.GetComponent<Text>();

            if (timerText == null)
            {
                Debug.LogError("Timer Text is not assigned and couldn't be found! Make sure there's a GameObject named 'TimerText' in the scene with a Text component.");
                return; 
            }
        }
    }

    private void Update()
    {
        if (isRunning)
        {
            timeElapsed += Time.deltaTime; 
            UpdateTimerUI(); 
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return; 

       
        int minutes = Mathf.FloorToInt(timeElapsed / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);

       
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StopTimer()
    {
        isRunning = false; 
    }

    public void StartTimer()
    {
        isRunning = true; 
    }

    public void ResetTimer()
    {
        timeElapsed = 0f; 
        UpdateTimerUI(); 
    }
}
