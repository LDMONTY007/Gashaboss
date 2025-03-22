using System;
using UnityEngine;
// Used to attach to the game object prefabs for items
// Passes logic to the ScriptableObject for that item
// Primarily handles adding to inventory and gameobject deletion
public class Item: MonoBehaviour, ICollectable
{
    [SerializeField] private ItemData item; // Assign scriptable object via inspector
    public void OnCollect(){
        if(item == null) {
            Debug.LogError("Item Instance Has No Object Assigned: Please Add One.\n Aborting Item Pickup.");
            Destroy(gameObject);
            return;
        }
        Player.instance.inventory.Add(item);
        item.OnPickup();
        Destroy(gameObject); // Destroy only the physical object, not the script
        Debug.Log("Deleting Item Prefab, Successfully added to inventory.");
    }
}