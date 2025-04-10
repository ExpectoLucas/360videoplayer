using UnityEngine;
using UnityEngine.UI;

public class MovingHandleSlider : MonoBehaviour
{
    // 与 Slider 组件关联（确保 Handle Rect 指向红色小圆点，否则拖动会失效）
    public Slider slider;

    // 用于获取头部俯仰角（例如 VR 摄像机或主摄像机的 Transform）
    public Transform headTransform;

    // 进度条容器（用于计算水平范围和垂直范围），例如 Fill Area 或 Slider 的轨道
    public RectTransform container;

    // 固定的 Handle 尺寸（单位像素），用于保持红点形状不变
    public float fixedHandleSize = 20f;

    // 内部缓存 Slider 的 Handle RectTransform
    private RectTransform handleRect;

    void Start()
    {
        // 检查必要的引用
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
    }

    void LateUpdate()
    {
        // ① 将 Handle 的 anchors 固定在容器左侧中间，确保定位参考点正确
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(fixedHandleSize, fixedHandleSize);

        // ② 计算横向位置：以 container 的宽度为范围，
        // 当 slider.normalizedValue 为 0 时，x = 0（即容器左边缘），为 1 时 x = container.width
        float containerWidth = container.rect.width;
        float horizontalX = slider.normalizedValue * containerWidth;

        // ③ 计算垂直位置：根据 headTransform 的俯仰角计算
        // 取 headTransform 的 x 轴旋转角（注意 Unity 中 eulerAngles.x 范围是 0~360，要转换到 -180~180）
        float pitch = headTransform.rotation.eulerAngles.x;
        if (pitch > 180f)
            pitch -= 360f;
        // 设定映射：当 pitch = 0（水平看）时，红点在中间，偏移 0；
        // 当 pitch = -90（向上看 90°）时，偏移 +container.height/2（上移）；
        // 当 pitch = 90（向下看 90°）时，偏移 -container.height/2（下移）。
        float containerHeight = container.rect.height;
        float verticalOffset = -(pitch / 90f) * (containerHeight / 2f);

        // ④ 合成最终位置：横坐标由 Slider 进度决定，纵坐标由头部俯仰计算
        handleRect.anchoredPosition = new Vector2(horizontalX, verticalOffset);
    }
}
