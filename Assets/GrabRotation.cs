using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabRotation : MonoBehaviour
{

    float rotationSpeed = 1f;
    public GameObject sphere;
    private Camera camera;

    private float screenWidth;
    private Vector3 startPoint;
    private Quaternion startRotation;

    private float _sensitivity;
    private Vector3 _mouseReference;
    private Vector3 _mouseOffset;
    private Vector3 _rotation;
    private bool _isRotating;
    // Start is called before the first frame update
    void Start()
    {
        _sensitivity = 0.4f;
        _rotation = Vector3.zero;
        screenWidth = Screen.width;
        camera = Camera.main;
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            startPoint = Input.mousePosition;
            startRotation = sphere.transform.rotation;
        }
        else if (Input.GetMouseButton(0))
        {
            float currentDistX = (Input.mousePosition - startPoint).x;
            float currentDistY = (Input.mousePosition - startPoint).y;
            //this.transform.Rotate(startRotation * Vector3.right * (currentDistY / screenWidth) * 360);
            //this.transform.Rotate(startRotation * Vector3.forward * (currentDistX / screenWidth) * 360);

            float xAxisRotation = Input.GetAxis("Mouse X") * rotationSpeed;
            float yAxisRotation = Input.GetAxis("Mouse Y") * rotationSpeed;

            //Vector3 relativeUp = camera.transform.TransformDirection(Vector3.up);
            //Vector3 relativeRight = camera.transform.TransformDirection(Vector3.right);

            sphere.transform.Rotate(Vector3.up * xAxisRotation, Space.World);
            sphere.transform.Rotate(Vector3.right * yAxisRotation, Space.World);
            //sphere.transform.rotation = Quaternion.Euler(sphere.transform.eulerAngles.x, sphere.transform.eulerAngles.y, 0);
            //sphere.transform.rotation = startRotation * Quaternion.Euler(Vector3.forward * (currentDistX / screenWidth) * 360);
        }
        else if (Input.GetKeyDown("r"))
        {
            sphere.transform.rotation = Quaternion.Euler(180, 0, 0);
        }
        //if (_isRotating)
        //{
        //    // offset
        //    _mouseOffset = (Input.mousePosition - _mouseReference);

        //    // apply rotation
        //    _rotation.y = -(_mouseOffset.x + _mouseOffset.y) * _sensitivity;

        //    // rotate
        //    cube.transform.Rotate(_rotation);

        //    // store mouse
        //    _mouseReference = Input.mousePosition;
        //}
    }

    void OnMouseDown()
    {
        // rotating flag
        _isRotating = true;

        // store mouse
        _mouseReference = Input.mousePosition;
        Debug.Log("MOuse Down");
    }

    void OnMouseUp()
    {
        // rotating flag
        Debug.Log("MOuse Up");
        _isRotating = false;
    }

    //private void OnMouseDrag()
    //{
    //    Debug.Log("MOuse Drag");
    //    float xAxisRotation = Input.GetAxis("Mouse X") * rotationSpeed;
    //    float yAxisRotation = Input.GetAxis("Mouse Y") * rotationSpeed;

    //    sphere.transform.Rotate(Vector3.forward, xAxisRotation, Space.World);
    //    sphere.transform.Rotate(Vector3.right, yAxisRotation, Space.World);
    //}
}
