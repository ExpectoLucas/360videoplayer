using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerUIController : MonoBehaviour
{

    public Slider timeSlider;
    public VideoPlayer videoPlayer;
    public GameObject videoSphere;
    public TimeHandler timeHandler;

    public Sprite playImage;
    public Sprite pauseImage;
    public Button playPauseButton;

    private float timer = 0;
    private float skippingTime = 10;
    float rotationSpeed = 2f;
    public bool sliderdown = false;
    public static bool playing = false;
    private bool wasPlaying = false;
    private bool started = false;
    //public GameObject thumbnailCanvas;
    //public RawImage thumbnailImage;
    
    private float startTime; // Start timer
    private float elapsedTime;

    //video for different experiments
    //private string videoURL = "C:/Users/chens/Desktop/Thesis/video/selected/Costa_Rica.mp4";//1
    public string videoURL = "null";
    
    public float videoStartTime = 0.0f;
    
    // set video URL for different scenes
    public void SetVideoURL(string videoSource)
    {
        string videoURL = videoSource.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        //thumbnailCanvas.SetActive(false);
        //PlayPauseButton(); //Enabled. Pause the video when the scene is loaded
    }

    // Update is called once per frame
    void Update()
    {
        // If we are using the time slider, do this on every update
        if (sliderdown)
        {
            // Timer is needed to let the videoplayer load. If there is no timer, the update will happen too fast, and the player will not load in time, making it unrsponsive.
            if (timer < 0.5f)
            {
                videoPlayer.frame = (long)(timeSlider.value * videoPlayer.frameCount);
                //thumbnailImage.texture = videoPlayer.texture;
                //timeHandler.UpdateDraggedTime();
                timer = 1.0f;
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }
        // Check for mouse interaction (in case of the desktop video player)
        else if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            GrabRotation();
        }
        else if (Input.GetKeyDown("r"))
        {
            videoSphere.transform.rotation = Quaternion.Euler(180, 0, 0);
        }
    }

    // Play/pause button, referenced in the OnClick of the UI Button element.
    public void PlayPauseButton()
    {
        // If the video has not been started, load the url.
        if (!started)
        {
            StartVideo();
        }
        // If the video is playing, pause it.
        else if (playing)
        {   
            videoPlayer.Pause();
            playing = false;
            playPauseButton.GetComponent<Image>().sprite = playImage;
            Debug.Log("[Control panel] pasued");
        }
        else
        {
            videoPlayer.Play();
            playing = true;
            playPauseButton.GetComponent<Image>().sprite = pauseImage;
            Debug.Log("[Control panel] played");

        }
    }

    // To start the video, load the video url and play the video.
    private void StartVideo()
    {
        videoPlayer.url = videoURL;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
        timeHandler.UpdateTotalTime();
        //videoPlayer.Play();
        playPauseButton.GetComponent<Image>().sprite = pauseImage;
        started = true;
        playing = true;
        
        //Debug.Log("start video function called");
    }
    
    private void OnVideoPrepared(VideoPlayer player)
    {
        player.time = videoStartTime;
        player.Play();
    }

    // Timing function to determine whether the user is dragging the timeline or clicking 
    public void StartTimer()
    {
        startTime = Time.time;
    }
    public void StopTimer()
    {
        elapsedTime = Time.time - startTime;
        //Used to determine whether the user is dragging the timeline or clicking
        Debug.Log(elapsedTime <= 1 ? "[Timeline] slider clicked." : "[Timeline]slider grabbed.");
    }
    
    // This function is called when the slider is grabbed. The UI follow script is disabled in order to move the head freely without moving the UI.
    // The video player is paused, and the thumbnail canvas is enabled.
    public void SliderDown()
    {
        GetComponent<UITransform>().enabled = false;
        wasPlaying = videoPlayer.isPlaying;
        videoPlayer.Pause();
        playing = false;
        sliderdown = true;
        //thumbnailCanvas.SetActive(true);
        
        StartTimer();
        Debug.Log("[Timeline] press: " + timeHandler.timeCurr.text);
    }

    // This function is called when the slider is let go. The UI follow script is enabled again, and the thumbnail canvas is disabled.
    // The video player frame is set according to the current value of the time slider.
    // If the video was playing before, then start playing it again.
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

        //thumbnailCanvas.SetActive(false);
        
        StopTimer();
        Debug.Log("[Timeline] release: " + timeHandler.timeCurr.text);

    }

    // The skip forward function, called by the UI button.
    public void SkipForward()
    {
        if (videoPlayer.time + skippingTime < videoPlayer.length)
        {
            videoPlayer.time = videoPlayer.time + skippingTime;
        }
        else
        {
            videoPlayer.time = videoPlayer.length;
        }
    }

    // The skip backward function, called by the UI button.
    public void SkipBackward()
    {
        if (videoPlayer.time - skippingTime > 0f)
        {
            videoPlayer.time = videoPlayer.time - skippingTime;
        }
        else
        {
            videoPlayer.time = 0f;
        }
    }

    // Sphere rotation based on mouse input.
    private void GrabRotation()
    {
        float xAxisRotation = Input.GetAxis("Mouse X") * rotationSpeed;
        float yAxisRotation = Input.GetAxis("Mouse Y") * rotationSpeed;

        videoSphere.transform.Rotate(Vector3.up * xAxisRotation, Space.World);
        videoSphere.transform.Rotate(Vector3.right * -yAxisRotation, Space.World);
    }
}
