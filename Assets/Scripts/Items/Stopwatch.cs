using UnityEngine;

public class StopWatch: Item {
    void Start(){
        Player.onPlayerHit += ApplyEffect();
    }
    void OnDestroy(){
        Player.onPlayerHit -= ApplyEffect();
    }
    public void ApplyEffect(){
        //TODO: .2 secs added arbitrarily, should probably test
        Player.modifiers.Add(new StatModifier(StatModified.iFrameTime, .2f, StatModifierType.Add, "Stop Watch: " + this));
    }
}