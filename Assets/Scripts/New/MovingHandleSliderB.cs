using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;  // For IPointerDownHandler / IPointerUpHandler
using UnityEngine.Video;  // Add Video namespace

public class MovingHandleSliderB : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI and Camera Settings")]
    public Slider slider;
    public Transform headTransform;
    public RectTransform container;
    [Tooltip("Camera used to get vertical FOV; if not set, Camera.main will be used automatically")]
    public Camera vrCamera;
    [Tooltip("Video player, used to check playback status")]
    public VideoPlayer videoPlayer;

    [Header("Handle Basic Settings")]
    [Tooltip("Fixed pixel width for handle in horizontal direction")]
    public float fixedHandleWidth = 10f;

    [Header("Trail History Settings")]
    // [Tooltip("Prefab for recording trail, needs to be a UI Image Prefab")]
    // public GameObject trailPrefab;
    [Tooltip("Time interval for recording trail (seconds), won't record repeatedly below this interval")]
    public float recordInterval = 0.1f;

    private RectTransform handleRect;
    private float recordTimer = 0f;
    private bool isDragging = false;

    void Start()
    {
        if (slider == null || headTransform == null || container == null)
        {
            Debug.LogError("Please set slider, headTransform and container in the Inspector!");
            enabled = false;
            return;
        }

        handleRect = slider.handleRect;
        if (handleRect == null)
        {
            Debug.LogError("Slider.handleRect is empty, please specify the handle RectTransform on the Slider!");
            enabled = false;
            return;
        }

        if (vrCamera == null)
            vrCamera = Camera.main;

        // Initialize TrailDataManager
        if (videoPlayer != null)
        {
            TrailDataManager.Instance.Initialize(videoPlayer);
        }
        
        // Set vertical container reference for heatmap container
        if (HeatmapManager.Instance != null)
        {
            HeatmapManager.Instance.verticalContainer = container;
        }
    }

    void LateUpdate()
    {
        // 1. Update handle anchors/size/position (using left-stretch mode)
        handleRect.anchorMin = new Vector2(0f, 0f);
        handleRect.anchorMax = new Vector2(0f, 1f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);

        float w = container.rect.width;
        float h = container.rect.height;
        float verticalFov = vrCamera != null ? vrCamera.fieldOfView : 60f;
        float handleH = h * (verticalFov / 180f);

        // In left-stretch mode, sizeDelta.y represents the offset relative to stretch height
        handleRect.sizeDelta = new Vector2(fixedHandleWidth, -h + handleH);

        float x = slider.normalizedValue * w;
        float pitch = headTransform.rotation.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        float y = -(pitch / 90f) * (h / 2f);

        handleRect.anchoredPosition = new Vector2(x, y);

        // 2. Only accumulate timer and record trail when not dragging and video is playing
        if (!isDragging && videoPlayer != null && videoPlayer.isPlaying)
        {
            recordTimer += Time.deltaTime;
            if (recordTimer >= recordInterval)
            {
                recordTimer = 0f;

                // Calculate current pitch angle (relative to horizontal point)
                pitch = headTransform.rotation.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                // Limit within -90 to 90 degrees range
                pitch = Mathf.Clamp(pitch, -90f, 90f);

                // Record trail point
                TrailDataManager.Instance.AddTrailPoint(
                    (float)videoPlayer.time,
                    -pitch,
                    false  // isHorizontal = false for B slider
                );

                // Create trail segment (commented out, kept for future use)
                // CreateTrailSegment(x, y, handleH * 0.5f);

                // **Important: After generating trail, bring handle to the front**
                handleRect.transform.SetAsLastSibling();
            }
        }
    }

    // Called when user presses on the Slider (starts dragging)
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        recordTimer = 0f; // Reset timer when drag starts
    }

    // Called when user releases mouse or controller
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        recordTimer = 0f; // Also reset after dragging ends, ensuring interval starts from 0
    }

    // Method for creating trail segments (commented out, kept for future use)
    /*
    private void CreateTrailSegment(float x, float y, float height)
    {
        GameObject go = Instantiate(trailPrefab, container);
        RectTransform rt = go.GetComponent<RectTransform>();

        rt.anchorMin = handleRect.anchorMin;
        rt.anchorMax = handleRect.anchorMax;
        rt.pivot = handleRect.pivot;
        rt.sizeDelta = new Vector2(fixedHandleWidth * 0.5f, height);
        rt.anchoredPosition = new Vector2(x, y);
    }
    */
}
