using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

public class ControllerFunctions : MonoBehaviour
{

    public Vector2 joystick;
    public float speed;
    public GameObject videoSphere;
    public XRController rightHand;
    public XRController leftHand;

    public InputDeviceCharacteristics controllerCharacteristics;


    // Start is called before the first frame update
    void Start()
    {
        //List<InputDevice> devices = new List<InputDevice>();
        //InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
