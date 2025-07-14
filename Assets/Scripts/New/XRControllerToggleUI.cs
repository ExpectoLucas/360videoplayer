using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.IO;

[RequireComponent(typeof(XRController))]
public class XRControllerToggleUI : MonoBehaviour
{
    [Tooltip("要隐藏/显示的 UI 根节点")]
    [SerializeField] private GameObject playerUI;

    [Tooltip("监听哪个按钮来切换 UI，比如 Grip, Trigger, PrimaryButton(A), SecondaryButton(B) 等")]
    [SerializeField] private InputHelpers.Button toggleButton = InputHelpers.Button.Grip;

    [Tooltip("保存TrailData并更新热力图的按键")]
    [SerializeField] private InputHelpers.Button saveTrailDataButton = InputHelpers.Button.Grip;

    [Tooltip("按下程度阈值（一般小于 0.1 就算按下）")]
    [SerializeField] private float activationThreshold = 0.1f;

    [Tooltip("防抖时间间隔（秒）")]
    [SerializeField] private float debounceTime = 0.2f;

    private XRController xrController;
    private bool previousState = false;
    private bool previousSaveState = false;  // 用于跟踪保存按键的状态
    private CanvasGroup uiCanvasGroup;
    private float lastToggleTime = 0f;
    private float lastSaveTime = 0f;  // 用于防抖保存按键

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

        // 处理UI切换按键
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

        // 处理TrailData保存按键
        bool isSavePressed = false;
        InputHelpers.IsPressed(
            xrController.inputDevice,
            saveTrailDataButton,
            out isSavePressed,
            activationThreshold
        );

        // 边沿触发 + 防抖
        if (isSavePressed && !previousSaveState && Time.time - lastSaveTime > debounceTime)
        {
            lastSaveTime = Time.time;
            SaveTrailDataAndUpdateHeatmap();
        }
        previousSaveState = isSavePressed;
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

    /// <summary>
    /// 保存当前TrailData并更新热力图
    /// </summary>
    private void SaveTrailDataAndUpdateHeatmap()
    {
        Debug.Log("XRController: Saving trail data and updating heatmap...");

        // 1. 保存当前的TrailData并重新开始录制
        TrailDataManager trailDataManager = TrailDataManager.Instance;
        if (trailDataManager != null)
        {
            trailDataManager.SaveCurrentTrailDataAndRestart();
            Debug.Log("XRController: Trail data saved and recording restarted");
        }
        else
        {
            Debug.LogWarning("XRController: TrailDataManager instance not found");
        }

        // 2. 更新热力图
        UpdateHeatmap();
    }

    /// <summary>
    /// 更新热力图显示
    /// </summary>
    private void UpdateHeatmap()
    {
        // 获取视频名称和用户名
        VideoPlayerUIController controller = FindObjectOfType<VideoPlayerUIController>();
        if (controller != null)
        {
            string videoName = "";
            string userName = controller.userName;

            // 获取视频文件名
            if (!string.IsNullOrEmpty(controller.videoURL) && controller.videoURL != "null")
            {
                videoName = Path.GetFileNameWithoutExtension(controller.videoURL);
            }
            else
            {
                Debug.LogWarning("XRController: Could not get video filename for heatmap update");
                return;
            }

            if (string.IsNullOrEmpty(userName))
            {
                userName = "User";
                Debug.LogWarning("XRController: Username is empty, using default 'User'");
            }

            // 更新热力图
            HeatmapManager heatmapManager = HeatmapManager.Instance;
            if (heatmapManager != null)
            {
                heatmapManager.LoadAndDisplayHeatmap(videoName, userName);
                Debug.Log($"XRController: Heatmap updated for video: {videoName}, user: {userName}");
            }
            else
            {
                Debug.LogWarning("XRController: HeatmapManager instance not found");
            }
        }
        else
        {
            Debug.LogWarning("XRController: VideoPlayerUIController not found for heatmap update");
        }
    }
}
