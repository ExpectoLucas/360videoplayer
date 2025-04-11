using UnityEngine;
using UnityEngine.UI;

public class MovingFillSliderS : MonoBehaviour
{
    // 关联的 Slider 组件
    public Slider slider;

    // 需要移动的填充区域 RectTransform（代表那根竖直移动的线）
    public RectTransform fillRect;

    // 固定高度（像素），用于保持 fillRect 不拉伸，沿竖直方向移动时占据固定高度
    public float fixedFillHeight = 20f;

    // 内部使用，保存 fillRect 的父容器（通常是 Fill Area）
    private RectTransform container;

    void Start()
    {
        if (slider == null || fillRect == null)
        {
            Debug.LogError("请确保 slider 和 fillRect 已正确设置");
            return;
        }

        // 获取 fillRect 的父容器 RectTransform
        container = fillRect.parent.GetComponent<RectTransform>();

        // 添加 Slider 数值变化监听
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        // 初始化位置
        OnSliderValueChanged(slider.value);
    }

    void OnSliderValueChanged(float value)
    {
        if (container == null)
            return;

        // 计算 container 的高度
        float containerHeight = container.rect.height;

        // 计算目标 y 坐标：
        // 当 normalizedValue 为 0 时，fillRect 在容器底部（y = 0）；
        // 当 normalizedValue 为 1 时，fillRect 在容器顶部，其 y 坐标为 containerHeight - fixedFillHeight
        float targetY = slider.normalizedValue * (containerHeight - fixedFillHeight);

        // 固定 fillRect 的高度，避免拉伸
        Vector2 size = fillRect.sizeDelta;
        size.y = fixedFillHeight;
        fillRect.sizeDelta = size;

        // 更新 anchoredPosition 的 y 坐标（这里只修改 y 轴，x 轴保留原有值）
        Vector2 pos = fillRect.anchoredPosition;
        pos.y = targetY;
        fillRect.anchoredPosition = pos;
    }

    void OnDestroy()
    {
        // 移除监听，防止内存泄漏
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}