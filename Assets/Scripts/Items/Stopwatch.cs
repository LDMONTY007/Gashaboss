using UnityEngine;
using System;

public class StopWatch: Item{
    
    public override void OnPickup(){
        transform.SetParent(null); // Detach from game object to prevent destruction

        Player.instance.inventory.Add(this);
        Player.instance.onPlayerHit += ApplyEffect;
        ApplyEffect();
        Destroy(gameObject); // Destroy only the physical object, not the script
    }
    public override void RemoveItem(){
        Player.instance.inventory.Add(this);
        Player.instance.onPlayerHit -= ApplyEffect;
    }
    public override void ApplyEffect(){
        //TODO: .2 secs added arbitrarily, should probably test
        Player.instance.modifiers.Add(new StatModifier(StatModified.iFrameTime, .2f, StatModifierType.Add, this));
    }
}