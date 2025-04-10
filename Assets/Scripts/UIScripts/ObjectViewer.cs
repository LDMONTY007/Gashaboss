using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for using TextMeshPro UI

/// <summary>
/// ObjectViewer component that handles displaying 3D models for inspection.
/// This component allows players to view and rotate 3D models with automatic camera positioning.
/// </summary>
public class ObjectViewer : UIInputHandler
{
    // Singleton pattern to access this component from other scripts
    public static ObjectViewer instance;

    //----------------------------------------
    // UI REFERENCES
    //----------------------------------------
    [Header("UI Elements")]
    public CanvasGroup objectViewerPanel;      // Main panel that contains all UI elements
    public TextMeshProUGUI objectNameText;     // Text field to display object name
    public Button closeButton;                 // Button to close the viewer

    //----------------------------------------
    // 3D MODEL VIEWING SETTINGS
    //----------------------------------------
    [Header("3D Model Holder")]
    public Transform modelHolder;              // Parent transform where 3D models will be instantiated
    public Camera viewerCamera;                // Camera used to view the 3D model

    //----------------------------------------
    // CAMERA CONFIGURATION
    //----------------------------------------
    [Header("Camera Settings")]
    [Tooltip("Default camera distance if bounds calculation fails")]
    public float defaultDistance = 1.5f;       // Fallback distance if auto-fitting fails

    [Tooltip("Padding multiplier for auto-fitting (1.2 = 20% padding)")]
    public float autoFitPadding = 1.2f;        // How much extra space to leave around the model

    //----------------------------------------
    // PRIVATE VARIABLES
    //----------------------------------------
    private GameObject currentModel;           // Currently displayed 3D model instance
    private Vector2 rotationInput;             // Stores mouse/touch movement for rotation
    public float rotationSpeed = 5f;           // Controls how fast the object rotates
    private Vector3 initialCameraPosition;     // Stores the original camera position to maintain direction

    /// <summary>
    /// Initialize the component and set up singleton instance
    /// </summary>
    private void Awake()
    {
        // Singleton pattern setup
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Set up close button listener
        closeButton.onClick.AddListener(CloseViewer);

        // Initialize UI to hidden state
        objectViewerPanel.alpha = 0f;
        objectViewerPanel.blocksRaycasts = false;
        objectViewerPanel.interactable = false;

        // Store the initial camera position to maintain viewing direction
        if (viewerCamera != null)
        {
            initialCameraPosition = viewerCamera.transform.localPosition;
        }
    }

    /// <summary>
    /// Handle input for model rotation
    /// </summary>
    private void Update()
    {
        // Only process input when the viewer is active
        if (objectViewerPanel.alpha != 0f)
        {
            // --- Model Rotation Logic ---
            // Rotate the model when left mouse button is held down
            if (Input.GetMouseButton(0))
            {
                float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
                float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;

                // Rotate the model horizontally around Y axis
                modelHolder.Rotate(Vector3.up, -rotationX, Space.World);

                // Rotate the model vertically around X axis
                modelHolder.Rotate(Vector3.right, rotationY, Space.World);
            }
        }
    }

    /// <summary>
    /// Open the object viewer and display the specified 3D model
    /// </summary>
    /// <param name="collectiblePrefab">The 3D model prefab to display</param>
    /// <param name="collectibleName">Name of the collectible to display in UI</param>
    public void OpenViewer(GameObject collectiblePrefab, string collectibleName)
    {
        // Clean up any existing model
        if (currentModel != null)
            Destroy(currentModel);

        // Instantiate the new model inside the model holder
        currentModel = Instantiate(collectiblePrefab, modelHolder);

        // Disable any scripts on the model to prevent unwanted behavior
        foreach (MonoBehaviour script in currentModel.GetComponentsInChildren<MonoBehaviour>())
        {
            script.enabled = false;
        }

        // Reset model position and rotation
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        // Update UI text
        objectNameText.text = collectibleName;

        // Show the UI panel
        objectViewerPanel.alpha = 1f;
        objectViewerPanel.blocksRaycasts = true;
        objectViewerPanel.interactable = true;

        // Activate the camera
        if (viewerCamera != null)
        {
            viewerCamera.gameObject.SetActive(true);
        }

        // Automatically adjust camera to fit the model
        AutoFitCamera();

        // Set cursor and game state
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;  // Pause the game while viewing the object

        // Update UI state
        UIManager.Instance.currentUIState = UIManager.UIState.ObjectViewer;
    }

