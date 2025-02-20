public interface IDataPersistance{
    void LoadData(GameData data);
    void SaveData(ref GameData data); //Use a reference so the method can modify the data object for storage.
}