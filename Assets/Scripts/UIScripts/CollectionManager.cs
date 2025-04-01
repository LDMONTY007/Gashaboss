// This code manages Collection UI and the Collectibles the player has collected.
// This script is attached to the CollectionManager GameObject.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionManager : UIInputHandler, IDataPersistence
{
    public static CollectionManager instance; // Singleton for easy access

    [Header("UI Elements")]
    public GameObject collectionPanel; // The UI panel for the collection menu
    public Transform collectionContent; // Scroll View content holder
    public GameObject collectibleButtonPrefab; // Prefab for collectible buttons
    public Button closeCollectionButton; // Button to close the collection UI

    [Header("Collected Collectibles")]
    private Dictionary<string, DropData> collectedCollectibles;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        collectionPanel.SetActive(false); // UI starts hidden
        closeCollectionButton.onClick.AddListener(CloseCollection);
        collectedCollectibles = new Dictionary<string, DropData>(); // Initialize dictionary
    }

    /*
    // COMMENTED OUT: We no longer check Escape here because UIManager handles it centrally.
    private void Update()
    {
        if (collectionPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCollection();
            return;
        }
    }
    */

    public void AddToCollection(Collectible collectible)
    {
        if (collectedCollectibles.ContainsKey(collectible.collectibleName)) return; // Avoid duplicates
        collectedCollectibles.Add(collectible.collectibleName, collectible.collectibleData);
    }

    public void OpenCollection()
    {
        collectionPanel.SetActive(true);
        foreach (KeyValuePair<string, DropData> collectible in this.collectedCollectibles)
        {
            LoadMenuItem(collectible.Value.droppedObject, collectible.Key);
        }
        SetCursorState(true);
        UIManager.Instance.currentUIState = UIManager.UIState.Collection; // <<< USE STATE MACHINE
    }


    public void CloseCollection()
    {
        foreach (Transform child in collectionContent)
        {
            Destroy(child.gameObject);
        }

        collectionPanel.SetActive(false);

        // --- Proper cursor and state handling ---
        if (ObjectViewer.instance.objectViewerPanel.alpha != 0f)
        {
            // If Object Viewer is still open
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            UIManager.Instance.currentUIState = UIManager.UIState.ObjectViewer;
        }
        else
        {
            // Everything is closed
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            UIManager.Instance.currentUIState = UIManager.UIState.None;
        }
    }



    public void LoadMenuItem(GameObject collectiblePrefab, string collectibleName)
    {
        GameObject button = Instantiate(collectibleButtonPrefab, collectionContent);

        TextMeshProUGUI collectibleText = button.transform.Find("CollectibleText").GetComponent<TextMeshProUGUI>();
        if (collectibleText != null)
        {
            collectibleText.text = collectibleName;
        }

        button.GetComponent<Button>().onClick.AddListener(() => ObjectViewer.instance.OpenViewer(collectiblePrefab, collectibleName));
    }

    public void LoadData(GameData gameData)
    {
        this.collectedCollectibles = gameData.collectedCollectibles;
    }

    public void SaveData(GameData gameData)
    {
        gameData.collectedCollectibles = this.collectedCollectibles;
    }

    private void SetCursorState(bool uiMode)
    {
        if (uiMode)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Optional: pause game
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f; // Resume game
        }
    }

    // Check if collection panel is open
    protected override bool IsUIActive()
    {
        return collectionPanel.activeSelf;
    }

    // What happens when Escape is pressed while Collection UI is open
    protected override void OnEscapePressed()
    {
        CloseCollection();
    }

}
