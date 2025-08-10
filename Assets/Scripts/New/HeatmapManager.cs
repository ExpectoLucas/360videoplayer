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

[Serializable]
public class POIPoint
{
    public float time;
    public float Hangle;  // Horizontal angle
    public float Vangle;  // Vertical angle
    public string hint;   // Hint information

    public POIPoint(float time, float hangle, float vangle, string hint)
    {
        this.time = time;
        this.Hangle = hangle;
        this.Vangle = vangle;
        this.hint = hint;
    }
}

[Serializable]
public class POIData
{
    public List<POIPoint> points = new List<POIPoint>();
    public string videoName;
}

public class HeatmapManager : MonoBehaviour
{
    [Header("Heatmap Settings")]
    [Tooltip("Heatmap color gradient")]
    public Gradient heatmapGradient = new Gradient();
    [Tooltip("Maximum transparency of heatmap")]
    [Range(0f, 1f)]
    public float maxAlpha = 0.7f;
    [Tooltip("Heatmap grid density (grids per second)")]
    [Range(0.1f, 10f)]
    public float timeGridDensity = 1f; // 1 grid per second
    [Tooltip("Angle grid density (grids per degree)")]
    [Range(0.1f, 5f)]
    public float angleGridDensity = 1f; // 1 grid per degree
    [Tooltip("Heatmap texture resolution")]
    [Range(64, 512)]
    public int textureResolution = 256;
    
    [Header("UI Components")]
    [Tooltip("Horizontal slider container (for displaying horizontal heatmap)")]
    public RectTransform horizontalContainer;
    [Tooltip("Vertical slider container (for displaying vertical heatmap)")]
    public RectTransform verticalContainer;
    
    [Header("POI Settings")]
    [Tooltip("Size of POI red dots")]
    [Range(2f, 20f)]
    public float poiDotSize = 8f;
    [Tooltip("Color of POI red dots")]
    public Color poiColor = Color.red;
    [Tooltip("Font size of POI hint text")]
    [Range(8, 24)]
    public int hintTextSize = 12;
    
    private Dictionary<string, HeatmapData> horizontalData = new Dictionary<string, HeatmapData>();
    private Dictionary<string, HeatmapData> verticalData = new Dictionary<string, HeatmapData>();
    private Dictionary<string, POIData> poiData = new Dictionary<string, POIData>();
    
    private GameObject horizontalHeatmapObject;
    private GameObject verticalHeatmapObject;
    private Image horizontalHeatmapImage;
    private Image verticalHeatmapImage;
    
    
    private List<GameObject> horizontalPOIObjects = new List<GameObject>();
    private List<GameObject> verticalPOIObjects = new List<GameObject>();
    private GameObject hintPanel;
    private Text hintText;
    
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
        
