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
    public T modValue { get; private set; }
    public StatModifierType type { get; private set; }
    public object source { get; private set; } // Track what added the modifier

    public StatModifier(StatModified stat, T value, StatModifierType type, object source = null){
        stat = stat;
        modValue = value;
        type = type;
        source = source;
    }
    public T makeModifications(T value){
        if((value is string)){
            Debug.LogError("Tried to modify a string value! Check Modifier: " + object);
            return null;
        }
        T result = value;
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
                case default:
                    Debug.LogError("Modifier does not have appropriate modifcation type, Source: " + object);
                    return null;
            }
        return result;
    }
}