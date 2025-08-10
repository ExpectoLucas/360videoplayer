using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.IO;

[RequireComponent(typeof(XRController))]
public class XRControllerToggleUI : MonoBehaviour
{
    [Tooltip("UI root node to hide/show")]
    [SerializeField] private GameObject playerUI;

    [Tooltip("Button to listen for UI toggle, such as Grip, Trigger, PrimaryButton(A), SecondaryButton(B), etc.")]
    [SerializeField] private InputHelpers.Button toggleButton = InputHelpers.Button.Grip;

    [Tooltip("Button to save TrailData and update the heatmap")]
    [SerializeField] private InputHelpers.Button saveTrailDataButton = InputHelpers.Button.Grip;

    [Tooltip("Press threshold (typically considered pressed when below 0.1)")]
    [SerializeField] private float activationThreshold = 0.1f;

    [Tooltip("Debounce time interval (seconds)")]
    [SerializeField] private float debounceTime = 0.2f;

    private XRController xrController;
    private bool previousState = false;
    private bool previousSaveState = false;  // Used to track the save button state
    private CanvasGroup uiCanvasGroup;
    private float lastToggleTime = 0f;
    private float lastSaveTime = 0f;  // Used for save button debouncing

    void Awake()
    {
        xrController = GetComponent<XRController>();
        if (playerUI == null)
        {
            Debug.LogWarning("XRControllerToggleUI: No playerUI specified, toggle functionality will not work.");
            return;
        }

        // Try to get CanvasGroup
        uiCanvasGroup = playerUI.GetComponent<CanvasGroup>();
        if (uiCanvasGroup == null)
        {
            uiCanvasGroup = playerUI.GetComponentInChildren<CanvasGroup>();
        }

        // If no CanvasGroup, add one automatically
        if (uiCanvasGroup == null)
        {
            uiCanvasGroup = playerUI.AddComponent<CanvasGroup>();
            Debug.Log("XRControllerToggleUI: Automatically added CanvasGroup component to " + playerUI.name);
        }
    }

    void Update()
    {
        if (playerUI == null || xrController == null || uiCanvasGroup == null)
            return;

        // Handle UI toggle button
        bool isPressed = false;
        InputHelpers.IsPressed(
            xrController.inputDevice,
            toggleButton,
            out isPressed,
            activationThreshold
        );

        // Edge triggering + debouncing
        if (isPressed && !previousState && Time.time - lastToggleTime > debounceTime)
        {
            lastToggleTime = Time.time;
            ToggleUI();
        }
        previousState = isPressed;

        // Handle TrailData save button
        bool isSavePressed = false;
        InputHelpers.IsPressed(
            xrController.inputDevice,
            saveTrailDataButton,
            out isSavePressed,
            activationThreshold
        );

        // Edge triggering + debouncing
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
            // Hide UI
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            // Show UI
            uiCanvasGroup.alpha = 1f;
            uiCanvasGroup.interactable = true;
            uiCanvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// Save current TrailData and update heatmap
    /// </summary>
    private void SaveTrailDataAndUpdateHeatmap()
    {
        Debug.Log("XRController: Saving trail data and updating heatmap...");

        // 1. Save current TrailData and restart recording
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

        // 2. Update heatmap
        UpdateHeatmap();
    }

    /// <summary>
    /// Update heatmap display
    /// </summary>
    private void UpdateHeatmap()
    {
        // Get video name and user name
        VideoPlayerUIController controller = FindObjectOfType<VideoPlayerUIController>();
        if (controller != null)
        {
            string videoName = "";
            string userName = controller.userName;

            // Get video file name
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

            // Update heatmap
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
