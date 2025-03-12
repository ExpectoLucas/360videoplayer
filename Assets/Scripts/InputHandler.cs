using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
//using SFB;                  // To use the Standalone File Browser package
using System;

public class InputHandler : MonoBehaviour {

    public Slider timeSlider;
    public RectTransform sliderHandle;
    public RectTransform sliderFill;
    public Slider volumeSlider;
    public Slider speedSlider;
    public VideoPlayer vp;
    public AudioSource vpAudio;

    public Image speedIcon;
    public Sprite normalIcon;
    public Sprite slowIcon;
    public Sprite fastIcon;

    public GameObject preLoadImage;
    public GameObject errorImage;
    public GameObject postSubmitImage;
    public GameObject postTimeUpImage;

    public Text totalTimeText;

    private int prevResWidth;
    private int prevResHeight;
    private bool wasPlaying; // Whether the video was playing before the slider was dragged

    int ffwdcount = 0;      // How many consecutive times the fast-forward button has been pressed
    int frevcount = 0;      // And the rewind button
    private float rewindMultiplier;

    float frameRate;

    TimeHandler timeHandler;

    public Button openButton;

    private void Start()
    {
        timeHandler = GetComponent<TimeHandler>();
    }

    public void PlayButton()
    {
        if (vp.isPlaying)       // If we are already playing the video, the Play button serves the purpose of resetting the playback speed back to 1.0x
        {
            ResetPlaybackSpeed();
        }
        else
        {
            vp.Play();
            //StartPlaybackSpeed();

            // Manually set time slider handle x position to 0, to work around new 2019.3 bug.
            sliderHandle.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -10, 20);
            sliderFill.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -10, 10);
        }
        ffwdcount = 0;
        frevcount = 0;
    }

    public void ResetPlaybackSpeed()
    {
        vp.playbackSpeed = 1f;
        ResetSpeedSlider(true);
        ffwdcount = 0;
        frevcount = 0;
    }

    public void StartPlaybackSpeed()
    {
        vp.playbackSpeed = (4f / 3f);
        ResetSpeedSlider(false);
        ffwdcount = 0;
        frevcount = 0;
    }

    public void PauseButton()
    {
        ffwdcount = 0;
        frevcount = 0;
        vp.Pause();
    }

    public void FastReverseButton()
    {
        if (!vp.isPlaying)      // If video is paused, we want to play it, otherwise the effect is not visible
        {
            vp.Play();
        }

        if (ffwdcount > 0)
        {
            ffwdcount--;
            //Debug.Log("ffwdcount = " + ffwdcount);
            UpdatePlaybackSpeed(ffwdcount);
        }
        
        else if (frevcount < 6)
        {
            frevcount++;
            //Debug.Log("frevcount = " + frevcount);
            UpdatePlaybackSpeed(-frevcount);
        }
        /*ffwdcount = 0;
        frevcount++;
        rewindMultiplier = 1f + Mathf.Pow(2, frevcount - 1) * 0.5f;
        if (frevcount == 1)
        {
            StartCoroutine(Rewind());
        }*/
    }

    private IEnumerator Rewind()
    {
        if (frevcount <= 1)     // If this is the first time we're rewinding, first get the normal framerate of the video
        {
            frameRate = vp.frameRate;
        }
        vp.Pause();             // Then pause the video so we can start jumping to still frames.
        while (frevcount > 0)
        {
            //float waittime = 1f / (frameRate * rewindMultiplier);
            //float waittime = 1/frameRate;
            //if (vp.frame > (int)rewindMultiplier-1)
            if (vp.frame > (int)(0.5f * frameRate * rewindMultiplier))  // As long as there there is enough room before the start of the video
            {
                vp.frame = vp.frame - (int)(0.5f * frameRate * rewindMultiplier);       // Jump back half a second worth of video content under the current multiplication speed.
                //vp.frame = vp.frame - (int)(0.5 * frameRate);
            }
            else
            {
                frevcount = 0;
                vp.Stop();
            }
            timeHandler.ForceUpdateTimeAndSlider();     // Update the position of the slider and the displayed time.
            yield return new WaitForSeconds(0.5f);                      // Wait half a second before the next frame update.
        }
    }

    public void JumpBackButton()    // Jump back 5 seconds
    {
        //ffwdcount = 0;
        //frevcount = 0;
        if (vp.time - 5f > 0f)
        {
            vp.time = vp.time - 5f;
        }
        else
        {
            vp.time = 0f;
        }
        //timeHandler.ForceUpdateTimeAndSlider(vp.time);
    }

    public void FastForwardButton()
    {
        if (!vp.isPlaying)      // If video is paused, we want to play it, otherwise the fast-forward is not visible.
        {
            vp.Play();
        }

        if (frevcount > 0)
        {
            frevcount--;
            //Debug.Log("frevcount = " + frevcount);
            UpdatePlaybackSpeed(-frevcount);
        }

        else if (ffwdcount < 3)
        {
            ffwdcount++;
            //Debug.Log("ffwdcount = " + ffwdcount);
            UpdatePlaybackSpeed(ffwdcount);
        }

        /*frevcount = 0;
        ffwdcount++;
        float speed = 1f + Mathf.Pow(2, ffwdcount - 1) * 0.5f;
        vp.playbackSpeed = speed;
        UpdateSpeedSlider(speed);
        */
    }

    private void UpdatePlaybackSpeed(int speedIndex)
    {
        if (vp.playbackSpeed <= 3f && vp.playbackSpeed >= 0.1f)       // Only do stuff if we are within the region available to the UI speed slider.
        {
            float speed = 1f + Mathf.Pow(2, speedIndex - 1) * 0.5f;
            
            if (speedIndex == 0)
            {
                speed = 1;
            }
            else if (Math.Sign(speedIndex) == -1)
            {
                //speed = 1 / speed;
                speed = 1f / (1f + Mathf.Pow(2, -1 * speedIndex - 1) * 0.5f);
            }

            Mathf.Clamp(speed, 0.1f, 3f);   // Limit to usable range

            vp.playbackSpeed = speed;
            UpdateSpeedSlider(speed);
            //Debug.Log(speed);
        }
    }

    public void JumpForwardButton() // Jump forward 5 seconds
    {
        //frevcount = 0;
        //ffwdcount = 0;
        if (vp.time + 5f < vp.length)
        {
            vp.time = vp.time + 5f;
        }
        else
        {
            vp.time = vp.length;
        }
        //timeHandler.ForceUpdateTimeAndSlider(vp.time);
    }

    public void SliderPointerDown()
    {
        if (vp.isPlaying)
        {
            wasPlaying = true;
        } else
        {
            wasPlaying = false;
        }
        PauseButton();
    }

    public void SliderPointerUp()
    {
        vp.time = (timeSlider.normalizedValue) * vp.length;
        //vp.frame = (long)(vp.time * vp.frameRate);
        if (wasPlaying)
        {
            //PlayButton();       // Only resume playback if the video was playing before the drag action started
            vp.Play();            // But don't reset playback speed, so just give the play command directly.
        }
    }

    public void FrameStepButton()
    {
        if (!vp.isPaused)
        {
            vp.Pause();
        }
        vp.StepForward();
        timeHandler.ForceUpdateTimeAndSlider();
    }

    public void FrameBackButton()
    {
        if (!vp.isPaused)
        {
            vp.Pause();
        }
        if (vp.frame > 0) {
            vp.frame = vp.frame - 1;
        }
        timeHandler.ForceUpdateTimeAndSlider();
    }

    /*public static string ShowOpenPanel(bool includeFiles, bool includeDirectories)
    {
        UnityEngine.WSA.Application.InvokeOnUIThread(async () => {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add("mp4");
            var file = await filePicker.PickSingleFileAsync();
        }, false);
        return string.Empty;
    }*/

    public void OpenFile()
    {/*
        vp.Pause();
        var extensions = new[] {
            new ExtensionFilter("Video files", "mp4", "mov", "avi", "wmv", "mkv" )
        };

        //string previousURL = vp.url;

        // For the sake of the experiment: after opening a video disable this button to prevent cheating.
        if (GetComponentInParent<ExperimentToggle>().experimenting)
        {
            openButton.interactable = false;
        }

        try
        {
            vp.url = StandaloneFileBrowser.OpenFilePanel("Open video file", "./", extensions, false)[0];
        }
        catch (IndexOutOfRangeException)
        {
            // If opening a file was cancelled, this is the result. Stop playback, enable the default image again, and re-enable Open button functionality.
            vp.Stop();
            preLoadImage.SetActive(true);
            openButton.interactable = true;
        }

        if (vp.url != null) // Only try playing if a file was selected. If there was no file loaded, we cannot play.
        {
            PlayButton();
            StartPlaybackSpeed();   // Set playback speed to 1.33
            preLoadImage.SetActive(false);
            errorImage.SetActive(false);
            postSubmitImage.SetActive(false);
            postTimeUpImage.SetActive(false);

            TimeHandler timeHandler = GetComponent<TimeHandler>();
            timeHandler.UpdateTotalTime();
        }*/
    }

    public void OpenFile(string filename)
    {/*
        vp.Pause();
        
        // For the sake of the experiment: after opening a video disable this button to prevent cheating.
        //if (GetComponentInParent<ExperimentToggle>().experimenting)
        //{
        //    openButton.interactable = false;
        //}

        // Load video by URL
        vp.url = ("./" +filename+".mp4");

        if (vp.url != null) // Only try playing if a file was selected. If there was no file loaded, we cannot play.
        {
            //PlayButton();             // Not doing this here; playing is managed by the QuestionnaireController
            //StartPlaybackSpeed();     // Set playback speed to 1.33
            //preLoadImage.SetActive(false);
            //errorImage.SetActive(false);
            //postSubmitImage.SetActive(false);
            //postTimeUpImage.SetActive(false);

            TimeHandler timeHandler = GetComponent<TimeHandler>();
            timeHandler.UpdateTotalTime();
        }*/
    }

    public void HandleVideoError(object sender, string message)
    {
        //Debug.Log(message);
        if (message.Contains("Can't play movie"))
        {
            errorImage.SetActive(true);
            vp.Stop();
        }
        else
        {
            vp.Pause();
        }
    }

    public void ResetAfterExperimentAnswer()
    {
        postSubmitImage.SetActive(true);
        openButton.interactable = true;
    }

    public void ResetAfterTimeUp()
    {
        postTimeUpImage.SetActive(true);
        openButton.interactable = true;
    }

    public void VolumeSliderChange()
    {
        vpAudio.volume = volumeSlider.normalizedValue;
    }

    public void SpeedSliderChange()
    {
        if (speedSlider.normalizedValue < 0.5)
        {
            vp.playbackSpeed = 0.1f * Mathf.Pow(100f, speedSlider.normalizedValue); //Value 0 on slider corresponds to speed 0.1, value 0.5 is speed 1, value 1 is speed 10. Function: y = 100^(x-0.5)
        }                                                                           // Min speed is x0.1
        else
        {
            //vp.playbackSpeed = 4f * speedSlider.normalizedValue - 1f;        // Max speed is x3
            vp.playbackSpeed = 2f * speedSlider.normalizedValue;        // Max speed is x2
        }
        // Update icon (and text?) to show current speed
        if (speedSlider.normalizedValue < 0.47f)
        {
            speedIcon.sprite = slowIcon;
        }
        else if (speedSlider.normalizedValue > 0.53f)
        {
            speedIcon.sprite = fastIcon;
        }
        else
        {
            speedIcon.sprite = normalIcon;
        }
        //For debug purposes, display the speed in one of the time displays
        //totalTimeText.text = vp.playbackSpeed.ToString();
    }

    private void ResetSpeedSlider(bool isPlaying)
    {
        if (isPlaying)      // If the video is playing already, we want to reset it to 1x speed.
        {
            speedSlider.normalizedValue = 0.5f;
        }
        else                // If we are starting the video for the first time, we set the default starting speed to 1.33x to help playing the whole video take less time out of the 2 minutes.
        {
            speedSlider.normalizedValue = 0.667f;                   
        }
    }

    public void SpeedSliderPointerUp()
    {
        ffwdcount = 0;          // Reset the keyboard fast-forwards and slowdowns.
        frevcount = 0;
    }

    private void UpdateSpeedSlider(float speed)
    {
        if (speed < 1)
        {
            speedSlider.normalizedValue = 0.5f * (Mathf.Log10(speed) + 1);
        }
        else        // If the speed is 1 or higher, we use the linear function
        {
            speedSlider.normalizedValue = speed/2f;
        }
    }

    public void ToggleFullscreen()
    {
        if (!Screen.fullScreen) {
            // Store previous resolution before fullscreening
            prevResWidth = Screen.currentResolution.width;
            prevResHeight = Screen.currentResolution.height;
        }

        // Toggle fullscreen
        Screen.fullScreen = !Screen.fullScreen;
        if (!Screen.fullScreen)
        {
            Screen.SetResolution(prevResWidth, prevResHeight, true);
        }
        //GetComponent<ToolbarToggler>().ForceVisible();
    }

    // **************************
    // * Keyboard handling here *
    // **************************
    private void Update()
    {
        // Space: for playing and pausing. Play/Pause is always responsive, the other keys are prioritised to only allow one at once.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (vp.isPlaying)
            {
                PauseButton();
            } else
            {
                PlayButton();
            }
        }

        // Left/Right: for jumping back and forth
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            JumpBackButton();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            JumpForwardButton();
        }

        // < and >, for frame-by-frame
        else if (Input.GetKeyDown(KeyCode.Comma))
        {
            FrameBackButton();
        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            FrameStepButton();
        }

        // Up and Down, for fast forward/reverse
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            FastForwardButton();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            FastReverseButton();
        }

        else if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFullscreen();
        }

        //if (Event.current.Equals(Event.KeyboardEvent(""))) {}         // Idea to circumvent Update function, but it's apparently worse.
    }

}
