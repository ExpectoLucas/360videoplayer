using UnityEngine;
using UnityEngine.UI;

public class MovingHandleSliderS : MonoBehaviour
{
    // 与 Slider 组件关联（红点拖动功能需保持 Slider.handleRect 不为空）
    public Slider slider;

    // 用于获取头部信息的 Transform（例如 VR 摄像机或主摄像机），用于计算左右旋转（yaw）
    public Transform headTransform;

    // 进度条容器（如 Fill Area 或 Slider 轨道），建议红点作为该容器的子物体
    public RectTransform container;

    // 固定的红点尺寸（单位像素），保证红点始终为正圆
    public float fixedHandleSize = 20f;

    // 内部缓存红点的 RectTransform（即 Slider.handleRect）
    private RectTransform handleRect;

    // 记录初始头部 yaw，用于作相对角度的计算
    private float initialYaw;

    void Start()
    {
        // 检查必要引用
        if (slider == null || headTransform == null || container == null)
        {
            Debug.LogError("请在 Inspector 中设置 slider、headTransform 和 container！");
            enabled = false;
            return;
        }

        handleRect = slider.handleRect;
        if (handleRect == null)
        {
            Debug.LogError("Slider 的 handleRect 为空，请在 Slider 组件上指定红色小圆点！");
            enabled = false;
            return;
        }

        // 建议将红点（handle）的父物体设置为 container，这样计算的局部坐标就正确
        if (handleRect.parent != container)
            Debug.LogWarning("建议将红点（handle）的父物体设置为 container，以便计算的坐标更准确！");

        // 记录初始头部 yaw（转换到 -180～180 范围）
        float initYaw = headTransform.rotation.eulerAngles.y;
        if (initYaw > 180f)
            initYaw -= 360f;
        initialYaw = initYaw;
    }

    void LateUpdate()
    {
        // ① 固定红点的 RectTransform 属性，
        // 将 anchors 固定为底部中间，即 (0.5, 0)；pivot 设为 (0.5, 0.5)
        handleRect.anchorMin = new Vector2(0.5f, 0f);
        handleRect.anchorMax = new Vector2(0.5f, 0f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(fixedHandleSize, fixedHandleSize);

        // ② 获取容器的尺寸
        float containerWidth = container.rect.width;
        float containerHeight = container.rect.height;

        // ③ 计算垂直位置：由 slider.normalizedValue 决定
        // 当 slider.normalizedValue 为 0，verticalY 为 0（容器底部）；为 1，verticalY 为 containerHeight（容器顶部）
        float verticalY = slider.normalizedValue * containerHeight;

        // ④ 计算水平位置：根据头部左右转向（yaw）循环移动
        // 获取当前 headTransform 的 yaw（转换到 -180～180 范围）
        float currentYaw = headTransform.rotation.eulerAngles.y;
        if (currentYaw > 180f)
            currentYaw -= 360f;
        // 计算与初始 yaw 的相对角度
        float relativeYaw = currentYaw - initialYaw;
        // 将 relativeYaw 包装到 [-180,180] 范围内
        relativeYaw = Mathf.Repeat(relativeYaw + 180f, 360f) - 180f;
        // 归一化：当 relativeYaw == 0 时映射为 0.5（容器中间），-180 映射到 0，+180 映射到 1
        float normalizedYaw = (relativeYaw + 180f) / 360f;
        // 由于锚点在容器中间，水平偏移 = (normalizedYaw - 0.5) * containerWidth
        float horizontalX = (normalizedYaw - 0.5f) * containerWidth;

        // ⑤ 合成最终位置，注意：此处的 anchoredPosition 是相对于容器底部中间
        handleRect.anchoredPosition = new Vector2(horizontalX, verticalY);
    }
}
