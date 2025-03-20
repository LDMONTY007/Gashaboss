// Use this script to make a object viewable in the Object Viewer panel when the player interacts with it.
// Attach this script to the collectible object in the scene.
// Assign the weapon’s 3D prefab and name in the Inspector.
// When the player interacts with the collectible, the ViewWeapon() function is called.
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public GameObject weaponModelPrefab; // Assign in Inspector
    public string weaponName; // Assign the name in the Inspector

    private bool collected = false; // Prevents duplicate collection

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true; // Mark as collected

            // Add weapon to the Collection
            CollectionManager.Instance.AddToCollection(weaponModelPrefab, weaponName);

            // Destroy the physical object after collection
            Destroy(gameObject);
        }
    }
}
