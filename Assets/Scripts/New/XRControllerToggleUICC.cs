using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

[RequireComponent(typeof(XRController))]
public class XRControllerToggleUICC : MonoBehaviour
{
    [Tooltip("要隐藏/显示的多个 UI 对象")]
    [SerializeField] private GameObject[] uiObjects;

    [Tooltip("永远隐藏的 UI 对象（不受按钮切换影响）")]
    [SerializeField] private GameObject[] alwaysHiddenUIObjects;

    [Tooltip("监听哪个按钮来切换 UI，比如 Grip, Trigger, PrimaryButton(A), SecondaryButton(B) 等")]
    [SerializeField] private InputHelpers.Button toggleButton = InputHelpers.Button.Grip;

    [Tooltip("按下程度阈值（一般小于 0.1 就算按下）")]
    [SerializeField] private float activationThreshold = 0.1f;

    [Tooltip("防抖时间间隔（秒）")]
    [SerializeField] private float debounceTime = 0.2f;

    private XRController xrController;
    private bool previousState = false;
    private CanvasGroup[] uiCanvasGroups;
    private CanvasGroup[] alwaysHiddenCanvasGroups;
    private float lastToggleTime = 0f;

    void Awake()
    {
        xrController = GetComponent<XRController>();
        
        // 处理预设置的UI对象
        if (uiObjects != null && uiObjects.Length > 0)
        {
            InitializeStaticUIObjects();
        }

        // 处理永远隐藏的UI对象
        if (alwaysHiddenUIObjects != null && alwaysHiddenUIObjects.Length > 0)
        {
            InitializeAlwaysHiddenUIObjects();
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

    private void InitializeAlwaysHiddenUIObjects()
    {
        // 初始化永远隐藏UI的CanvasGroup数组
        alwaysHiddenCanvasGroups = new CanvasGroup[alwaysHiddenUIObjects.Length];
        
        for (int i = 0; i < alwaysHiddenUIObjects.Length; i++)
        {
            if (alwaysHiddenUIObjects[i] == null)
            {
                Debug.LogWarning($"XRControllerToggleUI: 永远隐藏UI对象索引{i}为空，跳过该对象。");
                continue;
            }

            // 尝试获取CanvasGroup
            CanvasGroup canvasGroup = alwaysHiddenUIObjects[i].GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = alwaysHiddenUIObjects[i].GetComponentInChildren<CanvasGroup>();
            }

            // 如果没有CanvasGroup，自动添加一个
            if (canvasGroup == null)
            {
                canvasGroup = alwaysHiddenUIObjects[i].AddComponent<CanvasGroup>();
                Debug.Log($"XRControllerToggleUI: 自动添加了CanvasGroup组件到永远隐藏UI {alwaysHiddenUIObjects[i].name}");
            }
            
            alwaysHiddenCanvasGroups[i] = canvasGroup;
            
            // 立即隐藏这些UI对象
            SetUIVisibility(canvasGroup, false);
            Debug.Log($"XRControllerToggleUI: 永远隐藏UI对象 {alwaysHiddenUIObjects[i].name} 已设置为隐藏状态");
        }
    }

    void Update()
    {
        if (xrController == null)
            return;

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

    private void ToggleUI()
    {
        // 检查当前UI状态
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
    }

    private bool GetCurrentUIVisibility()
    {
        // 检查静态UI的状态
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
}
