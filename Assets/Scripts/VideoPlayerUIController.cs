using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerUIController : MonoBehaviour
{
    [Header("用户信息")]
    public string userName = "User";  // 用户名，可在 Inspector 中设置

    [Header("UI 组件")]
    public Slider timeSlider;
    public Slider timeSlider2; // 第二个时间滑块
    public VideoPlayer videoPlayer;
    public GameObject videoSphere;
    public TimeHandler timeHandler;

    public Sprite playImage;
    public Sprite pauseImage;
    public Button playPauseButton;

    [Header("热力图设置")]
    [Tooltip("水平滑块容器（用于显示水平热力图）")]
    public RectTransform horizontalHeatmapContainer;
    [Tooltip("垂直滑块容器（用于显示垂直热力图）")]
    public RectTransform verticalHeatmapContainer;

    private float timer = 0;
    private float skippingTime = 10;
    float rotationSpeed = 2f;
    public bool sliderdown = false;
    public bool slider2down = false; // 第二个滑块的按下状态
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
    
    private bool isUpdatingSlider = false; // 防止循环更新

    // Start is called before the first frame update
    void Start()
    {
        // 添加滑块值改变事件监听
        if (timeSlider != null)
            timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        if (timeSlider2 != null)
            timeSlider2.onValueChanged.AddListener(OnSlider2ValueChanged);
        //thumbnailCanvas.SetActive(false);
        //PlayPauseButton(); //Enabled. Pause the video when the scene is loaded
    }

    // 第一个滑块值改变时的回调
    private void OnSliderValueChanged(float value)
    {
        if (!isUpdatingSlider)
        {
            isUpdatingSlider = true;
            if (timeSlider2 != null)
                timeSlider2.value = value;
            isUpdatingSlider = false;
        }
    }

    // 第二个滑块值改变时的回调
    private void OnSlider2ValueChanged(float value)
    {
        if (!isUpdatingSlider)
        {
            isUpdatingSlider = true;
            if (timeSlider != null)
                timeSlider.value = value;
            isUpdatingSlider = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 如果视频正在播放，同步更新两个滑块的值
        if (playing && !sliderdown && !slider2down)
        {
            if (videoPlayer != null && videoPlayer.length > 0)
            {
                float normalizedTime = (float)(videoPlayer.time / videoPlayer.length);
                if (timeSlider != null)
                    timeSlider.value = normalizedTime;
                if (timeSlider2 != null)
                    timeSlider2.value = normalizedTime;
            }
        }

        // 处理第一个滑块
        if (sliderdown && timeSlider != null)
        {
            if (timer < 0.5f)
            {
                videoPlayer.frame = (long)(timeSlider.value * videoPlayer.frameCount);
                timer = 1.0f;
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }
        // 处理第二个滑块
        else if (slider2down && timeSlider2 != null)
        {
            if (timer < 0.5f)
            {
                videoPlayer.frame = (long)(timeSlider2.value * videoPlayer.frameCount);
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
        
        // 加载并显示热力图
        LoadHeatmap();
    }
    
    private void LoadHeatmap()
    {
        // 获取视频文件名
        string videoName = "unknown_video";
        if (!string.IsNullOrEmpty(videoURL) && videoURL != "null")
        {
            videoName = System.IO.Path.GetFileNameWithoutExtension(videoURL);
        }
        
        // 设置热力图容器的引用
        if (HeatmapManager.Instance != null)
        {
            HeatmapManager.Instance.horizontalContainer = horizontalHeatmapContainer;
            HeatmapManager.Instance.verticalContainer = verticalHeatmapContainer;
            
            // 加载并显示热力图
            HeatmapManager.Instance.LoadAndDisplayHeatmap(videoName, userName);
            Debug.Log($"Loading heatmap for video: {videoName}, user: {userName}");
        }
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

    // 第二个滑块的相关方法
    public void Slider2Down()
    {
        if (timeSlider2 == null) return;
        
        GetComponent<UITransform>().enabled = false;
        wasPlaying = videoPlayer.isPlaying;
        videoPlayer.Pause();
        playing = false;
        slider2down = true;
        StartTimer();
        Debug.Log("[Timeline2] press: " + timeHandler.timeCurr.text);
    }

    public void Slider2Up()
    {
        if (timeSlider2 == null) return;
        
        videoPlayer.frame = (long)(timeSlider2.value * videoPlayer.frameCount);
        GetComponent<UITransform>().enabled = true;
        if (wasPlaying)
        {
            videoPlayer.Play();
            playing = true;
            playPauseButton.GetComponent<Image>().sprite = pauseImage;
            wasPlaying = false;
        }
        slider2down = false;
        StopTimer();
        Debug.Log("[Timeline2] release: " + timeHandler.timeCurr.text);
    }

    // 在组件销毁时移除事件监听
    void OnDestroy()
    {
        if (timeSlider != null)
            timeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        if (timeSlider2 != null)
            timeSlider2.onValueChanged.RemoveListener(OnSlider2ValueChanged);
    }
}
