using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ViewSectorDirection : MonoBehaviour
{
    private Transform vrCamera;
    public Image sector_img;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        vrCamera = Camera.main.transform;
    }

    void Update()
    {
        if (vrCamera != null)
        {
            // Get current Y-axis rotation angle of headset
            float headYRotation = vrCamera.eulerAngles.y;

            // UI Z-axis rotation makes sector correctly indicate head orientation
            transform.localEulerAngles = new Vector3(0, 0, -headYRotation);

            UpdateFOV(cam.fieldOfView);
        }
    }
    

    void UpdateFOV(float fovAngle)
    {
        // FOV is usually 90 degrees, corresponding to 0.25 of a 360-degree circle
        sector_img.fillAmount = fovAngle / 360f;
    }
}
