// This snippet is part of the file Assets/Scripts/UIScripts/Collectible.cs
// This needs to be attached whatever "collection" system we have in place to allow the player to view their collectibles.
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    // This function is called when a player selects an item in their inventory
    public void ViewSelectedItem(GameObject weaponPrefab, string weaponName)
    {
        // Calls the Object Viewer Singleton to display the selected weapon
        ObjectViewer.instance.OpenViewer(weaponPrefab, weaponName);
    }
}
