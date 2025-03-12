using UnityEngine;
using UnityEngine.XR;

public class XRRenderScale : MonoBehaviour
{
    void Start()
    {
        XRSettings.eyeTextureResolutionScale = 1.5f;
    }
}