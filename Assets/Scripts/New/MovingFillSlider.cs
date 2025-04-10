using UnityEngine;
using UnityEngine.UI;

public class MovingFillSlider : MonoBehaviour
{
    // 关联的 Slider 组件
    public Slider slider;

    // 需要移动的填充区域 RectTransform
    public RectTransform fillRect;

    // 固定宽度（像素），可根据需求在 Inspector 中调整
    public float fixedFillWidth = 20f;

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

        // 添加 Slider 值变化监听
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        // 初始化位置
        OnSliderValueChanged(slider.value);
    }

    void OnSliderValueChanged(float value)
    {
        if (container == null)
            return;

        // 计算 container 的宽度
        float containerWidth = container.rect.width;

        // 计算目标 x 坐标：
        // 当 normalizedValue 为 0 时，目标为 0；
        // 当 normalizedValue 为 1 时，目标为 (containerWidth - fixedFillWidth)
        float targetX = slider.normalizedValue * (containerWidth - fixedFillWidth);

        // 固定 fillRect 的宽度，避免拉伸
        Vector2 size = fillRect.sizeDelta;
        size.x = fixedFillWidth;
        fillRect.sizeDelta = size;

        // 更新 anchoredPosition 的 x 坐标
        Vector2 pos = fillRect.anchoredPosition;
        pos.x = targetX;
        fillRect.anchoredPosition = pos;
    }

    void OnDestroy()
    {
        // 注意在销毁时移除监听，防止内存泄漏
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}
