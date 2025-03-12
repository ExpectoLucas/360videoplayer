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
            // 获取头显当前Y轴旋转角度
            float headYRotation = vrCamera.eulerAngles.y;

            // UI的Z轴旋转使扇形正确指示头部朝向
            transform.localEulerAngles = new Vector3(0, 0, -headYRotation);

            UpdateFOV(cam.fieldOfView);
        }
    }
    

    void UpdateFOV(float fovAngle)
    {
        // FOV通常为90度，对应360度圆盘的0.25
        sector_img.fillAmount = fovAngle / 360f;
    }
}
