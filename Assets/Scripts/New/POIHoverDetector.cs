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
    
    [Header("VR手柄设置")]
    public XRController leftController;
    public XRController rightController;
    public float raycastDistance = 10f; // 射线检测距离
    
    private GraphicRaycaster graphicRaycaster;
    private Canvas canvas;
    private RectTransform rectTransform;
    
    public void Initialize(string hint, HeatmapManager manager)
    {
        hintText = hint;
        heatmapManager = manager;
        
        // 获取必要的组件
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }
        
        // 自动查找VR手柄
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
        
        // 检查左手柄射线
        if (leftController != null && CheckControllerRaycast(leftController, out Vector3 leftHitPoint))
        {
            currentlyHovering = true;
            activeControllerPosition = leftController.transform.position;
        }
        
        // 检查右手柄射线
        if (rightController != null && CheckControllerRaycast(rightController, out Vector3 rightHitPoint))
        {
            currentlyHovering = true;
            activeControllerPosition = rightController.transform.position;
        }
        
        // 处理悬浮状态变化
        if (currentlyHovering && !isHovering)
        {
            // 开始悬浮
            isHovering = true;
            OnHoverEnter(activeControllerPosition);
        }
        else if (!currentlyHovering && isHovering)
        {
            // 结束悬浮
            isHovering = false;
            OnHoverExit();
        }
    }
    
    private bool CheckControllerRaycast(XRController controller, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;
        
        if (controller == null || canvas == null || graphicRaycaster == null)
            return false;
        
        // 从手柄位置发射射线
        Ray ray = new Ray(controller.transform.position, controller.transform.forward);
        
        // 检查射线是否与Canvas平面相交
        if (RaycastCanvas(ray, out Vector2 canvasPosition))
        {
            // 检查Canvas位置是否在POI点的范围内
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, canvasPosition, canvas.worldCamera, out localPoint))
            {
                // 检查点是否在POI的矩形范围内
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
        
        // 获取Canvas的平面
        Plane canvasPlane = new Plane(-canvas.transform.forward, canvas.transform.position);
        
        // 检查射线与平面的交点
        if (canvasPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            
            // 将3D点转换为Canvas的2D坐标
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
