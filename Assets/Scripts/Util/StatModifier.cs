using UnityEngine;
public enum StatModifierType{Add, Subtract, PercentageMul, PercentageDiv}
public enum StatModified{
    // Add more as needed, only add if item needs it
    // If adding something here please update or add a Todo to update 
    // Player class to adjust modifier stats
    // Keep names to the same spelling as the stat name for conventions sake
    iFrameTime
}
public class StatModifier{
    public StatModified stat { get; private set; }
    public float modValue { get; private set; }
    public StatModifierType type { get; private set; }
    public object source { get; private set; } // Track what added the modifier

    public StatModifier(StatModified stat, float value, StatModifierType type, object source = null){
        this.stat = stat;
        this.modValue = value;
        this.type = type;
        this.source = source;
    }
    public float makeModifications(float value){
        float result = value;
        switch (this.type){
                case StatModifierType.Add:
                    result += this.modValue;
                    break;
                case StatModifierType.Subtract:
                    result -= this.modValue;
                    break;
                case StatModifierType.PercentageMul:
                    result *= this.modValue;
                    break;
                case StatModifierType.PercentageDiv:
                    result /= this.modValue;
                    break;
            }
        return result;
    }
}