using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationButtons : MonoBehaviour
{
    public GameObject videoSphere;
    private bool rotating;
    private int direction;
    private float rotationSpeed;
    private float initialSpeed;
    private float timer = 0;

    public UITransform uiTransform;

    // Start is called before the first frame update
    void Start()
    {
        rotating = false;
        initialSpeed = 120;
        rotationSpeed = initialSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (rotating)
        {
            Rotate();
            timer += Time.deltaTime;
            if (timer > 0.5 && rotationSpeed < 450)
            {
                IncreaseSpeed();
                timer = 0;
            }
        }
    }

    private void Rotate()
    {
        videoSphere.transform.Rotate(Vector3.up * direction * rotationSpeed * Time.deltaTime);
    }

    public void LeftButton()
    {
        rotating = true;
        direction = -1;
        uiTransform.enabled = false;
    }

    public void RightButton()
    {
        rotating = true;
        direction = 1;
        uiTransform.enabled = false;

    }

    public void StopRotation()
    {
        rotating = false;
        rotationSpeed = initialSpeed;
        uiTransform.enabled = true;
    }

    private void IncreaseSpeed()
    {
        rotationSpeed *= 1.7f;
    }
}
