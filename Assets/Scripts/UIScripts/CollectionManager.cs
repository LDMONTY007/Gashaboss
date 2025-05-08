// This code manages Collection UI and the Collectibles the player has collected.
// This script is attached to the CollectionManager GameObject.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CollectionManager : UIInputHandler, IDataPersistence
{
    public static CollectionManager instance; // Singleton for easy access

    [Header("UI Elements")]
    public GameObject collectionPanel; // The UI panel for the collection menu
    public Transform collectionContent; // Scroll View content holder
    public GameObject collectibleButtonPrefab; // Prefab for collectible buttons
    public Button closeCollectionButton; // Button to close the collection UI
    [SerializeField] private GameObject gachaMachine; // needed to spawn, bought collectibles

    [Header("Collected Collectibles")]
    private Dictionary<string, DropData> collectedCollectibles;

    public event Action OnCollectionOpen;
    public event Action OnCollectionClose;

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
            LoadMenuItem(collectible.Value.droppedObject, collectible.Key, collectible.Value.cost);
        }
        UIManager.Instance.TogglePause();
        UIManager.Instance.currentUIState = UIManager.UIState.Collection; // <<< USE STATE MACHINE

        OnCollectionOpen?.Invoke();
    }


    public void CloseCollection()
    {
        foreach (Transform child in collectionContent)
        {
            Destroy(child.gameObject);
        }

        collectionPanel.SetActive(false);

        // --- Proper cursor and state handling ---
        if (ObjectViewer.instance.objectViewerPanel.alpha != 0f){
            ObjectViewer.instance.CloseViewer();
        }
        UIManager.Instance.TogglePause();
        UIManager.Instance.currentUIState = UIManager.UIState.None;
        OnCollectionClose?.Invoke();
    }
    public void LoadMenuItem(GameObject collectiblePrefab, string collectibleName, int cost)
    {
        Debug.Log("Loading Menu Item");

        GameObject listing = Instantiate(collectibleButtonPrefab, collectionContent);

        TextMeshProUGUI collectibleText = listing.transform.Find("CollectibleText").GetComponent<TextMeshProUGUI>();
        if (collectibleText != null)
        {
            collectibleText.text = collectibleName;
        }
        // add event to view button
        listing.transform.Find("CollectibleButton").GetComponent<Button>().onClick.AddListener(() => ObjectViewer.instance.OpenViewer(collectiblePrefab, collectibleName));
        // add event to buy button
        Transform buyButton = listing.transform.Find("BuyButton");
        buyButton.GetComponent<Button>().onClick.AddListener(() => BuyCollectible(collectibleName, cost));
        
        //Update the buy button to show cost        
        TextMeshProUGUI costText = buyButton.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        if (costText != null)
        {
            costText.text = "Buy: " + cost;
        }
    }

    public void LoadData(GameData gameData)
    {
        //clear the collectibles list before loading them.
        collectedCollectibles.Clear();
        foreach (string key in gameData.collectedCollectibles)
        {
            //get the drop using our search function
            DropData dropToSave = SaveDataManager.instance.FindDropData(key);
            Debug.Log(key);

            //if the drop is an environmental unlock,
            //make sure to unlock it when we load the data in.
            //this is where we unlock any gashapon machines the player has unlocked in their save data.
            if (dropToSave.isEnvironmentalUnlock)
            {
                Debug.Log("ENVIRONMENTAL UNLOCK");
                //instantiate the object so that it unlocks the environmental object.
                GameObject tempDrop = Instantiate(dropToSave.droppedObject);
                tempDrop.GetComponent<GachaDrop>().unlockOnStart = true;
            }

            //get the name of the collectible from the collectible itself and store
            //the drop data.
            collectedCollectibles.Add(dropToSave.droppedObject.GetComponent<Collectible>().collectibleName, dropToSave);
        }
    }

    public void SaveData(GameData gameData)
    {
        //loop through and save all the collected collectibles.
        gameData.collectedCollectibles.Clear();
        foreach (var kvp in collectedCollectibles)
        {
            gameData.collectedCollectibles.Add(kvp.Value.name);
        }
        
    }

    //used in gacha machine to remove any objects that
    //have already been collected by the player and are only collected once.
    public bool CollectionContains(DropData data)
    {
        return collectedCollectibles.ContainsValue(data);
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

    private void BuyCollectible(string collectibleName, int cost){
        if (Player.instance.SpendCaps(cost)){
            DropData spawn = collectedCollectibles[collectibleName];
            gachaMachine.GetComponent<GachaMachine>().SpawnSpecificCapsule(spawn);
        }
    }
}
