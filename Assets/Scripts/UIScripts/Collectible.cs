// Use this script to make a object viewable in the Object Viewer panel when the player interacts with it.
// Attach this script to the collectible object in the scene.
// Assign the weapon’s 3D prefab and name in the Inspector.
// When the player interacts with the collectible, the ViewWeapon() function is called.
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public GameObject weaponModelPrefab; // Assign the weapon’s 3D prefab in the Inspector
    public string weaponName; // Set the name of the weapon in the Inspector

    // This function is called when the player chooses to view the collected weapon
    public void ViewWeapon()
    {
        // Calls the Object Viewer Singleton to display the selected weapon
        ObjectViewer.Instance.OpenViewer(weaponModelPrefab, weaponName);
    }
}
