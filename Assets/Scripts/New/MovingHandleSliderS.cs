using UnityEngine;
using UnityEngine.UI;

public class MovingHandleSliderS : MonoBehaviour
{
    [Header("UI 与 摄像机设置")]
    public Slider slider;
    public Transform headTransform;
    public RectTransform container;
    [Tooltip("用于获取 FOV 的摄像机；若不填，会自动用 Camera.main")]
    public Camera vrCamera;

    [Header("红点基础设置")]
    [Tooltip("红点竖直方向（高度）固定像素")]
    public float fixedHandleSize = 20f;

    [Header("轨迹历史设置")]
    [Tooltip("UI Image Prefab，用于记录轨迹，每次生成一个条状片段")]
    public GameObject trailPrefab;
    [Tooltip("记录轨迹的时间间隔（秒），小于此间隔不再重复记录）")]
    public float recordInterval = 0.1f;

    private RectTransform handleRect;
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

        if (vrCamera == null)
            vrCamera = Camera.main;

        // 记录初始头部 yaw 作为零点
        float y = headTransform.rotation.eulerAngles.y;
        if (y > 180f) y -= 360f;
        initialYaw = y;
    }

    void LateUpdate()
    {
        // —— 1. 更新 handle 的 anchors/pivot/size/position —— 
        float containerW = container.rect.width;
        float containerH = container.rect.height;

        // 计算 handle 宽度 = containerW × (FOV / 360)
        float fov = vrCamera.fieldOfView;
        float handleW = containerW * (fov / 360f);

        handleRect.anchorMin = new Vector2(0.5f, 0f); // 底部中间
        handleRect.anchorMax = new Vector2(0.5f, 0f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(handleW, fixedHandleSize);

        // 垂直位置：slider 进度从底部往上
        float verticalY = slider.normalizedValue * containerH;

        // 水平位置：头部 yaw 循环映射
        float currentYaw = headTransform.rotation.eulerAngles.y;
        if (currentYaw > 180f) currentYaw -= 360f;
        float relYaw = currentYaw - initialYaw;
        relYaw = Mathf.Repeat(relYaw + 180f, 360f) - 180f;
        float normYaw = (relYaw + 180f) / 360f;
        // 锚点在中间，偏移 = (normYaw - 0.5) × 宽度
        float horizontalX = (normYaw - 0.5f) * containerW;

        handleRect.anchoredPosition = new Vector2(horizontalX, verticalY);

        // —— 2. 按间隔记录轨迹 —— 
        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            CreateTrailSegment(horizontalX, verticalY , handleW * 0.5f);
            // 生成后把 handle 提到最上层
            handleRect.transform.SetAsLastSibling();
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
