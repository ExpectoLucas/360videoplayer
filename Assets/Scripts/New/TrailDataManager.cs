using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Video;

[Serializable]
public class TrailPoint
{
    public float time;
    public float angle;  // For S: horizontal angle (-180 to 180), For B: pitch angle (-90 to 90)

    public TrailPoint(float time, float angle)
    {
        this.time = Mathf.Round(time * 100f) / 100f;  // Keep two decimal places
        this.angle = Mathf.Round(angle * 100f) / 100f;  // Keep two decimal places
    }
}

[Serializable]
public class TrailData
{
    public List<TrailPoint> points = new List<TrailPoint>();
    public string videoName;
    public DateTime recordTime;
    public string angleType;  // "Horizontal" or "Vertical"
    public double videoDuration;  // Total video duration (seconds)
    public string userName;  // User name
    public float fov;  // Field of view for corresponding direction (horizontal or vertical)

    public TrailData(string videoName, string angleType, double duration, string userName, float fov)
    {
        this.videoName = videoName;
        this.recordTime = DateTime.Now;
        this.angleType = angleType;
        this.videoDuration = Math.Round(duration * 100.0) / 100.0;  // Keep two decimal places
        this.userName = userName;
        this.fov = Mathf.Round(fov * 100f) / 100f;  // Keep two decimal places
    }
}

public class TrailDataManager : MonoBehaviour
{
    private static TrailDataManager instance;
    public static TrailDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("TrailDataManager");
                instance = go.AddComponent<TrailDataManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private TrailData horizontalTrailData;
    private TrailData verticalTrailData;
    private VideoPlayer videoPlayer;
    private string videoFileName;
    private string userName;
    private Camera vrCamera;  // VR camera reference

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Display data save path - changed to NewData folder
        string savePath = Path.Combine(Application.dataPath, "TrailData");
        Debug.Log($"Trail data will be saved to: {savePath}");

