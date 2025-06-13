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
        this.time = time;
        this.angle = angle;
    }
}

[Serializable]
public class TrailData
{
    public List<TrailPoint> points = new List<TrailPoint>();
    public string videoName;
    public DateTime recordTime;
    public string angleType;  // "Horizontal" or "Vertical"

    public TrailData(string videoName, string angleType)
    {
        this.videoName = videoName;
        this.recordTime = DateTime.Now;
        this.angleType = angleType;
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

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // 显示数据保存路径
        string savePath = Path.Combine(Application.persistentDataPath, "TrailData");
        Debug.Log($"Trail data will be saved to: {savePath}");
    }

    public void Initialize(VideoPlayer videoPlayer)
    {
        this.videoPlayer = videoPlayer;
        
        // 从 VideoPlayerUIController 获取视频文件名和用户名
        VideoPlayerUIController controller = FindObjectOfType<VideoPlayerUIController>();
        if (controller != null)
        {
            // 获取用户名
            userName = controller.userName;
            if (string.IsNullOrEmpty(userName))
            {
                userName = "User";
                Debug.LogWarning("Username is empty, using default name 'User'");
            }

            // 获取视频文件名
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

        // 初始化两个数据集合
        horizontalTrailData = new TrailData(videoFileName, "Horizontal");
        verticalTrailData = new TrailData(videoFileName, "Vertical");

        // 订阅视频结束事件
        videoPlayer.loopPointReached += OnVideoEnd;
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

        // 创建基础目录
        string baseDirectory = Path.Combine(Application.persistentDataPath, "TrailData");
        if (!Directory.Exists(baseDirectory))
        {
            Directory.CreateDirectory(baseDirectory);
        }

        // 创建包含用户名、时间和视频名的文件夹
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string folderName = $"{userName}_{timestamp}_{videoFileName}";
        string sessionDirectory = Path.Combine(baseDirectory, folderName);
        
        try
        {
            Directory.CreateDirectory(sessionDirectory);
            Debug.Log($"Created session directory: {sessionDirectory}");

            // 保存水平角度数据
            if (horizontalTrailData != null && horizontalTrailData.points.Count > 0)
            {
                string horizontalPath = Path.Combine(sessionDirectory, "Horizontal.json");
                string horizontalJson = JsonUtility.ToJson(horizontalTrailData, true);
                File.WriteAllText(horizontalPath, horizontalJson);
                Debug.Log($"Horizontal trail data saved to: {horizontalPath}");
            }

            // 保存垂直角度数据
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
            // 重置数据
            horizontalTrailData = null;
            verticalTrailData = null;
        }
    }
} 