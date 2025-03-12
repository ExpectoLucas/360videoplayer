using UnityEngine;
using UnityEngine.UI;
using System;

public class Timer : MonoBehaviour
{
    public float timeSpent = 0.0f;
    public Button startButton;
    public Button stopButton;

    private bool isTimerRunning = false;

    //public VideoPlayerUIController videoPlayerUIController;
    public TimeHandler timeHandler;
    
    void Start()
    {
        startButton.onClick.AddListener(StartTimer);
        stopButton.onClick.AddListener(StopTimer);
    }

    void Update()
    {
        if (isTimerRunning)
        {
            timeSpent += Time.deltaTime;
        }
    }

    void StartTimer()
    {
        if (isTimerRunning == false)
        {
            Debug.Log("[Timer] Timer start！");
        }
        isTimerRunning = true;
    }

    void StopTimer()
    {
        isTimerRunning = false;
        
        //print time spent
        //Debug.Log("Time spent: " + timeLeft);
        TimeSpan time = TimeSpan.FromSeconds(timeSpent);
        string outputTime = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        Debug.Log("[Timer] Time spent in this task: " + outputTime);
        
        //print the time on timeline when click the Finished button
        Debug.Log("[Timeline] Ending Time = " + timeHandler.timeCurr.text);

    }
}
