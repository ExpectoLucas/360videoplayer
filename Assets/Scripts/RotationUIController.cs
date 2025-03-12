using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class RotationUIController : MonoBehaviour
{

    public Slider timeSlider;
    public Slider rotationSlider;
    public VideoPlayer videoPlayer;
    public GameObject videoSphere;
    public TimeHandler timeHandler;
    public UITransform uiTransform;

    public Sprite playImage;
    public Sprite pauseImage;
    public Button playPauseButton;

    private string videoURL = Application.streamingAssetsPath + "/Catcafe.mp4";


    private float timer = 0;
    private float previousValue;
    private bool sliderdown = false, jumped = false;
    public static bool playing = false;
    private bool wasPlaying = false;
    private bool started = false;

    // Start is called before the first frame update

    void Awake()
    {
        // Assign a callback for when this slider changes
        rotationSlider.onValueChanged.AddListener(this.OnRotationSliderChanged);

        previousValue = rotationSlider.value;
    }
    void Start()
    {
        PlayPauseButton();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(timeSlider.normalizedValue);
        // If we interact with the slider (either by dragging or letting go)

        if (sliderdown)
        {
            if (timer < 0.5f)
            {
                videoPlayer.frame = (long)(timeSlider.value * videoPlayer.frameCount);
                //timeHandler.UpdateDraggedTime();
                timer = 1.0f;
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }
    }

    public void PlayPauseButton()
    {
        if (!started)
        {
            StartVideo();
        }
        if (playing)
        {
            videoPlayer.Pause();
            playing = false;
            playPauseButton.GetComponent<Image>().sprite = playImage;
        }
        else
        {
            videoPlayer.Play();
            playing = true;
            playPauseButton.GetComponent<Image>().sprite = pauseImage;
        }
    }

    private void StartVideo()
    {
        videoPlayer.url = videoURL;
        videoPlayer.Prepare();
        timeHandler.UpdateTotalTime();
        videoPlayer.Play();
        playPauseButton.GetComponent<Image>().sprite = pauseImage;
        started = true;
        playing = true;
    }

    public void SliderDown()
    {
        wasPlaying = videoPlayer.isPlaying;
        videoPlayer.Pause();
        playing = false;
        sliderdown = true;
        GetComponent<UITransform>().enabled = false;
    }

    public void SliderUp()
    {
        videoPlayer.frame = (long)(timeSlider.value * videoPlayer.frameCount);
        GetComponent<UITransform>().enabled = true;
        if (wasPlaying)
        {
            videoPlayer.Play();
            playing = true;
            playPauseButton.GetComponent<Image>().sprite = pauseImage;
            wasPlaying = false;
        }
        sliderdown = false;
    }

    void OnRotationSliderChanged(float value)
    {
        float delta = value - this.previousValue;
        this.videoSphere.transform.Rotate(Vector3.up * delta * 360);

        this.previousValue = value;
    }

    public void RotationSliderUp()
    {
        this.GetComponent<UITransform>().enabled = true;
    }

    public void RotationSliderDown()
    {
        this.GetComponent<UITransform>().enabled = false;
    }
}
