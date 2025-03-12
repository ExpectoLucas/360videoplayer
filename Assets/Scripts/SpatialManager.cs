using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpatialManager : MonoBehaviour
{
    public GameObject radarObject;
    public GameObject circularCompassObject;
    public GameObject linearCompassObject;
    public GameObject nadirCompassObject;
    public GameObject compassCylinder;

    public Transform rigRot;
    public Transform camRot;
    public Transform controllerCamera;
    public RectTransform circularCompassPointer;
    public RectTransform radarCone;
    public RectTransform compassImage;
    private float rigRotFl;
    private float camRotFl;
    private float rotOffset;
    private float compPxPerDeg;
    private float pxNorthToNorth = 1800;
    private float compStartPosX;

    Renderer[] cylinderRenderers;

    public Text lengthField;

    public enum IndicatorType
    {
        RadarCone = 0,
        RelativeNorth = 1,
        CombinedStaticRadar = 2,
        CombinedStaticNorth = 3,
        LinearCompass = 4,
        NadirCompass = 5,
        CylinderCompass = 6

    };
    public IndicatorType indicatorType;  // Dropdown menu for picking visualisation type behaviour
    private int numberOfIndicatorTypes = System.Enum.GetValues(typeof(IndicatorType)).Length;

    private void Awake()
    {
        Time.fixedDeltaTime = 0.005f;
        //Debug.Log(Time.fixedDeltaTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        //Compass preparation
        compStartPosX = compassImage.localPosition.x;
        compPxPerDeg = pxNorthToNorth / 360f;

        //Enable requested indicator type
        SwitchType(indicatorType);

        circularCompassPointer.localPosition = new Vector3(0.3f, 0, 0);
        circularCompassPointer.pivot = new Vector2(3, 0.5f);
        circularCompassPointer.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        circularCompassPointer.GetComponent<Image>().color = Color.red;

        if(indicatorType == IndicatorType.CylinderCompass)
        {
            cylinderRenderers = compassCylinder.GetComponentsInChildren<Renderer>();    
        }
    }

    void SwitchType(IndicatorType type)
    {
        if (type == IndicatorType.RadarCone)
        {
            linearCompassObject.gameObject.SetActive(false);
            circularCompassObject.gameObject.SetActive(false);
            radarObject.gameObject.SetActive(true);
            nadirCompassObject.gameObject.SetActive(false);
            compassCylinder.gameObject.SetActive(false);

            //radarCone.localPosition = new Vector3(4.1f, 0, 0);
            //radarCone.pivot = new Vector2(1, 0.5f);
            //radarCone.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            //radarCone.GetComponent<Image>().color = Color.black;
        }
        else if (type == IndicatorType.RelativeNorth)
        {
            linearCompassObject.gameObject.SetActive(false);
            circularCompassObject.gameObject.SetActive(true);
            radarObject.gameObject.SetActive(false);
            nadirCompassObject.gameObject.SetActive(false);
            compassCylinder.gameObject.SetActive(false);
        }
        else if (type == IndicatorType.CombinedStaticNorth || type == IndicatorType.CombinedStaticRadar)
        {
            linearCompassObject.gameObject.SetActive(false);
            circularCompassObject.gameObject.SetActive(true);
            radarObject.gameObject.SetActive(true);
            nadirCompassObject.gameObject.SetActive(false);
            compassCylinder.gameObject.SetActive(false);

            radarCone.transform.localEulerAngles = (new Vector3(0, 0, 0));
            circularCompassPointer.transform.localEulerAngles = (new Vector3(0, 0, 0));

        }
        else if (type == IndicatorType.LinearCompass)
        {
            radarObject.gameObject.SetActive(false);
            circularCompassObject.gameObject.SetActive(false);
            linearCompassObject.gameObject.SetActive(true);
            nadirCompassObject.gameObject.SetActive(false);
            compassCylinder.gameObject.SetActive(false);
        }
        else if (type == IndicatorType.NadirCompass)
        {
            radarObject.gameObject.SetActive(false);
            circularCompassObject.gameObject.SetActive(false);
            linearCompassObject.gameObject.SetActive(false);
            nadirCompassObject.gameObject.SetActive(true);
            compassCylinder.gameObject.SetActive(false);

        }
        else if (type == IndicatorType.CylinderCompass)
        {
            radarObject.gameObject.SetActive(false);
            circularCompassObject.gameObject.SetActive(false);
            linearCompassObject.gameObject.SetActive(false);
            nadirCompassObject.gameObject.SetActive(false);
            compassCylinder.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rotOffset = camRot.rotation.eulerAngles.y;
        //lengthField.text = rotOffset.ToString();
        lengthField.text = ((int)indicatorType).ToString();
        //lengthField.text = numberOfIndicatorTypes.ToString();
        updateVis(indicatorType);
        /* 
         * Ideas:
         * Rotation = (current VR rig rotation + current camera rotation)%360 (both around vertical axis) INCORRECT! We're doing world readings, not local.
         * Rotation = current camera rotation ONLY. Being a child of the VR rig, its rotation propagates to its world reading!
         * North = 0deg, South=+-180d, 
         * To reset orientation, simply deduct (current VR rig rotation + current camera rotation) from current VR rig rotation, 
         *   keep camera orientation untouched.
         *   
         *   Camera rotation: Scene > VR Rig > VR Camera > Rotation > Y         (clamped to -180<=Y<180)
         *   VR Rig rotation: Scene > VR Rig > Rotation > Y                     (clamped to -180<=Y<180)
        */
    }

    public void ResetOrientation()
    {
        rigRot.transform.Rotate(new Vector3(0, -rotOffset, 0));
        camRot.transform.position = new Vector3(0, 0, 0);
    } 

    public void AdvanceIndicatorType()
    {
        indicatorType = (IndicatorType)(((int)indicatorType + 1)%numberOfIndicatorTypes);
        SwitchType(indicatorType);
    }

    public float getRotation()
    {
        return rotOffset;
    }

    private void updateVis(IndicatorType type)
    {
        if (type == IndicatorType.RadarCone || type == IndicatorType.CombinedStaticNorth)
        {
            //Vision cone
            radarCone.transform.localEulerAngles = (new Vector3(0, 0, -rotOffset));
            
            //Camera for controllers on radar:
            controllerCamera.position = new Vector3(camRot.position.x, 3.2f, camRot.position.z);
            controllerCamera.eulerAngles = new Vector3(90, rotOffset, 0);

        }
        else if (type == IndicatorType.RelativeNorth || type == IndicatorType.CombinedStaticRadar)
        {
            //Relative North arrow
            circularCompassPointer.transform.localEulerAngles = (new Vector3(0, 0, rotOffset));
        }
        else if (type == IndicatorType.LinearCompass)
        {
            //Linear compass
            compassImage.localPosition = new Vector3(compStartPosX-(rotOffset)%360*compPxPerDeg, 0, 0); 
        }
        else if (type == IndicatorType.NadirCompass)
        {
            //Nadir compass
            //Set position
            nadirCompassObject.GetComponent<Transform>().position = new Vector3(camRot.position.x, 0, camRot.position.z);
            //Set rotation
            nadirCompassObject.GetComponent<Transform>().eulerAngles = new Vector3(90, 0, 0);
        }
        else if (type == IndicatorType.CylinderCompass)
        {
            //Do transparency?
            //foreach (Renderer r in cylinderRenderers)
            //{
            //    r.sharedMaterial.color = new Color(r.sharedMaterial.color.r, r.sharedMaterial.color.g, r.sharedMaterial.color.b, 1);
                // r.sharedMaterial.shader. = new Color(r.sharedMaterial.color.r, r.sharedMaterial.color.g, r.sharedMaterial.color.b, 1);
                //Does not work :( Need to find another parameter to modify
            //}
            //gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_YourParameter", someValue);
        }
    }
}