    /// <summary>
    /// Automatically adjust camera position to fit the current model in view
    /// </summary>
    private void AutoFitCamera()
    {
        // Safety check
        if (currentModel == null || viewerCamera == null)
            return;

        // Step 1: Calculate the bounds of the model
        Bounds bounds = CalculateModelBounds(currentModel);

        // Step 2: Handle case where bounds calculation failed
        if (bounds.size == Vector3.zero)
        {
            // Fallback to default distance if bounds couldn't be calculated
            UpdateCameraPosition(defaultDistance);
            return;
        }

        // Step 3: Calculate ideal camera distance based on model size
        // - Use the largest dimension of the model (width, height, or depth)
        // - Apply the formula: distance = (size * padding) / (2 * tan(FOV/2))
        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float distance = (maxDimension * autoFitPadding) /
                         (2.0f * Mathf.Tan(0.5f * viewerCamera.fieldOfView * Mathf.Deg2Rad));

        // Step 4: Center the model in view if it's not centered
        Vector3 centerOffset = bounds.center - currentModel.transform.position;
        currentModel.transform.position -= centerOffset;

        // Step 5: Update camera position with the calculated distance
        UpdateCameraPosition(distance);
    }

    /// <summary>
    /// Calculate the combined bounds of all renderers in the model
    /// </summary>
    /// <param name="model">The model to calculate bounds for</param>
    /// <returns>Combined bounds of all renderers</returns>
    private Bounds CalculateModelBounds(GameObject model)
    {
        // Initialize empty bounds
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        // Get all renderers in the model, including children
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        // Only proceed if the model has renderers
        if (renderers.Length > 0)
        {
            // Initialize bounds with the first renderer
            bounds = renderers[0].bounds;

            // Expand bounds to include all other renderers
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        return bounds;
    }

    /// <summary>
    /// Update camera position while maintaining the same viewing direction
    /// </summary>
    /// <param name="distance">The distance to position the camera</param>
    private void UpdateCameraPosition(float distance)
    {
        if (viewerCamera == null)
            return;

        // Get the normalized direction of the camera's initial position
        Vector3 cameraDirection = initialCameraPosition.normalized;

        // Position the camera at the calculated distance while maintaining direction
        viewerCamera.transform.localPosition = cameraDirection * distance;
    }

    /// <summary>
    /// Close the object viewer and return to previous state
    /// </summary>
    public void CloseViewer()
    {
        // Hide the UI panel
        objectViewerPanel.alpha = 0f;
        objectViewerPanel.blocksRaycasts = false;
        objectViewerPanel.interactable = false;

        // Clean up the model
        if (currentModel != null)
            Destroy(currentModel);

        // Deactivate the camera
        if (viewerCamera != null)
        {
            viewerCamera.gameObject.SetActive(false);
        }

        // Return to appropriate UI state
        if (CollectionManager.instance.collectionPanel.activeSelf)
        {
            // If collection panel is still open
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            UIManager.Instance.currentUIState = UIManager.UIState.Collection;
        }
        else
        {
            // Fully exit UI mode
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            UIManager.Instance.currentUIState = UIManager.UIState.None;
        }
    }

    /// <summary>
    /// Check if object viewer is currently active
    /// </summary>
    /// <returns>True if the viewer is open</returns>
    protected override bool IsUIActive()
    {
        return objectViewerPanel.alpha != 0f;
    }

    /// <summary>
    /// Handle Escape key press when object viewer is active
    /// </summary>
    protected override void OnEscapePressed()
    {
        CloseViewer();
    }
}