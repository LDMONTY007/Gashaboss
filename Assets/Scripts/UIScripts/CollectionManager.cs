// This code manages Collection UI and the weapons the player has collected.
// This script is attached to the CollectionManager GameObject.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance; // Singleton for easy access

    [Header("UI Elements")]
    public GameObject collectionPanel; // The UI panel for the collection menu
    public Transform collectionContent; // Scroll View content holder
    public GameObject weaponButtonPrefab; // Prefab for weapon buttons
    public Button closeCollectionButton; // Button to close the collection UI

    [Header("Collected Weapons")]
    private List<GameObject> collectedWeapons = new List<GameObject>(); // Stores collected weapon prefabs
    private List<string> weaponNames = new List<string>(); // Stores weapon names

    private void Awake()
    {
        // Ensure only one instance exists (Singleton)
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        collectionPanel.SetActive(false); // Ensure Collection UI starts hidden

        closeCollectionButton.onClick.AddListener(CloseCollection); // Attach close function to button
    }

    // Adds a weapon to the collection when the player collects it
    public void AddToCollection(GameObject weaponPrefab, string weaponName)
    {
        // Prevent duplicate weapons
        if (weaponNames.Contains(weaponName)) return;

        collectedWeapons.Add(weaponPrefab);
        weaponNames.Add(weaponName);

        // Create a new button in the collection menu
        GameObject button = Instantiate(weaponButtonPrefab, collectionContent);
        button.GetComponentInChildren<TextMeshProUGUI>().text = weaponName;

        // Set button to open the Object Viewer when clicked
        button.GetComponent<Button>().onClick.AddListener(() => ObjectViewer.Instance.OpenViewer(weaponPrefab, weaponName));
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
}
