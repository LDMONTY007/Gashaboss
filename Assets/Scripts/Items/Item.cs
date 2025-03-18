using System;
using UnityEngine;
//abstract used for any item in the game
//If the Item made modifies a stat make sure to add it
// to the StatModified enum over in Util/StatModifier.cs
public abstract class Item: MonoBehaviour{
    public abstract void OnPickup();
    public abstract void RemoveItem();
    public abstract void ApplyEffect();
}