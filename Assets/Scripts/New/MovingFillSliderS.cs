using UnityEngine;
using UnityEngine.UI;

public class MovingFillSliderS : MonoBehaviour
{
    // Associated Slider component
    public Slider slider;

    // Fill area RectTransform to move (represents the vertical moving line)
    public RectTransform fillRect;

    // Fixed height (pixels), used to keep fillRect from stretching, occupying fixed height when moving vertically
    public float fixedFillHeight = 20f;

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

        // Calculate container height
        float containerHeight = container.rect.height;

        // Calculate target y coordinate:
        // When normalizedValue is 0, fillRect is at bottom of container (y = 0);
        // When normalizedValue is 1, fillRect is at top of container, its y coordinate is containerHeight - fixedFillHeight
        float targetY = slider.normalizedValue * (containerHeight - fixedFillHeight);

        // Fix fillRect height to avoid stretching
        Vector2 size = fillRect.sizeDelta;
        size.y = fixedFillHeight;
        fillRect.sizeDelta = size;

        // Update anchoredPosition's y coordinate (only modify y axis here, preserve original x axis value)
        Vector2 pos = fillRect.anchoredPosition;
        pos.y = targetY;
        fillRect.anchoredPosition = pos;
    }

    void OnDestroy()
    {
        // Remove listener to prevent memory leaks
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}