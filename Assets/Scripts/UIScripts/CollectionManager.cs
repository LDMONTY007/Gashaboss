// This code manages Collection UI and the Collectibles the player has collected.
// This script is attached to the CollectionManager GameObject.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionManager : MonoBehaviour, IDataPersistence
{
    public static CollectionManager instance; // Singleton for easy access

    [Header("UI Elements")]
    public GameObject collectionPanel; // The UI panel for the collection menu
    public Transform collectionContent; // Scroll View content holder
    public GameObject collectibleButtonPrefab; // Prefab for collectible buttons
    public Button closeCollectionButton; // Button to close the collection UI

    [Header("Collected Collectibles")]
    private Dictionary<string, GameObject> collectedCollectibles = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // Ensure only one instance exists (Singleton)
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        collectionPanel.SetActive(false); // Ensure Collection UI starts hidden

        closeCollectionButton.onClick.AddListener(CloseCollection); // Attach close function to button
    }

    public void AddToCollection(GameObject collectiblePrefab, string collectibleName, Sprite collectibleIcon)
    {
        if (collectedCollectibles.ContainsKey(collectibleName)) return; // Avoid duplicates

        collectedCollectibles.Add(collectibleName, collectiblePrefab);

        LoadMenuItem(collectiblePrefab, collectibleName, collectibleIcon);
    }

    // Opens the Collection UI
    public void OpenCollection()
    {
        collectionPanel.SetActive(true);
    }

    // Closes the Collection UI
    public void CloseCollection()
    {
        collectionPanel.SetActive(false);
    }

    public void LoadMenuItem(GameObject collectiblePrefab, string collectibleName, Sprite collectibleIcon){
        GameObject button = Instantiate(collectibleButtonPrefab, collectionContent);

        // Set collectible name dynamically
        TextMeshProUGUI collectibleText = button.transform.Find("CollectibleText").GetComponent<TextMeshProUGUI>();
        if (collectibleText != null)
        {
            collectibleText.text = collectibleName;
        }

        // Set collectible image dynamically
        Image collectibleImage = button.transform.Find("CollectibleImage").GetComponent<Image>();
        if (collectibleImage != null)
        {
            collectibleImage.sprite = collectibleIcon;
            collectibleImage.preserveAspect = true;  // Ensures the image isn't stretched
        }

        // Set button to open Object Viewer when clicked
        button.GetComponent<Button>().onClick.AddListener(() => ObjectViewer.instance.OpenViewer(collectiblePrefab, collectibleName));
    }
    public void LoadData(GameData gameData){
        this.collectedCollectibles = gameData.collectedCollectibles;
        foreach(KeyValuePair<string,GameObject> collectible in this.collectedCollectibles){
            LoadMenuItem(collectible.Key, collectible.Value, collectible.Value.icon);
        }
    }
    public void SaveData(GameData gameData){
        gameData.collectedCollectibles = this.collectedCollectibles;
    }
}
