using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

[RequireComponent(typeof(XRController))]
public class XRControllerToggleUI : MonoBehaviour
{
    [Tooltip("要隐藏/显示的多个 UI 对象")]
    [SerializeField] private GameObject[] uiObjects;

    [Tooltip("运行时动态查找的UI对象名称列表")]
    [SerializeField] private string[] dynamicUINames = { "POI_43_V", "POI_174_V", "POI_37_V", "VerticalHeatmap"};

    [Tooltip("监听哪个按钮来切换 UI，比如 Grip, Trigger, PrimaryButton(A), SecondaryButton(B) 等")]
    [SerializeField] private InputHelpers.Button toggleButton = InputHelpers.Button.Grip;

    [Tooltip("按下程度阈值（一般小于 0.1 就算按下）")]
    [SerializeField] private float activationThreshold = 0.1f;

    [Tooltip("防抖时间间隔（秒）")]
    [SerializeField] private float debounceTime = 0.2f;

    private XRController xrController;
    private bool previousState = false;
    private CanvasGroup[] uiCanvasGroups;
    private System.Collections.Generic.List<CanvasGroup> dynamicUICanvasGroups;
    private System.Collections.Generic.List<GameObject> dynamicUIObjects;
    private float lastToggleTime = 0f;

    void Awake()
    {
        xrController = GetComponent<XRController>();
        
        // 处理预设置的UI对象
        if (uiObjects != null && uiObjects.Length > 0)
        {
            InitializeStaticUIObjects();
        }

        // 初始化动态UI列表
        if (dynamicUINames != null && dynamicUINames.Length > 0)
        {
            dynamicUIObjects = new System.Collections.Generic.List<GameObject>();
            dynamicUICanvasGroups = new System.Collections.Generic.List<CanvasGroup>();
        }
    }

    private void InitializeStaticUIObjects()
    {
        // 初始化CanvasGroup数组
        uiCanvasGroups = new CanvasGroup[uiObjects.Length];
        
        for (int i = 0; i < uiObjects.Length; i++)
        {
            if (uiObjects[i] == null)
            {
                Debug.LogWarning($"XRControllerToggleUI: UI对象索引{i}为空，跳过该对象。");
                continue;
            }

            // 尝试获取CanvasGroup
            CanvasGroup canvasGroup = uiObjects[i].GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = uiObjects[i].GetComponentInChildren<CanvasGroup>();
            }

            // 如果没有CanvasGroup，自动添加一个
            if (canvasGroup == null)
            {
                canvasGroup = uiObjects[i].AddComponent<CanvasGroup>();
                Debug.Log($"XRControllerToggleUI: 自动添加了CanvasGroup组件到 {uiObjects[i].name}");
            }
            
            uiCanvasGroups[i] = canvasGroup;
        }
    }

    void Update()
    {
        if (xrController == null)
            return;

        // 尝试查找动态UI对象
        FindDynamicUIObjects();

        bool isPressed = false;
        InputHelpers.IsPressed(
            xrController.inputDevice,
            toggleButton,
            out isPressed,
            activationThreshold
        );

        // 边沿触发 + 防抖
        if (isPressed && !previousState && Time.time - lastToggleTime > debounceTime)
        {
            lastToggleTime = Time.time;
            ToggleUI();
        }
        previousState = isPressed;
    }

    private void FindDynamicUIObjects()
    {
        if (dynamicUINames == null || dynamicUINames.Length == 0)
            return;

        for (int nameIndex = 0; nameIndex < dynamicUINames.Length; nameIndex++)
        {
            string uiName = dynamicUINames[nameIndex];
            if (string.IsNullOrEmpty(uiName))
                continue;

            // 查找所有同名的GameObject
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == uiName && !dynamicUIObjects.Contains(obj))
                {
                    dynamicUIObjects.Add(obj);
                    
                    // 为动态找到的对象设置CanvasGroup
                    CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = obj.GetComponentInChildren<CanvasGroup>();
                    }
                    if (canvasGroup == null)
                    {
                        canvasGroup = obj.AddComponent<CanvasGroup>();
                        Debug.Log($"XRControllerToggleUI: 自动添加了CanvasGroup组件到动态UI {obj.name}");
                    }
                    
                    dynamicUICanvasGroups.Add(canvasGroup);
                    Debug.Log($"XRControllerToggleUI: 找到动态UI对象 {obj.name} (实例ID: {obj.GetInstanceID()})");
                }
            }
        }
    }

    private void ToggleUI()
    {
        // 检查当前UI状态（优先检查静态UI，然后检查动态UI）
        bool isVisible = GetCurrentUIVisibility();
        
        // 切换静态UI对象的状态
        if (uiCanvasGroups != null)
        {
            for (int i = 0; i < uiCanvasGroups.Length; i++)
            {
                if (uiCanvasGroups[i] == null)
                    continue;

                SetUIVisibility(uiCanvasGroups[i], !isVisible);
            }
        }

        // 切换动态UI对象的状态
        if (dynamicUICanvasGroups != null)
        {
            for (int i = 0; i < dynamicUICanvasGroups.Count; i++)
            {
                if (dynamicUICanvasGroups[i] == null)
                    continue;

                SetUIVisibility(dynamicUICanvasGroups[i], !isVisible);
            }
        }
    }

    private bool GetCurrentUIVisibility()
    {
        // 先检查静态UI的状态
        if (uiCanvasGroups != null)
        {
            for (int i = 0; i < uiCanvasGroups.Length; i++)
            {
                if (uiCanvasGroups[i] != null)
                {
                    return uiCanvasGroups[i].alpha > 0.5f;
                }
            }
        }

        // 如果没有静态UI，检查动态UI的状态
        if (dynamicUICanvasGroups != null)
        {
            for (int i = 0; i < dynamicUICanvasGroups.Count; i++)
            {
                if (dynamicUICanvasGroups[i] != null)
                {
                    return dynamicUICanvasGroups[i].alpha > 0.5f;
                }
            }
        }

        return false; // 默认为隐藏状态
    }

    private void SetUIVisibility(CanvasGroup canvasGroup, bool visible)
    {
        if (visible)
        {
            // 显示UI
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            // 隐藏UI
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 手动刷新动态UI列表，可在运行时调用
    /// </summary>
    public void RefreshDynamicUIList()
    {
        if (dynamicUIObjects != null)
        {
            dynamicUIObjects.Clear();
            dynamicUICanvasGroups.Clear();
        }
        FindDynamicUIObjects();
        Debug.Log($"XRControllerToggleUI: 手动刷新完成，当前管理 {dynamicUIObjects.Count} 个动态UI对象");
    }
}