        // Get VR camera
        vrCamera = Camera.main;
        if (vrCamera == null)
        {
            Debug.LogWarning("Could not find main camera, FOV will be set to default value");
        }
    }

    public void Initialize(VideoPlayer videoPlayer)
    {
        this.videoPlayer = videoPlayer;
        
        // Get video filename and username from VideoPlayerUIController
        VideoPlayerUIController controller = FindObjectOfType<VideoPlayerUIController>();
        if (controller != null)
        {
            // Get username
            userName = controller.userName;
            if (string.IsNullOrEmpty(userName))
            {
                userName = "User";
                Debug.LogWarning("Username is empty, using default name 'User'");
            }

            // Get video filename
            if (!string.IsNullOrEmpty(controller.videoURL) && controller.videoURL != "null")
            {
                videoFileName = Path.GetFileNameWithoutExtension(controller.videoURL);
                Debug.Log($"Video filename extracted from URL: {videoFileName}");
            }
            else
            {
                videoFileName = "unknown_video";
                Debug.LogWarning("Could not get video filename from VideoPlayerUIController, using 'unknown_video'");
            }
        }
        else
        {
            userName = "User";
            videoFileName = "unknown_video";
            Debug.LogWarning("Could not find VideoPlayerUIController, using default values");
        }

        // Wait until video is prepared before initializing data collections
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        // Get video duration
        double duration = vp.length;
        Debug.Log($"Video duration: {duration} seconds");

        // Get horizontal and vertical FOV
        float verticalFov = vrCamera != null ? vrCamera.fieldOfView : 60f;  // Vertical FOV
        float aspect = vrCamera != null ? vrCamera.aspect : 16f/9f;
        float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad) * aspect) * Mathf.Rad2Deg;
        Debug.Log($"Device FOV - Horizontal: {horizontalFov} degrees, Vertical: {verticalFov} degrees");

        // Initialize two data collections, each using the corresponding FOV
        horizontalTrailData = new TrailData(videoFileName, "Horizontal", duration, userName, horizontalFov);
        verticalTrailData = new TrailData(videoFileName, "Vertical", duration, userName, verticalFov);

        // Subscribe to video end event
        vp.loopPointReached += OnVideoEnd;
    }

    public void AddTrailPoint(float time, float angle, bool isHorizontal)
    {
        if (isHorizontal)
        {
            if (horizontalTrailData != null)
            {
                horizontalTrailData.points.Add(new TrailPoint(time, angle));
            }
        }
        else
        {
            if (verticalTrailData != null)
            {
                verticalTrailData.points.Add(new TrailPoint(time, angle));
            }
        }
    }

    /// <summary>
    /// Manually save current Trail data and restart recording
    /// </summary>
    public void SaveCurrentTrailDataAndRestart()
    {
        // Save current data
        SaveTrailData();
        
        // Restart recording
        RestartRecording();
    }

    /// <summary>
    /// Public method: Save current data only (do not restart recording)
    /// </summary>
    public void SaveCurrentTrailData()
    {
        SaveTrailData();
    }

    /// <summary>
    /// Restart recording (clear current data and reinitialize)
    /// </summary>
    private void RestartRecording()
    {
        if (videoPlayer != null)
        {
            // Get video duration
            double duration = videoPlayer.length;
            
            // Get horizontal and vertical FOV
            float verticalFov = vrCamera != null ? vrCamera.fieldOfView : 60f;  // Vertical FOV
            float aspect = vrCamera != null ? vrCamera.aspect : 16f/9f;
            float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad) * aspect) * Mathf.Rad2Deg;

            // Reinitialize data collections
            horizontalTrailData = new TrailData(videoFileName, "Horizontal", duration, userName, horizontalFov);
            verticalTrailData = new TrailData(videoFileName, "Vertical", duration, userName, verticalFov);
            
            Debug.Log("Trail data recording restarted");
        }
        else
        {
            Debug.LogWarning("Cannot restart recording: VideoPlayer is null");
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        SaveTrailData();
    }

    private void OnApplicationQuit()
    {
        SaveTrailData();
    }

    private void SaveTrailData()
    {
        if ((horizontalTrailData == null || horizontalTrailData.points.Count == 0) &&
            (verticalTrailData == null || verticalTrailData.points.Count == 0))
        {
            Debug.Log("No trail data to save");
            return;
        }

        // Change base directory to TrailData folder
        string baseDirectory = Path.Combine(Application.dataPath, "TrailData");
        if (!Directory.Exists(baseDirectory))
        {
            Directory.CreateDirectory(baseDirectory);
        }

        // Create folder containing username, timestamp and video name
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string folderName = $"{userName}_{timestamp}_{videoFileName}";
        string sessionDirectory = Path.Combine(baseDirectory, folderName);
        
        try
        {
            Directory.CreateDirectory(sessionDirectory);
            Debug.Log($"Created session directory: {sessionDirectory}");

            // Save horizontal angle data
            if (horizontalTrailData != null && horizontalTrailData.points.Count > 0)
            {
                string horizontalPath = Path.Combine(sessionDirectory, "Horizontal.json");
                string horizontalJson = JsonUtility.ToJson(horizontalTrailData, true);
                File.WriteAllText(horizontalPath, horizontalJson);
                Debug.Log($"Horizontal trail data saved to: {horizontalPath}");
            }

            // Save vertical angle data
            if (verticalTrailData != null && verticalTrailData.points.Count > 0)
            {
                string verticalPath = Path.Combine(sessionDirectory, "Vertical.json");
                string verticalJson = JsonUtility.ToJson(verticalTrailData, true);
                File.WriteAllText(verticalPath, verticalJson);
                Debug.Log($"Vertical trail data saved to: {verticalPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving trail data: {e.Message}");
        }
        finally
        {
            // Reset data
            horizontalTrailData = null;
            verticalTrailData = null;
        }
    }
}