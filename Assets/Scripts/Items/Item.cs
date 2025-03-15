using System;
//Interface used for any item in the game
//If the Item made modifies a stat make sure to add it
// to the StatModified enum over in Util/StatModifier.cs
public interface Item{
    public void ApplyEffect();
}