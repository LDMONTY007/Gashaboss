// This script allows an object to be added to the Collection UI when the player interacts with it.
// Attach this script to collectible objects in the scene. (3D Objects that can be picked up, or assign it to something gained from a quest/mission)
// Assign the collectible�s 3D prefab, name, and icon in the Inspector.

using UnityEngine;

public abstract class Collectible : MonoBehaviour
{
    public DropData collectibleData; // Assign in Inspector, use drop data scriptable for all the items. 
    public string collectibleName; // Assign the name in Inspector
    public abstract void OnCollect();
}
