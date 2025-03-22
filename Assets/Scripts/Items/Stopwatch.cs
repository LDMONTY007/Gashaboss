using System;
using UnityEngine;
[CreateAssetMenu(fileName = "New StopWatch", menuName = "Items/StopWatch")]
public class StopWatch: ItemData{
    public override void OnPickup(){
        this.name = "StopWatch";
        this.description = "The StopWatch will help you freeze time when you really need it. Use it to avoid the enemies for longer!";
        Player.instance.onPlayerHit += ApplyEffect;
        ApplyEffect();
    }
    public override void RemoveItem(){
        Player.instance.inventory.Remove(this);
        Player.instance.onPlayerHit -= ApplyEffect;
    }
    public override void ApplyEffect(){
        //TODO: .2 secs added arbitrarily, should probably test
        Player.instance.modifiers.Add(new StatModifier(StatModified.iFrameTime, .2f, StatModifierType.Add, this));
        Debug.Log("Applied StopWatch Effect!");
    }
}