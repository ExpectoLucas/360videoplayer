using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR;
using Debug = UnityEngine.Debug;

// Script to control and receive controller input.
public class HandPresence : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristics;
    public InputDeviceCharacteristics rightControllerCharacteristics;
    InputDeviceCharacteristics leftControllerCharacteristics;
    //public List<GameObject> controllerPrefabs;
    //public GameObject handModelPrefab;
    public List<InputDevice> rightHandList;
    public List<InputDevice> devices;
    public GameObject videoSphere;

    public VideoPlayer videoPlayer;

    private Vector2 rightThumbstickValue;

    private Material sphereMaterial;
    private Material videoMaterial;
    private Material imageMaterial;

    private InputDevice rightHandController;
    private InputDevice leftHandController;
    private float timer = 1.0f;
    private bool scrubTime = false;
    private List<Texture> frames = new List<Texture>();

    // Start is called before the first frame update
    void Start()
    {
        rightHandList = new List<InputDevice>();
        devices = new List<InputDevice>();

        rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;

        //InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
        //InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            rightHandController = devices[0];
            leftHandController = devices[1];
        }
    }

    private void Awake()
    {

    }


    // Update is called once per frame
    void Update()
    {
        if (videoPlayer && videoSphere)
        {
            //Debug.Log(videoPlayer.frameCount);
            if (frames.Count == 0)
            {
                videoPlayer.Pause();
                for (int i = 0; i < (int)videoPlayer.frameCount; i++)
                {
                    videoPlayer.frame = i;
                    frames.Add((Texture2D)videoPlayer.texture);
                }
                videoPlayer.frame = 0;
                videoPlayer.Play();
            }

            //if (scrubTime)
            //{
            //    if (timer < 0.8f)
            //    {
            //        MoveTimeSliderWithFrames(rightThumbstickValue);
            //        timer = 1.0f;
            //    }
            //    timer -= Time.deltaTime;
            //}

            if (devices.Count > 0)
            {

                rightHandController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisValue);
                rightThumbstickValue = primary2DAxisValue;

                // 遥感旋转视角功能已禁用 - 注释掉以下代码以禁用遥感控制视角旋转
                /*
                if (primary2DAxisValue.x != 0)
                {
                    //Debug.Log("primary touchpad: " + primary2DAxisValue);
                    RotateSphere(rightThumbstickValue);
                }
                */

                // 遥感快进功能已禁用 - 注释掉以下代码以禁用遥感控制视频快进
                /*
                if (primary2DAxisValue.y != 0 && timer < 0.8f)
                {
                    MoveTimeSliderWithFrames(rightThumbstickValue);
                    timer = 1.0f;
                    timer -= Time.deltaTime;
                }
                else
                {
                    timer -= Time.deltaTime;
                    //scrubTime = false;
                    //videoPlayer.Play();
                    //UIController.playing = true;
                }
                */


                // Add a timer to make the timeline respond slower, in order for the video player to catch up and be mroe responsive
                //if (primary2DAxisValue.y != 0 && timer < 0.8f)
                //{
                //    //Debug.Log(timer);
                //    MoveTimeSlider(rightThumbstickValue);
                //    timer = 1.0f;
                //}
                //timer -= Time.deltaTime;
            }
            else
            {
                InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
                if (devices.Count > 0)
                {
                    try
                    {
                        rightHandController = devices[0];
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
            }
        }
    }

    void RotateSphere(Vector2 value)
    {
        // 遥感旋转视角功能已被禁用
        // 原始代码已注释，如需重新启用请取消注释
        /*
        float delta = value.x;
        this.videoSphere.transform.Rotate(Vector3.up * delta * 360 * 0.025f);
        */
    }

    void MoveTimeSlider(Vector2 value)
    {
        // 遥感时间控制功能已被禁用
        // 原始代码已注释，如需重新启用请取消注释
        /*
        float delta = value.y;
        videoPlayer.time += delta * 5;
        */
    }

    void MoveTimeSliderWithFrames(Vector2 value)
    {
        // 遥感快进功能已被禁用
        // 原始代码已注释，如需重新启用请取消注释
        /*
        float delta = value.y;
        videoPlayer.frame += (int)(delta * 100);
        videoSphere.GetComponent<MeshRenderer>().material.SetTexture("test", frames[(int)videoPlayer.frame]);
        */
    }

}