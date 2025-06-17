using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRController))]
public class XRControllerToggleUI : MonoBehaviour
{
    [Tooltip("要隐藏/显示的 UI 根节点")]
    [SerializeField] private GameObject playerUI;

    [Tooltip("监听哪个按钮来切换 UI，比如 Grip, Trigger, PrimaryButton(A), SecondaryButton(B) 等")]
    [SerializeField] private InputHelpers.Button toggleButton = InputHelpers.Button.Grip;

    [Tooltip("按下程度阈值（一般小于 0.1 就算按下）")]
    [SerializeField] private float activationThreshold = 0.1f;

    private XRController xrController;
    private bool previousState = false;

    void Awake()
    {
        xrController = GetComponent<XRController>();
        if (playerUI == null)
            Debug.LogWarning("XRControllerToggleUI: 没有指定 playerUI，切换功能不会生效。");
    }

    void Update()
    {
        if (playerUI == null || xrController == null)
            return;

        // 用 InputHelpers 检测是否按下了指定按钮
        bool isPressed = false;
        InputHelpers.IsPressed(
            xrController.inputDevice,
            toggleButton,
            out isPressed,
            activationThreshold
        );

        // 边沿触发：从未按下 → 刚按下
        if (isPressed && !previousState)
        {
            playerUI.SetActive(!playerUI.activeSelf);
        }
        previousState = isPressed;
    }
}
