using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

[RequireComponent(typeof(XRController))]
public class XRControllerToggleUI : MonoBehaviour
{
    [Tooltip("要隐藏/显示的 UI 根节点")]
    [SerializeField] private GameObject playerUI;

    [Tooltip("监听哪个按钮来切换 UI，比如 Grip, Trigger, PrimaryButton(A), SecondaryButton(B) 等")]
    [SerializeField] private InputHelpers.Button toggleButton = InputHelpers.Button.Grip;

    [Tooltip("按下程度阈值（一般小于 0.1 就算按下）")]
    [SerializeField] private float activationThreshold = 0.1f;

    [Tooltip("防抖时间间隔（秒）")]
    [SerializeField] private float debounceTime = 0.2f;

    private XRController xrController;
    private bool previousState = false;
    private CanvasGroup uiCanvasGroup;
    private float lastToggleTime = 0f;

    void Awake()
    {
        xrController = GetComponent<XRController>();
        if (playerUI == null)
        {
            Debug.LogWarning("XRControllerToggleUI: 没有指定 playerUI，切换功能不会生效。");
            return;
        }

        // 尝试获取CanvasGroup
        uiCanvasGroup = playerUI.GetComponent<CanvasGroup>();
        if (uiCanvasGroup == null)
        {
            uiCanvasGroup = playerUI.GetComponentInChildren<CanvasGroup>();
        }

        // 如果没有CanvasGroup，自动添加一个
        if (uiCanvasGroup == null)
        {
            uiCanvasGroup = playerUI.AddComponent<CanvasGroup>();
            Debug.Log("XRControllerToggleUI: 自动添加了CanvasGroup组件到 " + playerUI.name);
        }
    }

    void Update()
    {
        if (playerUI == null || xrController == null || uiCanvasGroup == null)
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
        bool isVisible = uiCanvasGroup.alpha > 0.5f;
        
        if (isVisible)
        {
            // 隐藏UI
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            // 显示UI
            uiCanvasGroup.alpha = 1f;
            uiCanvasGroup.interactable = true;
            uiCanvasGroup.blocksRaycasts = true;
        }
    }
}