        // Initialize color gradient
        InitializeGradient();
    }
    
    private void InitializeGradient()
    {
        // Use Viridis color scheme
        GradientColorKey[] colorKeys = new GradientColorKey[5];
        colorKeys[0] = new GradientColorKey(new Color(0.267004f, 0.004874f, 0.329415f), 0.0f);  // Dark purple
        colorKeys[1] = new GradientColorKey(new Color(0.127568f, 0.566949f, 0.550556f), 0.25f); // Cyan
        colorKeys[2] = new GradientColorKey(new Color(0.369214f, 0.788888f, 0.382914f), 0.5f);  // Green
        colorKeys[3] = new GradientColorKey(new Color(0.988362f, 0.998364f, 0.644924f), 0.75f); // Yellow
        colorKeys[4] = new GradientColorKey(new Color(0.993248f, 0.906157f, 0.143936f), 1.0f);  // Bright yellow
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(0.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(maxAlpha, 1.0f);
        
        heatmapGradient.SetKeys(colorKeys, alphaKeys);
    }
    
    public void LoadAndDisplayHeatmap(string videoName, string userName)
    {
        ClearExistingHeatmaps();
        LoadHistoricalData(videoName, userName);
        LoadPOIData(videoName);
        GenerateHeatmaps();
        GeneratePOIMarkers();
    }
    
    private void LoadHistoricalData(string videoName, string userName)
    {
        string baseDirectory = Path.Combine(Application.dataPath, "TrailData");
        if (!Directory.Exists(baseDirectory))
        {
            Debug.Log("No trail data directory found");
            return;
        }
        
        Debug.Log($"Searching for heatmap data in: {baseDirectory}");
        Debug.Log($"Looking for video: {videoName}, user: {userName}");
        
        // Find folders containing specified video name and user name
        string[] sessionDirectories = Directory.GetDirectories(baseDirectory);
        bool foundData = false;
        
        foreach (string sessionDir in sessionDirectories)
        {
            string folderName = Path.GetFileName(sessionDir);
            Debug.Log($"Checking folder: {folderName}");
            
            // Check if folder name contains video name and user name
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
        
        // Load horizontal data
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
    
    private void LoadPOIData(string videoName)
    {
        string poiPath = Path.Combine(Application.dataPath, "poi.json");
        if (!File.Exists(poiPath))
        {
            Debug.Log("poi.json file not found");
            return;
        }
        
        try
        {
            string json = File.ReadAllText(poiPath);
            POIData data = JsonUtility.FromJson<POIData>(json);
            
            if (data != null && data.points != null)
            {
                
                List<POIPoint> filteredPoints = new List<POIPoint>();
                foreach (var point in data.points)
                {
                    
                    if (string.IsNullOrEmpty(data.videoName) || data.videoName == videoName)
                    {
                        filteredPoints.Add(point);
                    }
                }
                
                if (filteredPoints.Count > 0)
                {
                    POIData filteredData = new POIData
                    {
                        points = filteredPoints,
                        videoName = videoName
                    };
                    poiData[videoName] = filteredData;
                    Debug.Log($"Loaded {filteredPoints.Count} POI points for video: {videoName}");
                }
                else
                {
                    Debug.Log($"No POI points found for video: {videoName}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading POI data: {e.Message}");
        }
    }
    
    private void GenerateHeatmaps()
    {
        Debug.Log("Generating heatmaps...");
        
        // Generate horizontal heatmap
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
        
        // Generate vertical heatmap
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
        
        
        Texture2D heatmapTexture = CreateHeatmapTexture(data, true);
        
        
        if (horizontalHeatmapObject == null)
        {
            horizontalHeatmapObject = new GameObject("HorizontalHeatmap");
            horizontalHeatmapObject.transform.SetParent(container, false);
            
            RectTransform rectTransform = horizontalHeatmapObject.AddComponent<RectTransform>();
            horizontalHeatmapImage = horizontalHeatmapObject.AddComponent<Image>();
            
            
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            
            horizontalHeatmapObject.transform.SetAsFirstSibling();
        }
        
        
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
        
        
        Texture2D heatmapTexture = CreateHeatmapTexture(data, false);
        

        if (verticalHeatmapObject == null)
        {
            verticalHeatmapObject = new GameObject("VerticalHeatmap");
            verticalHeatmapObject.transform.SetParent(container, false);
            
            RectTransform rectTransform = verticalHeatmapObject.AddComponent<RectTransform>();
            verticalHeatmapImage = verticalHeatmapObject.AddComponent<Image>();
            

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            

            verticalHeatmapObject.transform.SetAsFirstSibling();
        }
        

        Sprite heatmapSprite = Sprite.Create(heatmapTexture, new Rect(0, 0, heatmapTexture.width, heatmapTexture.height), Vector2.zero);
        verticalHeatmapImage.sprite = heatmapSprite;
        
        Debug.Log("Vertical heatmap generated successfully");
    }
    
    private Texture2D CreateHeatmapTexture(HeatmapData data, bool isHorizontal)
    {
        // Determine texture dimensions
        int width, height;
        if (isHorizontal)
        {
            // Horizontal heatmap: X-axis angle, Y-axis time
            width = Mathf.RoundToInt(360f * angleGridDensity);
            height = Mathf.RoundToInt((float)data.videoDuration * timeGridDensity);
        }
        else
        {
            // Vertical heatmap: X-axis time, Y-axis angle
            width = Mathf.RoundToInt((float)data.videoDuration * timeGridDensity);
            height = Mathf.RoundToInt(180f * angleGridDensity);
        }
        
  
        width = Mathf.Clamp(width, 64, textureResolution);
        height = Mathf.Clamp(height, 64, textureResolution);
        
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
     
        Dictionary<Vector2Int, int> gridVisits = new Dictionary<Vector2Int, int>();

     
        Dictionary<int, int> axisMax = new Dictionary<int, int>();   


        
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
                    // Horizontal heatmap: X-axis angle, Y-axis time
                    int x = Mathf.Clamp(Mathf.FloorToInt((expandedAngle + 180f) / 360f * width), 0, width - 1);
                    int y = Mathf.Clamp(Mathf.FloorToInt((float)point.time / (float)data.videoDuration * height), 0, height - 1);
                    gridPos = new Vector2Int(x, y);
                }
                else
                {
                    // Vertical heatmap: X-axis time, Y-axis angle
                    int x = Mathf.Clamp(Mathf.FloorToInt((float)point.time / (float)data.videoDuration * width), 0, width - 1);
                    int y = Mathf.Clamp(Mathf.FloorToInt((expandedAngle + 90f) / 180f * height), 0, height - 1);
                    gridPos = new Vector2Int(x, y);
                }
                
                if (!gridVisits.ContainsKey(gridPos))
                    gridVisits[gridPos] = 0;
                gridVisits[gridPos]++;

                int axisKey = isHorizontal ? gridPos.y : gridPos.x;
                if (!axisMax.ContainsKey(axisKey) || gridVisits[gridPos] > axisMax[axisKey])
                    axisMax[axisKey] = gridVisits[gridPos];
            }
        }
        

        foreach (var kvp in gridVisits)
        {
            int axisKey = isHorizontal ? kvp.Key.y : kvp.Key.x;
            if (axisMax.TryGetValue(axisKey, out int localMax) && localMax > 0)
            {
                float intensity = (float)kvp.Value / localMax;   // 0-1 normalized
                Color color = heatmapGradient.Evaluate(intensity);
                int index = kvp.Key.y * width + kvp.Key.x;
                if (index >= 0 && index < pixels.Length)
                    pixels[index] = color;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    /// <summary>
    /// Expand horizontal angle range based on FOV, handling circular characteristics
    /// </summary>
    private List<float> ExpandAngleByFOV(float centerAngle, float fov)
    {
        List<float> expandedAngles = new List<float>();
        float halfFov = fov * 0.5f;
        
        // Calculate angle range
        float startAngle = centerAngle - halfFov;
        float endAngle = centerAngle + halfFov;
        
        // Handle circular characteristics
        if (startAngle < -180f)
        {
            // Need to cross -180 degree boundary
            // First part: from -180 degrees to endAngle
            for (float angle = -180f; angle <= endAngle; angle += 1f / angleGridDensity)
            {
                expandedAngles.Add(angle);
            }
            
            // Second part: from startAngle+360 degrees to 180 degrees (equivalent to the other side of the circle)
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
            // Need to cross 180 degree boundary
            // First part: from startAngle to 180 degrees
            for (float angle = startAngle; angle <= 180f; angle += 1f / angleGridDensity)
            {
                expandedAngles.Add(angle);
            }
            
            // Second part: from -180 degrees to endAngle-360 degrees (equivalent to the other side of the circle)
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
            // Normal case, not crossing boundaries
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
    /// Expand vertical angle range based on FOV, clipping parts that exceed -90 to 90 degrees
    /// </summary>
    private List<float> ExpandVerticalAngleByFOV(float centerAngle, float fov)
    {
        List<float> expandedAngles = new List<float>();
        float halfFov = fov * 0.5f;
        
        // Calculate angle range
        float startAngle = centerAngle - halfFov;
        float endAngle = centerAngle + halfFov;
        
        // Clip parts that exceed -90 to 90 degrees
        float originalStart = startAngle;
        float originalEnd = endAngle;
        startAngle = Mathf.Max(startAngle, -90f);
        endAngle = Mathf.Min(endAngle, 90f);
        
        // Generate angle list
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
        // Clear horizontal heatmap
        if (horizontalHeatmapObject != null)
        {
            DestroyImmediate(horizontalHeatmapObject);
            horizontalHeatmapObject = null;
            horizontalHeatmapImage = null;
        }
        
        // Clear vertical heatmap
        if (verticalHeatmapObject != null)
        {
            DestroyImmediate(verticalHeatmapObject);
            verticalHeatmapObject = null;
            verticalHeatmapImage = null;
        }
        
        // Clear POI markers
        ClearPOIMarkers();
        
        // Clear data
        horizontalData.Clear();
        verticalData.Clear();
        poiData.Clear();
    }
    
    public void ClearHeatmaps()
    {
        ClearExistingHeatmaps();
        
        // Clear hint panel
        if (hintPanel != null)
        {
            DestroyImmediate(hintPanel);
            hintPanel = null;
            hintText = null;
        }
    }
    
    private void GeneratePOIMarkers()
    {
        Debug.Log("Generating POI markers...");
        
        // Clear previous POI markers
        ClearPOIMarkers();
        
        // Create hint panel (if it doesn't exist)
        CreateHintPanel();
        
        foreach (var kvp in poiData)
        {
            string videoName = kvp.Key;
            POIData data = kvp.Value;
            
            Debug.Log($"Generating POI markers for video: {videoName}, POI count: {data.points.Count}");
            
            foreach (var poi in data.points)
            {
                // Create POI markers in horizontal container
                if (horizontalContainer != null)
                {
                    CreatePOIMarker(poi, horizontalContainer, true);
                }
                
                // Create POI markers in vertical container
                if (verticalContainer != null)
                {
                    CreatePOIMarker(poi, verticalContainer, false);
                }
            }
        }
        
        Debug.Log($"Generated POI markers: {horizontalPOIObjects.Count} horizontal, {verticalPOIObjects.Count} vertical");
    }
    
    private void CreatePOIMarker(POIPoint poi, RectTransform container, bool isHorizontal)
    {
        // Create POI marker game object
        GameObject poiObject = new GameObject($"POI_{poi.time}_{(isHorizontal ? "H" : "V")}");
        poiObject.transform.SetParent(container, false);
        
        // Add RectTransform component
        RectTransform rectTransform = poiObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(poiDotSize, poiDotSize);
        
        // Add Image component to display red dot
        Image dotImage = poiObject.AddComponent<Image>();
        dotImage.color = poiColor;
        
        // Create circular texture
        Texture2D circleTexture = CreateCircleTexture((int)poiDotSize);
        Sprite circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, circleTexture.width, circleTexture.height), Vector2.one * 0.5f);
        dotImage.sprite = circleSprite;
        
        // Calculate POI position in container
        Vector2 normalizedPosition = CalculatePOIPosition(poi, isHorizontal);
        
        // Set anchor and position
        rectTransform.anchorMin = normalizedPosition;
        rectTransform.anchorMax = normalizedPosition;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Add hover detection component
        POIHoverDetector hoverDetector = poiObject.AddComponent<POIHoverDetector>();
        hoverDetector.Initialize(poi.hint, this);
        
        // Ensure POI is displayed on top layer
        poiObject.transform.SetAsLastSibling();
        
        // Add to corresponding list
        if (isHorizontal)
        {
            horizontalPOIObjects.Add(poiObject);
        }
        else
        {
            verticalPOIObjects.Add(poiObject);
        }
        
        Debug.Log($"Created POI marker at position {normalizedPosition} for {(isHorizontal ? "horizontal" : "vertical")} container");
    }
    
    private Vector2 CalculatePOIPosition(POIPoint poi, bool isHorizontal)
    {
        Vector2 normalizedPosition;
        
        if (isHorizontal)
        {
            // Horizontal heatmap: X-axis angle (-180 to 180), Y-axis time
            // Get total video duration (from loaded heatmap data)
            double videoDuration = GetVideoDuration();
            
            float normalizedX = (poi.Hangle + 180f) / 360f; // Map -180 to 180 to 0 to 1
            float normalizedY = (float)(poi.time / videoDuration); // Map time to 0 to 1
            
            normalizedPosition = new Vector2(
                Mathf.Clamp01(normalizedX),
                Mathf.Clamp01(normalizedY)
            );
        }
        else
        {
            // Vertical heatmap: X-axis time, Y-axis angle (-90 to 90)
            double videoDuration = GetVideoDuration();
            
            float normalizedX = (float)(poi.time / videoDuration); // Map time to 0 to 1
            float normalizedY = (poi.Vangle + 90f) / 180f; // Map -90 to 90 to 0 to 1
            
            normalizedPosition = new Vector2(
                Mathf.Clamp01(normalizedX),
                Mathf.Clamp01(normalizedY)
            );
        }
        
        return normalizedPosition;
    }
    
    private double GetVideoDuration()
    {
        // Get video duration from loaded heatmap data
        foreach (var kvp in horizontalData)
        {
            return kvp.Value.videoDuration;
        }
        foreach (var kvp in verticalData)
        {
            return kvp.Value.videoDuration;
        }
        return 60.0; // Default value
    }
    
    private Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
    
                    pixels[y * size + x] = Color.white;
                }
                else
                {
        
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    private void CreateHintPanel()
    {
        if (hintPanel != null) return;
        
 
        hintPanel = new GameObject("HintPanel");
        RectTransform hintRect = hintPanel.AddComponent<RectTransform>();
        
   
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            hintPanel.transform.SetParent(canvas.transform, false);
        }
        else
        {
            Debug.LogWarning("No Canvas found for hint panel");
            return;
        }
        
 
        hintRect.sizeDelta = new Vector2(200, 50);
        hintRect.anchorMin = Vector2.zero;
        hintRect.anchorMax = Vector2.zero;

        Image backgroundImage = hintPanel.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.8f); 
        
   
        GameObject textObject = new GameObject("HintText");
        textObject.transform.SetParent(hintPanel.transform, false);
        
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);
        
        hintText = textObject.AddComponent<Text>();
        hintText.text = "";
        hintText.fontSize = hintTextSize;
        hintText.color = Color.white;
        hintText.alignment = TextAnchor.MiddleCenter;
        

        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null)
        {
            hintText.font = font;
        }
        

        hintPanel.SetActive(false);
    }
    
    private void ClearPOIMarkers()
    {
        // Clear horizontal POI markers
        foreach (var obj in horizontalPOIObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        horizontalPOIObjects.Clear();
        
        // Clear vertical POI markers
        foreach (var obj in verticalPOIObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        verticalPOIObjects.Clear();
    }
    
    public void ShowHint(string hint, Vector3 worldPosition)
    {
        if (hintPanel == null || hintText == null) return;
        
        hintText.text = hint;
        hintPanel.SetActive(true);
        
        // Convert world coordinates to screen coordinates
        Canvas canvas = hintPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Camera cam = canvas.worldCamera ?? Camera.main;
            
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPosition);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out localPoint);
            
            hintPanel.GetComponent<RectTransform>().localPosition = localPoint + new Vector2(10, 10); // Slightly offset
        }
    }
    
    public void HideHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }
}