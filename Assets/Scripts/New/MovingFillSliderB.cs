using UnityEngine;
using UnityEngine.UI;

public class MovingFillSliderB : MonoBehaviour
{
    // Associated Slider component
    public Slider slider;

    // Fill area RectTransform to move
    public RectTransform fillRect;

    // Fixed width (pixels), adjustable in Inspector as needed
    public float fixedFillWidth = 20f;

    // Internal use, save fillRect's parent container (usually Fill Area)
    private RectTransform container;

    void Start()
    {
        if (slider == null || fillRect == null)
        {
            Debug.LogError("Please ensure slider and fillRect are properly set");
            return;
        }

        // Get fillRect's parent container RectTransform
        container = fillRect.parent.GetComponent<RectTransform>();

        // Add Slider value change listener
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        // Initialize position
        OnSliderValueChanged(slider.value);
    }

    void OnSliderValueChanged(float value)
    {
        if (container == null)
            return;

        // Calculate container width
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
