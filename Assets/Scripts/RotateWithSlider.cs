using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateWithSlider : MonoBehaviour
{

    public GameObject videoSphere;
    public Slider slider;

    // Preserve the original and current orientation
    private float previousValue;

    void Awake()
    {
        // Assign a callback for when this slider changes
        this.slider.onValueChanged.AddListener(this.OnRotationSliderChanged);

        this.previousValue = this.slider.value;
    }

    void OnRotationSliderChanged(float value)
    {
        float delta = value - this.previousValue;
        this.videoSphere.transform.Rotate(Vector3.up * delta * 360);

        this.previousValue = value;
    }
}
