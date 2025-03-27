using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for using TextMeshPro UI

public class ObjectViewer : MonoBehaviour
{
    public static ObjectViewer instance; // Singleton instance 

    [Header("UI Elements")]
    public GameObject objectViewerPanel; 
    public TextMeshProUGUI objectNameText; 
    public Button closeButton; 

    [Header("3D Model Holder")]
    public Transform modelHolder;
    public Camera viewerCamera;

    private GameObject currentModel; // Stores the currently displayed 3D model
    private Vector2 rotationInput; // Stores mouse/touch movement for rotation
    public float rotationSpeed = 5f; // Controls how fast the object rotates

    private void Awake()
    {
        // Implement Singleton pattern: If an instance doesn't exist, assign this one.
        if (instance == null)
            instance = this;
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

    public void OpenViewer(GameObject collectiblePrefab, string collectibleName)
    {
        if (currentModel != null)
            Destroy(currentModel);

        // Instantiate the selected collectible inside modelHolder
        currentModel = Instantiate(collectiblePrefab, modelHolder);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        // Update UI with collectible name
        objectNameText.text = collectibleName;

        // Show the Object Viewer UI
        objectViewerPanel.SetActive(true);

        // Enable Viewer Camera when opening
        if (viewerCamera != null)
        {
            viewerCamera.gameObject.SetActive(true);
        }

    }


    // Closes the Object Viewer and removes the 3D model
    public void CloseViewer()
    {
        objectViewerPanel.SetActive(false); // Hide the UI panel

        // Destroy the currently displayed model (if any)
        if (currentModel != null)
            Destroy(currentModel);

        // Disable Viewer Camera when closing
        if (viewerCamera != null)
        {
            viewerCamera.gameObject.SetActive(false);
        }
    }
}
