using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for using TextMeshPro UI

public class ObjectViewer : MonoBehaviour
{
    public static ObjectViewer Instance; // Singleton instance 

    [Header("UI Elements")]
    public GameObject objectViewerPanel; 
    public TextMeshProUGUI objectNameText; 
    public Button closeButton; 

    [Header("3D Model Holder")]
    public Transform modelHolder;

    private GameObject currentModel; // Stores the currently displayed 3D model
    private Vector2 rotationInput; // Stores mouse/touch movement for rotation
    public float rotationSpeed = 5f; // Controls how fast the object rotates

    private void Awake()
    {
        // Implement Singleton pattern: If an instance doesn't exist, assign this one.
        if (Instance == null)
            Instance = this;
        else
        {
            // If another instance exists, destroy this one to prevent duplicates
            Destroy(gameObject);
            return;
        }

        // Object Viewer starts hidden
        objectViewerPanel.SetActive(false);

        // Attach the CloseViewer() function to the close button click event
        closeButton.onClick.AddListener(CloseViewer);
    }

    private void Update()
    {
        // Check if the Object Viewer panel is open
        if (objectViewerPanel.activeSelf)
        {
            // Rotate model using mouse drag (left-click)
            if (Input.GetMouseButton(0)) // 0 = Left Mouse Button
            {
                // Get mouse movement values for X and Y axis
                float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
                float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;

                // Rotate the modelHolder in World Space (left/right rotation)
                modelHolder.Rotate(Vector3.up, -rotationX, Space.World);

                // Rotate the modelHolder in Local Space (up/down rotation)
                modelHolder.Rotate(Vector3.right, rotationY, Space.World);
            }
        }
    }

    // Opens the Object Viewer and displays the selected 3D model
    public void OpenViewer(GameObject modelPrefab, string objectName)
    {
        // If there's already a model being viewed, destroy it
        if (currentModel != null)
            Destroy(currentModel);

        // Instantiate (spawn) the selected model inside the modelHolder
        currentModel = Instantiate(modelPrefab, modelHolder);

        // Reset model position & rotation to keep it centered
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        // Update the UI text to display the object's name
        objectNameText.text = objectName;

        // Show the Object Viewer UI panel
        objectViewerPanel.SetActive(true);
    }

    // Closes the Object Viewer and removes the 3D model
    public void CloseViewer()
    {
        objectViewerPanel.SetActive(false); // Hide the UI panel

        // Destroy the currently displayed model (if any)
        if (currentModel != null)
            Destroy(currentModel);
    }
}
