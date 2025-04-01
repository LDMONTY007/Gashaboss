using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for using TextMeshPro UI

public class ObjectViewer : UIInputHandler
{
    public static ObjectViewer instance; // Singleton instance 

    [Header("UI Elements")]
    public CanvasGroup objectViewerPanel;
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
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        closeButton.onClick.AddListener(CloseViewer);

        objectViewerPanel.alpha = 0f;
        objectViewerPanel.blocksRaycasts = false;
        objectViewerPanel.interactable = false;
    }

    private void Update()
    {
        if (objectViewerPanel.alpha != 0f)
        {
            /*
            // COMMENTED OUT: We no longer check Escape here because UIManager handles it centrally.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseViewer();
                return; 
            }
            */

            // --- Rotation ---
            if (Input.GetMouseButton(0))
            {
                float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
                float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;

                modelHolder.Rotate(Vector3.up, -rotationX, Space.World);
                modelHolder.Rotate(Vector3.right, rotationY, Space.World);
            }
        }
    }


    public void OpenViewer(GameObject collectiblePrefab, string collectibleName)
    {
        if (currentModel != null)
            Destroy(currentModel);

        currentModel = Instantiate(collectiblePrefab, modelHolder);
        foreach (MonoBehaviour script in currentModel.GetComponentsInChildren<MonoBehaviour>())
        {
            script.enabled = false;
        }
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        objectNameText.text = collectibleName;

        objectViewerPanel.alpha = 1f;
        objectViewerPanel.blocksRaycasts = true;
        objectViewerPanel.interactable = true;

        if (viewerCamera != null)
        {
            viewerCamera.gameObject.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        // State-based
        UIManager.Instance.currentUIState = UIManager.UIState.ObjectViewer;
    }


    public void CloseViewer()
    {
        objectViewerPanel.alpha = 0f;
        objectViewerPanel.blocksRaycasts = false;
        objectViewerPanel.interactable = false;

        if (currentModel != null)
            Destroy(currentModel);

        if (viewerCamera != null)
        {
            viewerCamera.gameObject.SetActive(false);
        }

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



    // Check if object viewer is open
    protected override bool IsUIActive()
    {
        return objectViewerPanel.alpha != 0f;
    }

    // What happens when Escape is pressed while Object Viewer is open
    protected override void OnEscapePressed()
    {
        CloseViewer();
    }

}
