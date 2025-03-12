using UnityEngine;

public class DistanceCalculator : MonoBehaviour
{
    public Transform target; // assign the target object in the Inspector
    public Transform targetCam;
    //private Camera mainCamera;

    void Start()
    {
        //targetCam = Camera.main;
        float distance = Vector3.Distance(targetCam.transform.position, target.position);
        //Debug.Log("Distance: " + distance);
    }

}
