using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MovingHandleSliderS : MonoBehaviour
{
    [Header("UI and Camera Settings")]
    public Slider slider;
    public Transform headTransform;
    public RectTransform container;
    [Tooltip("Camera used to get FOV; if not set, Camera.main will be used automatically")]
    public Camera vrCamera;
    [Tooltip("Video player, used to check playback status")]
    public VideoPlayer videoPlayer;

    [Header("Handle Basic Settings")]
    [Tooltip("Fixed pixel size for handle in vertical direction (height)")]
    public float fixedHandleSize = 20f;

    [Header("Trail History Settings")]
    // [Tooltip("UI Image Prefab for recording trail, generates a strip segment each time")]
    // public GameObject trailPrefab;
    [Tooltip("Time interval for recording trail (seconds), won't record repeatedly below this interval")]
    public float recordInterval = 0.1f;

    private RectTransform handleRect;
    private RectTransform oppositeHandleRect;  // Additional handle for boundary crossing
    private float recordTimer = 0f;
    private float initialYaw;

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
            Debug.LogError("Slider.handleRect is empty, please specify the handle RectTransform on the Slider component!");
            enabled = false;
            return;
        }

        // Create additional handle for boundary crossing
        CreateBoundaryHandles();

        if (vrCamera == null)
            vrCamera = Camera.main;

        // Record initial head yaw as zero point
        initialYaw = -90f;

        // Initialize TrailDataManager
        if (videoPlayer != null)
        {
            TrailDataManager.Instance.Initialize(videoPlayer);
        }
        
        // Set horizontal container reference for heatmap container
        if (HeatmapManager.Instance != null)
        {
            HeatmapManager.Instance.horizontalContainer = container;
        }
    }

    private void CreateBoundaryHandles()
    {
        // Create additional handle for boundary crossing
        GameObject oppositeHandle = Instantiate(handleRect.gameObject, handleRect.parent);
        oppositeHandle.name = "OppositeHandle";
        oppositeHandleRect = oppositeHandle.GetComponent<RectTransform>();
        oppositeHandleRect.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        // —— 1. Update handle anchors/pivot/size/position —— 
        float containerW = container.rect.width;
        float containerH = container.rect.height;

        // Calculate handle width = containerW × (horizontalFOV / 360)
        float verticalFov = vrCamera != null ? vrCamera.fieldOfView : 60f;
        float aspect = vrCamera != null ? vrCamera.aspect : 16f/9f;
        float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad) * aspect) * Mathf.Rad2Deg;
        float handleW = containerW * (horizontalFov / 360f);

        // Set base properties for all handles (bottom-stretch)
        handleRect.anchorMin = new Vector2(0f, 0f);
        handleRect.anchorMax = new Vector2(1f, 0f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        oppositeHandleRect.anchorMin = new Vector2(0f, 0f);
        oppositeHandleRect.anchorMax = new Vector2(1f, 0f);
        oppositeHandleRect.pivot = new Vector2(0.5f, 0.5f);

        // Vertical position: slider progress from bottom to top
        float verticalY = slider.normalizedValue * containerH;

        // Horizontal position: head yaw cycle mapping
        float currentYaw = headTransform.rotation.eulerAngles.y;
        if (currentYaw > 180f) currentYaw -= 360f;
        float relYaw = currentYaw - initialYaw;
        relYaw = Mathf.Repeat(relYaw + 180f, 360f) - 180f;
        float normYaw = (relYaw + 180f) / 360f;
        float horizontalX = (normYaw - 0.5f) * containerW;

        // Calculate if handle crosses boundary
        float handleHalfWidth = handleW * 0.5f;
        float leftEdge  = -containerW * 0.5f;   // Container's left edge
        float rightEdge = containerW * 0.5f;    // Container's right edge

        float handleLeftEdge = horizontalX - handleHalfWidth;
        float handleRightEdge = horizontalX + handleHalfWidth;

        if (handleLeftEdge < leftEdge)
        {
            // Case when crossing left boundary
            float overflow = leftEdge - handleLeftEdge;
            float mainHandleWidth = handleW - overflow;
            float oppositeHandleWidth = overflow;
            
            // Main handle (right part)
            handleRect.sizeDelta = new Vector2(-containerW + mainHandleWidth, fixedHandleSize);
            handleRect.anchoredPosition = new Vector2(leftEdge + mainHandleWidth * 0.5f, verticalY);

            // Opposite handle (left overflow part shown on right)
            oppositeHandleRect.sizeDelta = new Vector2(-containerW + oppositeHandleWidth, fixedHandleSize);
            oppositeHandleRect.anchoredPosition = new Vector2(rightEdge - oppositeHandleWidth * 0.5f, verticalY);
            oppositeHandleRect.gameObject.SetActive(true);
        }
        else if (handleRightEdge > rightEdge)
        {
            // Case when crossing right boundary
            float overflow = handleRightEdge - rightEdge;
            float mainHandleWidth = handleW - overflow;
            float oppositeHandleWidth = overflow;
            
            // Main handle (left part)
            handleRect.sizeDelta = new Vector2(-containerW + mainHandleWidth, fixedHandleSize);
            handleRect.anchoredPosition = new Vector2(rightEdge - mainHandleWidth * 0.5f, verticalY);

            // Opposite handle (right overflow part shown on left)
            oppositeHandleRect.sizeDelta = new Vector2(-containerW + oppositeHandleWidth, fixedHandleSize);
            oppositeHandleRect.anchoredPosition = new Vector2(leftEdge + oppositeHandleWidth * 0.5f, verticalY);
            oppositeHandleRect.gameObject.SetActive(true);
        }
        else
        {
            // Normal case, no boundary crossing
            handleRect.sizeDelta = new Vector2(-containerW + handleW, fixedHandleSize);
            handleRect.anchoredPosition = new Vector2(horizontalX, verticalY);
            oppositeHandleRect.gameObject.SetActive(false);
        }

        // —— 2. Record trail at intervals —— 
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            recordTimer += Time.deltaTime;
            if (recordTimer >= recordInterval)
            {
                recordTimer = 0f;
                
                // Calculate current horizontal angle (relative to center point)
                float trailYaw = headTransform.rotation.eulerAngles.y;
                if (trailYaw > 180f) trailYaw -= 360f;
                float trailRelYaw = trailYaw - initialYaw;
                trailRelYaw = Mathf.Repeat(trailRelYaw + 180f, 360f) - 180f;

                // Record trail point
                TrailDataManager.Instance.AddTrailPoint(
                    (float)videoPlayer.time,
                    trailRelYaw,
                    true  // isHorizontal = true for S slider
                );
                
                // Create trail based on handle state (commented out, kept for future use)
                /*
                if (horizontalX - handleHalfWidth < leftEdge)
                {
                    // When crossing left boundary, create two trails
                    CreateTrailSegment(leftEdge + handleRect.sizeDelta.x * 0.5f, verticalY, handleRect.sizeDelta.x * 0.5f);
                    CreateTrailSegment(rightEdge - oppositeHandleRect.sizeDelta.x * 0.5f, verticalY, oppositeHandleRect.sizeDelta.x * 0.5f);
                }
                else if (horizontalX + handleHalfWidth > rightEdge)
                {
                    // When crossing right boundary, create two trails
                    CreateTrailSegment(rightEdge - handleRect.sizeDelta.x * 0.5f, verticalY, handleRect.sizeDelta.x * 0.5f);
                    CreateTrailSegment(leftEdge + oppositeHandleRect.sizeDelta.x * 0.5f, verticalY, oppositeHandleRect.sizeDelta.x * 0.5f);
                }
                else
                {
                    // Normal case, create one trail
                    CreateTrailSegment(horizontalX, verticalY, handleW * 0.5f);
                }
                */
                
                // Bring handle to the top layer after generation
                handleRect.transform.SetAsLastSibling();
                oppositeHandleRect.transform.SetAsLastSibling();
            }
        }
    }

    // Method for creating trail segments (commented out, kept for future use)
    /*
    private void CreateTrailSegment(float x, float y, float width)
    {
        // Instantiate a prefab to container
        GameObject go = Instantiate(trailPrefab, container);
        RectTransform rt = go.GetComponent<RectTransform>();

        // Reuse handle's anchors and pivot
        rt.anchorMin = handleRect.anchorMin;
        rt.anchorMax = handleRect.anchorMax;
        rt.pivot = handleRect.pivot;

        // Width = half of handleWidth, height same as handle
        rt.sizeDelta = new Vector2(width, fixedHandleSize * 0.5f);

        // Position aligned to handle's current position
        rt.anchoredPosition = new Vector2(x, y);
    }
    */
}
