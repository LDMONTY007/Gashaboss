public interface IDataPersistence{
    // Take the field needed from the GameData members to 
    // Intialize the given feild in the class
    void LoadData(GameData data);
    // Set the field needed from the GameData members to the current value
    void SaveData(GameData data);
}