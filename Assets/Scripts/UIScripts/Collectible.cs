// This script allows an object to be added to the Collection UI when the player interacts with it.
// Attach this script to collectible objects in the scene. (3D Objects that can be picked up, or assign it to something gained from a quest/mission)
// Assign the collectible�s 3D prefab, name, and icon in the Inspector.

using UnityEngine;

public class Collectible : MonoBehaviour
{
    public GameObject collectibleModelPrefab; // Assign the 3D model prefab in Inspector
    public string collectibleName; // Assign the name in Inspector
    public Sprite collectibleIcon; // Assign the collectible icon in Inspector

    private bool collected = false; // Prevents duplicate collection

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true; // Mark as collected

            // Corrected method call to include all required arguments
            CollectionManager.instance.AddToCollection(collectibleModelPrefab, collectibleName, collectibleIcon);

            // Destroy the physical object after collection
            Destroy(gameObject);
        }
    }
}
