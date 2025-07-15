using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;  // 用于 IPointerDownHandler / IPointerUpHandler
using UnityEngine.Video;  // 添加 Video 命名空间

public class MovingHandleSliderB : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI 与 摄像机设置")]
    public Slider slider;
    public Transform headTransform;
    public RectTransform container;
    [Tooltip("用于获取垂直 FOV 的摄像机；如果不填，会自动用 Camera.main")]
    public Camera vrCamera;
    [Tooltip("视频播放器，用于检查播放状态")]
    public VideoPlayer videoPlayer;

    [Header("红点基础设置")]
    [Tooltip("红点横向（宽度）固定像素")]
    public float fixedHandleWidth = 10f;

    [Header("轨迹历史设置")]
    // [Tooltip("用于记录轨迹的预制体，需要是一个 UI Image Prefab")]
    // public GameObject trailPrefab;
    [Tooltip("记录轨迹的时间间隔（秒），低于此间隔不再重复记录")]
    public float recordInterval = 0.1f;

    private RectTransform handleRect;
    private float recordTimer = 0f;
    private bool isDragging = false;

    void Start()
    {
        if (slider == null || headTransform == null || container == null)
        {
            Debug.LogError("请在 Inspector 中设置 slider、headTransform 和 container！");
            enabled = false;
            return;
        }

        handleRect = slider.handleRect;
        if (handleRect == null)
        {
            Debug.LogError("Slider.handleRect 为空，请在 Slider 上指定红点 RectTransform！");
            enabled = false;
            return;
        }

        if (vrCamera == null)
            vrCamera = Camera.main;

        // 初始化 TrailDataManager
        if (videoPlayer != null)
        {
            TrailDataManager.Instance.Initialize(videoPlayer);
        }
        
        // 设置热力图容器的垂直容器引用
        if (HeatmapManager.Instance != null)
        {
            HeatmapManager.Instance.verticalContainer = container;
        }
    }

    void LateUpdate()
    {
        // 1. 更新 handle 的锚点/尺寸/位置 (使用left-stretch模式)
        handleRect.anchorMin = new Vector2(0f, 0f);
        handleRect.anchorMax = new Vector2(0f, 1f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);

        float w = container.rect.width;
        float h = container.rect.height;
        float verticalFov = vrCamera != null ? vrCamera.fieldOfView : 60f;
        float handleH = h * (verticalFov / 180f);

        // 在left-stretch模式下，sizeDelta.y表示相对于stretch高度的偏移
        handleRect.sizeDelta = new Vector2(fixedHandleWidth, -h + handleH);

        float x = slider.normalizedValue * w;
        float pitch = headTransform.rotation.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        float y = -(pitch / 90f) * (h / 2f);

        handleRect.anchoredPosition = new Vector2(x, y);

        // 2. 只有当不在拖拽中且视频正在播放时，才累加计时器并记录轨迹
        if (!isDragging && videoPlayer != null && videoPlayer.isPlaying)
        {
            recordTimer += Time.deltaTime;
            if (recordTimer >= recordInterval)
            {
                recordTimer = 0f;

                // 计算当前俯仰角度（相对于水平点）
                pitch = headTransform.rotation.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                // 限制在 -90 到 90 度范围内
                pitch = Mathf.Clamp(pitch, -90f, 90f);

                // 记录轨迹点
                TrailDataManager.Instance.AddTrailPoint(
                    (float)videoPlayer.time,
                    -pitch,
                    false  // isHorizontal = false for B slider
                );

                // 创建轨迹段（已注释，保留以备后用）
                // CreateTrailSegment(x, y, handleH * 0.5f);

                // **关键：生成完轨迹后，把 handle 放到最后**
                handleRect.transform.SetAsLastSibling();
            }
        }
    }

    // 当用户在 Slider 上按下（开始拖拽）时调用
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        recordTimer = 0f; // 拖拽开始时重置计时器
    }

    // 当用户松开鼠标或手柄时调用
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        recordTimer = 0f; // 结束拖拽后也清零，保证间隔从 0 开始
    }

    // 创建轨迹段的方法（已注释，保留以备后用）
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
