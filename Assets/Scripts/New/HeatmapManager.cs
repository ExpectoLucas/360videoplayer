using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

[Serializable]
public class HeatmapData
{
    public List<TrailPoint> points;
    public string videoName;
    public string angleType;
    public double videoDuration;
    public string userName;
    public float fov;
}

public class HeatmapManager : MonoBehaviour
{
    [Header("热力图设置")]
    [Tooltip("热力图的颜色渐变")]
    public Gradient heatmapGradient = new Gradient();
    [Tooltip("热力图的最大透明度")]
    [Range(0f, 1f)]
    public float maxAlpha = 0.7f;
    [Tooltip("热力图的网格密度（每秒多少个网格）")]
    [Range(0.1f, 10f)]
    public float timeGridDensity = 1f; // 每秒1个网格
    [Tooltip("角度网格密度（每度多少个网格）")]
    [Range(0.1f, 5f)]
    public float angleGridDensity = 1f; // 每度1个网格
    [Tooltip("热力图纹理分辨率")]
    [Range(64, 512)]
    public int textureResolution = 256;
    
    [Header("UI组件")]
    [Tooltip("水平滑块容器（用于显示水平热力图）")]
    public RectTransform horizontalContainer;
    [Tooltip("垂直滑块容器（用于显示垂直热力图）")]
    public RectTransform verticalContainer;
    
    private Dictionary<string, HeatmapData> horizontalData = new Dictionary<string, HeatmapData>();
    private Dictionary<string, HeatmapData> verticalData = new Dictionary<string, HeatmapData>();
    private GameObject horizontalHeatmapObject;
    private GameObject verticalHeatmapObject;
    private Image horizontalHeatmapImage;
    private Image verticalHeatmapImage;
    
