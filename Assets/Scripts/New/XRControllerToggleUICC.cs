using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

[RequireComponent(typeof(XRController))]
public class XRControllerToggleUICC : MonoBehaviour
{
    [Tooltip("Multiple UI objects to hide/show")]
    [SerializeField] private GameObject[] uiObjects;

    [Tooltip("Always hidden UI objects (not affected by button toggle)")]
    [SerializeField] private GameObject[] alwaysHiddenUIObjects;

    [Tooltip("Button to listen for UI toggle, such as Grip, Trigger, PrimaryButton(A), SecondaryButton(B), etc.")]
    [SerializeField] private InputHelpers.Button toggleButton = InputHelpers.Button.Grip;

    [Tooltip("Press threshold (typically considered pressed when below 0.1)")]
    [SerializeField] private float activationThreshold = 0.1f;

    [Tooltip("Debounce time interval (seconds)")]
    [SerializeField] private float debounceTime = 0.2f;

    private XRController xrController;
    private bool previousState = false;
    private CanvasGroup[] uiCanvasGroups;
    private CanvasGroup[] alwaysHiddenCanvasGroups;
    private float lastToggleTime = 0f;

    void Awake()
    {
        xrController = GetComponent<XRController>();
        
        // Process preset UI objects
        if (uiObjects != null && uiObjects.Length > 0)
        {
            InitializeStaticUIObjects();
        }

        // Process always hidden UI objects
        if (alwaysHiddenUIObjects != null && alwaysHiddenUIObjects.Length > 0)
        {
            InitializeAlwaysHiddenUIObjects();
        }
    }

    private void InitializeStaticUIObjects()
    {
        // Initialize CanvasGroup array
        uiCanvasGroups = new CanvasGroup[uiObjects.Length];
        
        for (int i = 0; i < uiObjects.Length; i++)
        {
            if (uiObjects[i] == null)
            {
                Debug.LogWarning($"XRControllerToggleUI: UI object at index {i} is null, skipping this object.");
                continue;
            }

            // Try to get CanvasGroup
            CanvasGroup canvasGroup = uiObjects[i].GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = uiObjects[i].GetComponentInChildren<CanvasGroup>();
            }

            // If no CanvasGroup, add one automatically
            if (canvasGroup == null)
            {
                canvasGroup = uiObjects[i].AddComponent<CanvasGroup>();
                Debug.Log($"XRControllerToggleUI: Automatically added CanvasGroup component to {uiObjects[i].name}");
            }
            
            uiCanvasGroups[i] = canvasGroup;
        }
    }

    private void InitializeAlwaysHiddenUIObjects()
    {
        // Initialize CanvasGroup array for always hidden UI
        alwaysHiddenCanvasGroups = new CanvasGroup[alwaysHiddenUIObjects.Length];
        
        for (int i = 0; i < alwaysHiddenUIObjects.Length; i++)
        {
            if (alwaysHiddenUIObjects[i] == null)
            {
                Debug.LogWarning($"XRControllerToggleUI: Always hidden UI object at index {i} is null, skipping this object.");
                continue;
            }

            // Try to get CanvasGroup
            CanvasGroup canvasGroup = alwaysHiddenUIObjects[i].GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = alwaysHiddenUIObjects[i].GetComponentInChildren<CanvasGroup>();
            }

            // If no CanvasGroup, add one automatically
            if (canvasGroup == null)
            {
                canvasGroup = alwaysHiddenUIObjects[i].AddComponent<CanvasGroup>();
                Debug.Log($"XRControllerToggleUI: Automatically added CanvasGroup component to always hidden UI {alwaysHiddenUIObjects[i].name}");
            }
            
            alwaysHiddenCanvasGroups[i] = canvasGroup;
            
            // Immediately hide these UI objects
            SetUIVisibility(canvasGroup, false);
            Debug.Log($"XRControllerToggleUI: Always hidden UI object {alwaysHiddenUIObjects[i].name} has been set to hidden state");
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

        // Edge triggering + debouncing
        if (isPressed && !previousState && Time.time - lastToggleTime > debounceTime)
        {
            lastToggleTime = Time.time;
            ToggleUI();
        }
        previousState = isPressed;
    }

    private void ToggleUI()
    {
        // Check current UI state
        bool isVisible = GetCurrentUIVisibility();
        
        // Toggle static UI object state
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
        // Check static UI state
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

        return false; // Default to hidden state
    }

    private void SetUIVisibility(CanvasGroup canvasGroup, bool visible)
    {
        if (visible)
        {
            // Show UI
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            // Hide UI
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
