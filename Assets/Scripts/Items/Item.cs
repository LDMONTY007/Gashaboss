using UnityEngine;

//Interface used for any item in the game
public abstract class Item: MonoBehaviour
{
    // the time till the effect happens again
    // Set this to something in subclasses if needed 
    protected int timeTillReapply;
    public string name;
    public bool isUnlocked;
    void Update(){
        if (timeTillReapply == null) return;
        if (timeTillReapply > 0) timeTillReapply -= Time.deltaTime;
        if (timeTillReapply <= 0) ApplyEffect();
    }
    public override void ApplyEffect();
}