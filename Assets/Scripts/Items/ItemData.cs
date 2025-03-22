using System;
using UnityEngine;
//abstract used for any item in the game
//If the Item made modifies a stat make sure to add it
// to the StatModified enum over in Util/StatModifier.cs
public abstract class ItemData : ScriptableObject{
    public string description;
    // OnPickup does intial setup
    // Add to listeners, intialize names and fields, etc.
    public abstract void OnPickup();
    // RemoveItem should remove any listeners and from the player inventory.
    public abstract void RemoveItem();
    // Whatever effect needs to be applied by the item.
    public abstract void ApplyEffect();
}
