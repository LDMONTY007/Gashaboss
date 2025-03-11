using UnityEngine;

//Interface used for any item in the game
public abstract class Item: MonoBehaviour
{
    // the time till the effect happens again
    // set this to something in subclasses during the start method if needed 
    // I put it in the abstract because I expect this to be used often enough
    // that it would be useful to not have to add the code in every subclass   
    protected float timeTillReapply;
    public string name;
    void Update(){
        if (timeTillReapply == null) return;
        if (timeTillReapply > 0) timeTillReapply -= Time.deltaTime;
        if (timeTillReapply <= 0) ApplyEffect();
    }
    public abstract void ApplyEffect();
}