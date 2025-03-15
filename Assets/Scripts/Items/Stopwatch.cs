using UnityEngine;
using System;

public class StopWatch: Item {
    void Start(){
        Player.instance.onPlayerHit += ApplyEffect;
    }
    void OnDestroy(){
        Player.instance.onPlayerHit -= ApplyEffect;
    }
    public void ApplyEffect(){
        //TODO: .2 secs added arbitrarily, should probably test
        Player.instance.modifiers.Add(new StatModifier(StatModified.iFrameTime, .2f, StatModifierType.Add, "Stop Watch: " + this));
    }
}