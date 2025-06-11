using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MovingHandleSliderS : MonoBehaviour
{
    [Header("UI 与 摄像机设置")]
    public Slider slider;
    public Transform headTransform;
    public RectTransform container;
    [Tooltip("用于获取 FOV 的摄像机；若不填，会自动用 Camera.main")]
    public Camera vrCamera;
    [Tooltip("视频播放器，用于检查播放状态")]
    public VideoPlayer videoPlayer;

    [Header("红点基础设置")]
    [Tooltip("红点竖直方向（高度）固定像素")]
    public float fixedHandleSize = 20f;

    [Header("轨迹历史设置")]
    [Tooltip("UI Image Prefab，用于记录轨迹，每次生成一个条状片段")]
    public GameObject trailPrefab;
    [Tooltip("记录轨迹的时间间隔（秒），小于此间隔不再重复记录）")]
    public float recordInterval = 0.1f;

    private RectTransform handleRect;
    private RectTransform oppositeHandleRect;  // 用于边界跨越时的额外handle
    private float recordTimer = 0f;
    private float initialYaw;

    void Start()
    {
        if (slider == null || headTransform == null || container == null || trailPrefab == null)
        {
            Debug.LogError("请在 Inspector 中设置 slider、headTransform、container 和 trailPrefab！");
            enabled = false;
            return;
        }

        handleRect = slider.handleRect;
        if (handleRect == null)
        {
            Debug.LogError("Slider.handleRect 为空，请在 Slider 组件上指定红点 RectTransform！");
            enabled = false;
            return;
        }

        // 创建用于边界跨越的额外handle
        CreateBoundaryHandles();

        if (vrCamera == null)
            vrCamera = Camera.main;

        // 记录初始头部 yaw 作为零点
        float y = headTransform.rotation.eulerAngles.y;
        if (y > 180f) y -= 360f;
        initialYaw = y;
    }

    private void CreateBoundaryHandles()
    {
        // 创建用于边界跨越的额外handle
        GameObject oppositeHandle = Instantiate(handleRect.gameObject, handleRect.parent);
        oppositeHandle.name = "OppositeHandle";
        oppositeHandleRect = oppositeHandle.GetComponent<RectTransform>();
        oppositeHandleRect.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        // —— 1. 更新 handle 的 anchors/pivot/size/position —— 
        float containerW = container.rect.width;
        float containerH = container.rect.height;

        // 计算 handle 宽度 = containerW × (FOV / 360)
        float fov = vrCamera.fieldOfView;
        float handleW = containerW * (fov / 360f);

        // 设置所有handle的基础属性
        handleRect.anchorMin = new Vector2(0.5f, 0f);
        handleRect.anchorMax = new Vector2(0.5f, 0f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(handleW, fixedHandleSize);
        oppositeHandleRect.anchorMin = handleRect.anchorMin;
        oppositeHandleRect.anchorMax = handleRect.anchorMax;
        oppositeHandleRect.pivot = handleRect.pivot;

        // 垂直位置：slider 进度从底部往上
        float verticalY = slider.normalizedValue * containerH;

        // 水平位置：头部 yaw 循环映射
        float currentYaw = headTransform.rotation.eulerAngles.y;
        if (currentYaw > 180f) currentYaw -= 360f;
        float relYaw = currentYaw - initialYaw;
        relYaw = Mathf.Repeat(relYaw + 180f, 360f) - 180f;
        float normYaw = (relYaw + 180f) / 360f;
        float horizontalX = (normYaw - 0.5f) * containerW;

        // 计算handle是否跨越边界
        float handleHalfWidth = handleW * 0.5f;
        float leftEdge  = container.rect.xMin;   // 等价于本地坐标的最左侧
        float rightEdge = container.rect.xMax;   // 等价于本地坐标的最右侧

        float handleLeftEdge = horizontalX - handleHalfWidth;
        float handleRightEdge = horizontalX + handleHalfWidth;

        if (handleLeftEdge < leftEdge)
        {
            // 计算跨越左边界的部分
            float overflow = leftEdge - handleLeftEdge;
            float mainHandleWidth = handleW - overflow;
            float oppositeHandleWidth = overflow;

            // 设置主handle
            handleRect.sizeDelta = new Vector2(mainHandleWidth, fixedHandleSize);
            handleRect.anchoredPosition = new Vector2(leftEdge + mainHandleWidth * 0.5f, verticalY);

            // 设置对侧handle（在右边）
            oppositeHandleRect.sizeDelta = new Vector2(oppositeHandleWidth, fixedHandleSize);
            oppositeHandleRect.anchoredPosition = new Vector2(rightEdge - oppositeHandleWidth * 0.5f, verticalY);
            oppositeHandleRect.gameObject.SetActive(true);
        }
        else if (handleRightEdge > rightEdge)
        {
            // 计算跨越右边界的部分
            float overflow = handleRightEdge - rightEdge;
            float mainHandleWidth = handleW - overflow;
            float oppositeHandleWidth = overflow;

            // 设置主handle
            handleRect.sizeDelta = new Vector2(mainHandleWidth, fixedHandleSize);
            handleRect.anchoredPosition = new Vector2(rightEdge - mainHandleWidth * 0.5f, verticalY);

            // 设置对侧handle（在左边）
            oppositeHandleRect.sizeDelta = new Vector2(oppositeHandleWidth, fixedHandleSize);
            oppositeHandleRect.anchoredPosition = new Vector2(leftEdge + oppositeHandleWidth * 0.5f, verticalY);
            oppositeHandleRect.gameObject.SetActive(true);
        }
        else
        {
            // 正常情况，只显示主handle
            handleRect.sizeDelta = new Vector2(handleW, fixedHandleSize);
            handleRect.anchoredPosition = new Vector2(horizontalX, verticalY);
            oppositeHandleRect.gameObject.SetActive(false);
        }

        // —— 2. 按间隔记录轨迹 —— 
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            recordTimer += Time.deltaTime;
            if (recordTimer >= recordInterval)
            {
                recordTimer = 0f;
                
                // 根据handle的状态创建轨迹
                if (horizontalX - handleHalfWidth < leftEdge)
                {
                    // 跨越左边界时，创建两个轨迹
                    CreateTrailSegment(leftEdge + handleRect.sizeDelta.x * 0.5f, verticalY, handleRect.sizeDelta.x * 0.5f);
                    CreateTrailSegment(rightEdge - oppositeHandleRect.sizeDelta.x * 0.5f, verticalY, oppositeHandleRect.sizeDelta.x * 0.5f);
                }
                else if (horizontalX + handleHalfWidth > rightEdge)
                {
                    // 跨越右边界时，创建两个轨迹
                    CreateTrailSegment(rightEdge - handleRect.sizeDelta.x * 0.5f, verticalY, handleRect.sizeDelta.x * 0.5f);
                    CreateTrailSegment(leftEdge + oppositeHandleRect.sizeDelta.x * 0.5f, verticalY, oppositeHandleRect.sizeDelta.x * 0.5f);
                }
                else
                {
                    // 正常情况，创建一个轨迹
                    CreateTrailSegment(horizontalX, verticalY, handleW * 0.5f);
                }

                // 生成后把 handle 提到最上层
                handleRect.transform.SetAsLastSibling();
                oppositeHandleRect.transform.SetAsLastSibling();
            }
        }
    }

    private void CreateTrailSegment(float x, float y, float width)
    {
        // 实例化一个 prefab 到 container
        GameObject go = Instantiate(trailPrefab, container);
        RectTransform rt = go.GetComponent<RectTransform>();

        // 复用 handle 的 anchors 和 pivot
        rt.anchorMin = handleRect.anchorMin;
        rt.anchorMax = handleRect.anchorMax;
        rt.pivot = handleRect.pivot;

        // 宽度 = handleWidth 的一半，高度与 handle 相同
        rt.sizeDelta = new Vector2(width, fixedHandleSize * 0.5f);

        // 位置对齐到 handle 当时的位置
        rt.anchoredPosition = new Vector2(x, y);
    }
}
