using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class TimeHandlerSwitchable : MonoBehaviour
{
    public VideoPlayer vp;
    public Text timeCurr;
    public Text timeTotal;
    public Slider timeSlider;
    public Image radialSlider;

    public GameObject linearObject;
    public GameObject circularObject;
    public GameObject circularCentreCover;
    private float sliderValue; //Between 0 and 1

    public enum IndicatorType
    {
        Linear = 0,
        CircularEdge = 1,
        CircularFill = 2,
    };
    public IndicatorType indicatorType;  // Dropdown menu for picking visualisation type behaviour
    private int numberOfIndicatorTypes = System.Enum.GetValues(typeof(IndicatorType)).Length;

    public QuestionnaireController questionnaireController;

    private void Awake()
    {
        // Set the window to fill the screen without black borders
       // Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow, 60);

        Physics.autoSimulation = false;
        Physics2D.autoSimulation = false;
    }

    void Start()
    {
        StartCoroutine(UpdateTime());
        Debug.Log("TimeHandlerStart");
        vp.loopPointReached += CheckOver;
        vp.errorReceived += ErrorReceived;
    }

    public void AdvanceIndicatorType()
    {
        indicatorType = (IndicatorType)(((int)indicatorType + 1) % numberOfIndicatorTypes);
        SwitchType(indicatorType);
    }

    void SwitchType(IndicatorType type)
    {
        if (type == IndicatorType.Linear)
        {
            linearObject.SetActive(true);
            circularObject.SetActive(false);
        }
        else if (type == IndicatorType.CircularEdge)
        {
            linearObject.SetActive(false);
            circularObject.SetActive(true);
            circularCentreCover.SetActive(true);
        }
        else if (type == IndicatorType.CircularFill)
        {
            linearObject.SetActive(false);
            circularObject.SetActive(true);
            circularCentreCover.SetActive(false);
        }
    }

    IEnumerator UpdateTime()
    {
        while (true)
        {
            if (!vp.isPaused)
            {
                // Adjust time displays
                timeCurr.text = (System.Math.Floor(vp.time/60)).ToString()+':'+System.Math.Floor(vp.time%60).ToString("00");

                // Adjust slider position
                UpdateSlider(indicatorType);
                //Debug.Log("Length = " + vp.length);
                //Debug.Log("Slider pos = " + (float)(vp.time / vp.length));

                //timeSlider.SetValueWithoutNotify((float)(vp.time / vp.length));
                //Debug.Log("Slider value = " + timeSlider.normalizedValue); 
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateSlider(IndicatorType type)
    {
        sliderValue = (float)(vp.time / vp.length);
        //if (type == IndicatorType.Linear)
        //{
            timeSlider.normalizedValue = sliderValue;
        //}
        //else if (type == IndicatorType.CircularEdge || indicatorType == IndicatorType.CircularFill)
        //{
            radialSlider.fillAmount = sliderValue;
        //}
    }

    private void UpdateSlider(IndicatorType type, double time)
    {
        sliderValue = (float)(time / vp.length);
        //if (type == IndicatorType.Linear)
        //{
        timeSlider.normalizedValue = sliderValue;
        //}
        //else if (type == IndicatorType.CircularEdge || indicatorType == IndicatorType.CircularFill)
        //{
        radialSlider.fillAmount = sliderValue;
        //}
    }

    private float GetSliderValue()
    {
        if (indicatorType == IndicatorType.Linear)
        {
            return timeSlider.normalizedValue;
        }
        else if (indicatorType == IndicatorType.CircularEdge || indicatorType == IndicatorType.CircularFill)
        {
            return radialSlider.fillAmount;
        }
        return 0;
    }

    void CheckOver(VideoPlayer vp)
    {
        ForceUpdateTimeAndSlider();
        questionnaireController.NotifyEnded();
    }

    void ErrorReceived(object sender, string message)
    {
        GetComponent<InputHandler>().HandleVideoError(sender, message);
    }

    public void ForceUpdateTime()
    {
        // Adjust time displays
        timeCurr.text = (System.Math.Floor(vp.time / 60)).ToString() + ':' + System.Math.Floor(vp.time % 60).ToString("00");
        timeTotal.text = (System.Math.Floor(vp.length / 60)).ToString() + ':' + System.Math.Floor(vp.length % 60).ToString("00");
    }

    public void ForceUpdateSlider()
    {
        // Adjust slider position
        //Debug.Log("Length = " + vp.length);
        //Debug.Log("Slider pos = " + (float)(vp.time / vp.length));
        UpdateSlider(indicatorType);
        //Debug.Log("Slider value = " + timeSlider.normalizedValue);
    }

    public void ForceUpdateTimeAndSlider(double time)
    {
        ForceUpdateTime(time);
        ForceUpdateSlider(time);
    }

    public void ForceUpdateTime(double time)
    {
        // Adjust time displays
        timeCurr.text = (System.Math.Floor(time / 60)).ToString() + ':' + System.Math.Floor(time % 60).ToString("00");
    }

    public void ForceUpdateSlider(double time)
    {
        // Adjust slider position
        //Debug.Log("Length = " + vp.length);
        //Debug.Log("Slider pos = " + (float)(vp.time / vp.length));
        UpdateSlider(indicatorType, time);
        //Debug.Log("Slider value = " + timeSlider.normalizedValue);
    }

    public void ForceUpdateTimeAndSlider()
    {
        ForceUpdateTime();
        ForceUpdateSlider();
    }

    public void UpdateDraggedTime()
    {
        //Debug.Log("Length = " + vp.length);
        //Debug.Log("Slider pos = " + (float)(vp.time / vp.length));
        double time = GetSliderValue() * vp.length;
        timeCurr.text = (System.Math.Floor(time / 60)).ToString() + ':' + System.Math.Floor(time % 60).ToString("00");
       // Debug.Log("Slider value = " + timeSlider.normalizedValue);
    }

    public void StartTimer()
    {
        StartCoroutine(UpdateTime());
    }

    public void UpdateTotalTime()
    {
        StartCoroutine(PopulateTotalTime());
    }

    /// <summary>
    /// Waits for half a second, then updates the total duration of the video ONCE.
    /// </summary>
    /// <returns></returns>
    public IEnumerator PopulateTotalTime()
    {
        yield return new WaitForSeconds(0.5f);
        timeTotal.text = (System.Math.Floor(vp.length / 60)).ToString() + ':' + System.Math.Floor(vp.length % 60).ToString("00");
    }
}