    private static HeatmapManager instance;
    public static HeatmapManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("HeatmapManager");
                instance = go.AddComponent<HeatmapManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 初始化颜色渐变
        InitializeGradient();
    }
    
    private void InitializeGradient()
    {
        // 使用Viridis颜色方案，与test.ipynb保持一致
        GradientColorKey[] colorKeys = new GradientColorKey[5];
        colorKeys[0] = new GradientColorKey(new Color(0.267004f, 0.004874f, 0.329415f), 0.0f);  // 深紫色
        colorKeys[1] = new GradientColorKey(new Color(0.127568f, 0.566949f, 0.550556f), 0.25f); // 青色
        colorKeys[2] = new GradientColorKey(new Color(0.369214f, 0.788888f, 0.382914f), 0.5f);  // 绿色
        colorKeys[3] = new GradientColorKey(new Color(0.988362f, 0.998364f, 0.644924f), 0.75f); // 黄色
        colorKeys[4] = new GradientColorKey(new Color(0.993248f, 0.906157f, 0.143936f), 1.0f);  // 亮黄色
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(0.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(maxAlpha, 1.0f);
        
        heatmapGradient.SetKeys(colorKeys, alphaKeys);
    }
    
    public void LoadAndDisplayHeatmap(string videoName, string userName)
    {
        ClearExistingHeatmaps();
        LoadHistoricalData(videoName, userName);
        GenerateHeatmaps();
    }
    
    private void LoadHistoricalData(string videoName, string userName)
    {
        string baseDirectory = Path.Combine(Application.persistentDataPath, "TrailData");
        if (!Directory.Exists(baseDirectory))
        {
            Debug.Log("No trail data directory found");
            return;
        }
        
        Debug.Log($"Searching for heatmap data in: {baseDirectory}");
        Debug.Log($"Looking for video: {videoName}, user: {userName}");
        
        // 查找包含指定视频名和用户名的文件夹
        string[] sessionDirectories = Directory.GetDirectories(baseDirectory);
        bool foundData = false;
        
        foreach (string sessionDir in sessionDirectories)
        {
            string folderName = Path.GetFileName(sessionDir);
            Debug.Log($"Checking folder: {folderName}");
            
            // 检查文件夹名是否包含视频名和用户名
            if (folderName.Contains(videoName) && folderName.Contains(userName))
            {
                Debug.Log($"Found matching folder: {folderName}");
                LoadSessionData(sessionDir);
                foundData = true;
            }
        }
        
        if (!foundData)
        {
            Debug.Log($"No matching data found for video: {videoName}, user: {userName}");
            Debug.Log("Available folders:");
            foreach (string sessionDir in sessionDirectories)
            {
                Debug.Log($"  - {Path.GetFileName(sessionDir)}");
            }
        }
    }
    
    private void LoadSessionData(string sessionDirectory)
    {
        Debug.Log($"Loading session data from: {sessionDirectory}");
        
        // 加载水平数据
        string horizontalPath = Path.Combine(sessionDirectory, "Horizontal.json");
        if (File.Exists(horizontalPath))
        {
            try
            {
                string json = File.ReadAllText(horizontalPath);
                TrailData data = JsonUtility.FromJson<TrailData>(json);
                string key = $"{data.userName}_{data.videoName}";
                
                Debug.Log($"Loaded horizontal data: {data.points.Count} points, video: {data.videoName}, user: {data.userName}");
                
                if (!horizontalData.ContainsKey(key))
                {
                    horizontalData[key] = new HeatmapData
                    {
                        points = data.points,
                        videoName = data.videoName,
                        angleType = data.angleType,
                        videoDuration = data.videoDuration,
                        userName = data.userName,
                        fov = data.fov
                    };
                }
                else
                {
                    // 合并数据
                    horizontalData[key].points.AddRange(data.points);
                    Debug.Log($"Merged horizontal data, total points: {horizontalData[key].points.Count}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading horizontal data: {e.Message}");
            }
        }
        else
        {
            Debug.Log("Horizontal.json not found in session directory");
        }
        
        // 加载垂直数据
        string verticalPath = Path.Combine(sessionDirectory, "Vertical.json");
        if (File.Exists(verticalPath))
        {
            try
            {
                string json = File.ReadAllText(verticalPath);
                TrailData data = JsonUtility.FromJson<TrailData>(json);
                string key = $"{data.userName}_{data.videoName}";
                
                Debug.Log($"Loaded vertical data: {data.points.Count} points, video: {data.videoName}, user: {data.userName}");
                
                if (!verticalData.ContainsKey(key))
                {
                    verticalData[key] = new HeatmapData
                    {
                        points = data.points,
                        videoName = data.videoName,
                        angleType = data.angleType,
                        videoDuration = data.videoDuration,
                        userName = data.userName,
                        fov = data.fov
                    };
                }
                else
                {
                    // 合并数据
                    verticalData[key].points.AddRange(data.points);
                    Debug.Log($"Merged vertical data, total points: {verticalData[key].points.Count}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading vertical data: {e.Message}");
            }
        }
        else
        {
            Debug.Log("Vertical.json not found in session directory");
        }
    }
    
    private void GenerateHeatmaps()
    {
        Debug.Log("Generating heatmaps...");
        
        // 生成水平热力图
        if (horizontalContainer != null)
        {
            Debug.Log($"Generating horizontal heatmap with {horizontalData.Count} data sets");
            foreach (var kvp in horizontalData)
            {
                GenerateHorizontalHeatmap(kvp.Value, horizontalContainer);
            }
        }
        else
        {
            Debug.LogWarning("Horizontal container is null, skipping horizontal heatmap generation");
        }
        
        // 生成垂直热力图
        if (verticalContainer != null)
        {
            Debug.Log($"Generating vertical heatmap with {verticalData.Count} data sets");
            foreach (var kvp in verticalData)
            {
                GenerateVerticalHeatmap(kvp.Value, verticalContainer);
            }
        }
        else
        {
            Debug.LogWarning("Vertical container is null, skipping vertical heatmap generation");
        }
    }
    
    private void GenerateHorizontalHeatmap(HeatmapData data, RectTransform container)
    {
        if (data.points == null || data.points.Count == 0) 
        {
            Debug.LogWarning("No points data for horizontal heatmap");
            return;
        }
        
        Debug.Log($"Generating horizontal heatmap: {data.points.Count} points, duration: {data.videoDuration}s, FOV: {data.fov}°");
        
        // 创建热力图纹理
        Texture2D heatmapTexture = CreateHeatmapTexture(data, true);
        
        // 创建或更新热力图UI对象
        if (horizontalHeatmapObject == null)
        {
            horizontalHeatmapObject = new GameObject("HorizontalHeatmap");
            horizontalHeatmapObject.transform.SetParent(container, false);
            
            RectTransform rectTransform = horizontalHeatmapObject.AddComponent<RectTransform>();
            horizontalHeatmapImage = horizontalHeatmapObject.AddComponent<Image>();
            
            // 设置UI属性
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 确保热力图在底层显示
            horizontalHeatmapObject.transform.SetAsFirstSibling();
        }
        
        // 创建Sprite并设置到Image
        Sprite heatmapSprite = Sprite.Create(heatmapTexture, new Rect(0, 0, heatmapTexture.width, heatmapTexture.height), Vector2.zero);
        horizontalHeatmapImage.sprite = heatmapSprite;
        
        Debug.Log("Horizontal heatmap generated successfully");
    }
    
    private void GenerateVerticalHeatmap(HeatmapData data, RectTransform container)
    {
        if (data.points == null || data.points.Count == 0) 
        {
            Debug.LogWarning("No points data for vertical heatmap");
            return;
        }
        
        Debug.Log($"Generating vertical heatmap: {data.points.Count} points, duration: {data.videoDuration}s, FOV: {data.fov}°");
        
        // 创建热力图纹理
        Texture2D heatmapTexture = CreateHeatmapTexture(data, false);
        
        // 创建或更新热力图UI对象
        if (verticalHeatmapObject == null)
        {
            verticalHeatmapObject = new GameObject("VerticalHeatmap");
            verticalHeatmapObject.transform.SetParent(container, false);
            
            RectTransform rectTransform = verticalHeatmapObject.AddComponent<RectTransform>();
            verticalHeatmapImage = verticalHeatmapObject.AddComponent<Image>();
            
            // 设置UI属性
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 确保热力图在底层显示
            verticalHeatmapObject.transform.SetAsFirstSibling();
        }
        
        // 创建Sprite并设置到Image
        Sprite heatmapSprite = Sprite.Create(heatmapTexture, new Rect(0, 0, heatmapTexture.width, heatmapTexture.height), Vector2.zero);
        verticalHeatmapImage.sprite = heatmapSprite;
        
        Debug.Log("Vertical heatmap generated successfully");
    }
    
    private Texture2D CreateHeatmapTexture(HeatmapData data, bool isHorizontal)
    {
        // 确定纹理尺寸
        int width, height;
        if (isHorizontal)
        {
            // 水平热力图：X轴角度，Y轴时间
            width = Mathf.RoundToInt(360f * angleGridDensity);
            height = Mathf.RoundToInt((float)data.videoDuration * timeGridDensity);
        }
        else
        {
            // 垂直热力图：X轴时间，Y轴角度
            width = Mathf.RoundToInt((float)data.videoDuration * timeGridDensity);
            height = Mathf.RoundToInt(180f * angleGridDensity);
        }
        
        // 限制纹理尺寸以避免性能问题
        width = Mathf.Clamp(width, 64, textureResolution);
        height = Mathf.Clamp(height, 64, textureResolution);
        
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        
        // 初始化纹理为透明
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // 统计访问次数
        Dictionary<Vector2Int, int> gridVisits = new Dictionary<Vector2Int, int>();
        int maxVisits = 0;
        
        foreach (var point in data.points)
        {
            List<float> expandedAngles = isHorizontal ? 
                ExpandAngleByFOV(point.angle, data.fov) : 
                ExpandVerticalAngleByFOV(point.angle, data.fov);
            
            foreach (float expandedAngle in expandedAngles)
            {
                Vector2Int gridPos;
                if (isHorizontal)
                {
                    // 水平热力图：X轴角度，Y轴时间
                    int x = Mathf.Clamp(Mathf.FloorToInt((expandedAngle + 180f) / 360f * width), 0, width - 1);
                    int y = Mathf.Clamp(Mathf.FloorToInt((float)point.time / (float)data.videoDuration * height), 0, height - 1);
                    gridPos = new Vector2Int(x, y);
                }
                else
                {
                    // 垂直热力图：X轴时间，Y轴角度
                    int x = Mathf.Clamp(Mathf.FloorToInt((float)point.time / (float)data.videoDuration * width), 0, width - 1);
                    int y = Mathf.Clamp(Mathf.FloorToInt((expandedAngle + 90f) / 180f * height), 0, height - 1);
                    gridPos = new Vector2Int(x, y);
                }
                
                if (!gridVisits.ContainsKey(gridPos))
                    gridVisits[gridPos] = 0;
                gridVisits[gridPos]++;
                maxVisits = Mathf.Max(maxVisits, gridVisits[gridPos]);
            }
        }
        
        // 设置像素颜色
        foreach (var kvp in gridVisits)
        {
            if (kvp.Value > 0)
            {
                float intensity = (float)kvp.Value / maxVisits;
                Color color = heatmapGradient.Evaluate(intensity);
                int index = kvp.Key.y * width + kvp.Key.x;
                if (index >= 0 && index < pixels.Length)
                {
                    pixels[index] = color;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    /// <summary>
    /// 根据FOV扩展水平角度范围，处理环形特性
    /// </summary>
    private List<float> ExpandAngleByFOV(float centerAngle, float fov)
    {
        List<float> expandedAngles = new List<float>();
        float halfFov = fov * 0.5f;
        
        // 计算角度范围
        float startAngle = centerAngle - halfFov;
        float endAngle = centerAngle + halfFov;
        
        // 处理环形特性
        if (startAngle < -180f)
        {
            // 需要跨越-180度边界
            // 第一部分：从-180度到endAngle
            for (float angle = -180f; angle <= endAngle; angle += 1f / angleGridDensity)
            {
                expandedAngles.Add(angle);
            }
            
            // 第二部分：从startAngle+360度到180度（相当于环的另一侧）
            float overflowStart = startAngle + 360f;
            for (float angle = overflowStart; angle <= 180f; angle += 1f / angleGridDensity)
            {
                expandedAngles.Add(angle);
            }
            
            Debug.Log($"Horizontal FOV expansion: center={centerAngle}°, FOV={fov}°, range=[{startAngle}°, {endAngle}°], " +
                     $"split into [-180°, {endAngle}°] and [{overflowStart}°, 180°], total points: {expandedAngles.Count}");
        }
        else if (endAngle > 180f)
        {
            // 需要跨越180度边界
            // 第一部分：从startAngle到180度
            for (float angle = startAngle; angle <= 180f; angle += 1f / angleGridDensity)
            {
                expandedAngles.Add(angle);
            }
            
            // 第二部分：从-180度到endAngle-360度（相当于环的另一侧）
            float overflowEnd = endAngle - 360f;
            for (float angle = -180f; angle <= overflowEnd; angle += 1f / angleGridDensity)
            {
                expandedAngles.Add(angle);
            }
            
            Debug.Log($"Horizontal FOV expansion: center={centerAngle}°, FOV={fov}°, range=[{startAngle}°, {endAngle}°], " +
                     $"split into [{startAngle}°, 180°] and [-180°, {overflowEnd}°], total points: {expandedAngles.Count}");
        }
        else
        {
            // 正常情况，不跨越边界
            for (float angle = startAngle; angle <= endAngle; angle += 1f / angleGridDensity)
            {
                expandedAngles.Add(angle);
            }
            
            Debug.Log($"Horizontal FOV expansion: center={centerAngle}°, FOV={fov}°, range=[{startAngle}°, {endAngle}°], " +
                     $"total points: {expandedAngles.Count}");
        }
        
        return expandedAngles;
    }
    
    /// <summary>
    /// 根据FOV扩展垂直角度范围，裁切超出-90到90度的部分
    /// </summary>
    private List<float> ExpandVerticalAngleByFOV(float centerAngle, float fov)
    {
        List<float> expandedAngles = new List<float>();
        float halfFov = fov * 0.5f;
        
        // 计算角度范围
        float startAngle = centerAngle - halfFov;
        float endAngle = centerAngle + halfFov;
        
        // 裁切超出-90到90度的部分
        float originalStart = startAngle;
        float originalEnd = endAngle;
        startAngle = Mathf.Max(startAngle, -90f);
        endAngle = Mathf.Min(endAngle, 90f);
        
        // 生成角度列表
        for (float angle = startAngle; angle <= endAngle; angle += 1f / angleGridDensity)
        {
            expandedAngles.Add(angle);
        }
        
        if (originalStart != startAngle || originalEnd != endAngle)
        {
            Debug.Log($"Vertical FOV expansion: center={centerAngle}°, FOV={fov}°, " +
                     $"original range=[{originalStart}°, {originalEnd}°], " +
                     $"clipped to=[{startAngle}°, {endAngle}°], total points: {expandedAngles.Count}");
        }
        else
        {
            Debug.Log($"Vertical FOV expansion: center={centerAngle}°, FOV={fov}°, " +
                     $"range=[{startAngle}°, {endAngle}°], total points: {expandedAngles.Count}");
        }
        
        return expandedAngles;
    }
    
    private void ClearExistingHeatmaps()
    {
        // 清除水平热力图
        if (horizontalHeatmapObject != null)
        {
            DestroyImmediate(horizontalHeatmapObject);
            horizontalHeatmapObject = null;
            horizontalHeatmapImage = null;
        }
        
        // 清除垂直热力图
        if (verticalHeatmapObject != null)
        {
            DestroyImmediate(verticalHeatmapObject);
            verticalHeatmapObject = null;
            verticalHeatmapImage = null;
        }
        
        // 清除数据
        horizontalData.Clear();
        verticalData.Clear();
    }
    
    public void ClearHeatmaps()
    {
        ClearExistingHeatmaps();
    }
} 