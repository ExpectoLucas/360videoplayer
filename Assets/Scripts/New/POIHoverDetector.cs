using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class POIHoverDetector : MonoBehaviour
{
    private string hintText;
    private HeatmapManager heatmapManager;
    private bool isHovering = false;
    
    [Header("VR Controller Settings")]
    public XRController leftController;
    public XRController rightController;
    public float raycastDistance = 10f; // Raycast detection distance
    
    private GraphicRaycaster graphicRaycaster;
    private Canvas canvas;
    private RectTransform rectTransform;
    
    public void Initialize(string hint, HeatmapManager manager)
    {
        hintText = hint;
        heatmapManager = manager;
        
        // Get necessary components
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }
        
        // Automatically find VR controllers
        if (leftController == null || rightController == null)
        {
            FindVRControllers();
        }
    }
    
    private void FindVRControllers()
    {
        XRController[] controllers = FindObjectsOfType<XRController>();
        foreach (var controller in controllers)
        {
            if (controller.controllerNode == XRNode.LeftHand)
            {
                leftController = controller;
            }
            else if (controller.controllerNode == XRNode.RightHand)
            {
                rightController = controller;
            }
        }
    }
    
    void Update()
    {
        CheckControllerHover();
    }
    
    private void CheckControllerHover()
    {
        bool currentlyHovering = false;
        Vector3 activeControllerPosition = Vector3.zero;
        
        // Check left controller ray
        if (leftController != null && CheckControllerRaycast(leftController, out Vector3 leftHitPoint))
        {
            currentlyHovering = true;
            activeControllerPosition = leftController.transform.position;
        }
        
        // Check right controller ray
        if (rightController != null && CheckControllerRaycast(rightController, out Vector3 rightHitPoint))
        {
            currentlyHovering = true;
            activeControllerPosition = rightController.transform.position;
        }
        
        // Handle hover state changes
        if (currentlyHovering && !isHovering)
        {
            // Start hovering
            isHovering = true;
            OnHoverEnter(activeControllerPosition);
        }
        else if (!currentlyHovering && isHovering)
        {
            // End hovering
            isHovering = false;
            OnHoverExit();
        }
    }
    
    private bool CheckControllerRaycast(XRController controller, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;
        
        if (controller == null || canvas == null || graphicRaycaster == null)
            return false;
        
        // Emit ray from controller position
        Ray ray = new Ray(controller.transform.position, controller.transform.forward);
        
        // Check if ray intersects with Canvas plane
        if (RaycastCanvas(ray, out Vector2 canvasPosition))
        {
            // Check if Canvas position is within POI point range
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, canvasPosition, canvas.worldCamera, out localPoint))
            {
                // Check if point is within POI rectangle range
                Rect rect = rectTransform.rect;
                if (rect.Contains(localPoint))
                {
                    hitPoint = controller.transform.position + controller.transform.forward * raycastDistance;
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private bool RaycastCanvas(Ray ray, out Vector2 canvasPosition)
    {
        canvasPosition = Vector2.zero;
        
        if (canvas == null) return false;
        
        // Get Canvas plane
        Plane canvasPlane = new Plane(-canvas.transform.forward, canvas.transform.position);
        
        // Check intersection point between ray and plane
        if (canvasPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            
            // Convert 3D point to Canvas 2D coordinates
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, hitPoint);
            canvasPosition = screenPoint;
            return true;
        }
        
        return false;
    }
    
    private void OnHoverEnter(Vector3 controllerPosition)
    {
        if (heatmapManager != null && !string.IsNullOrEmpty(hintText))
        {
            heatmapManager.ShowHint(hintText, transform.position);
            Debug.Log($"VR Controller hovering over POI - Showing hint: {hintText}");
        }
    }
    
    private void OnHoverExit()
    {
        if (heatmapManager != null)
        {
            heatmapManager.HideHint();
            Debug.Log("VR Controller hover exit - Hiding POI hint");
        }
    }
}
