using System;
using System.Linq; 
using UnityEngine;

public class SaveDataManager: MonoBehaviour{
    [SerializeField] private string fileName;
    [SerializeField] private bool isDataDisabled = false;
    [SerializeField] private bool isProfileOverridden = false;
    [SerializeField] private string testSelectedProfile;
    private GameData gameData;
    private List<IDataPersistance> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    private string selectedProfile;
    private Coroutine autoSaveCoroutine;
    public static SaveDataManager instance {get; private set;}
    
    private void Awake(){
        if (instance != null){
            Debug.Log("Found more than one SaveDataManager, yeeting the newest.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        if(isDataDisabled) Debug.LogWarning("Save Data is currently disabled.");
        // Application.persistentDataPath gives off the file directory for the app
        this.dataHandler = newFileDataHandler(Application.persistentDataPath, fileName);
        this.selectedProfile = dataHandler.GetMostRecentlyUpdatedProfileId();
        if (isProfileOverridden){
            this.selectedProfile = testSelectedProfile;
            Debug.LogWarning("Overriding selected profile with : " + testSelectedProfile);
        } 
    }
    private void Start(){
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
    }
    public void NewGame(){
        this.gameData = new GameData();
    }
    public void LoadGame(){
        if (isDataDisabled) return;
        // Load save data from data handler/ file
        this.gameData = dataHandler.Load(selectedProfile);
        if (this.gameData == null){
            Debug.Log("No data was found. Loading New Game instead.");
            NewGame();
        }
        // Push data to scripts
        foreach (IDataPersistence dPObj in dataPersistenceObjects){
            dPObj.LoadData(gameData);
        }
    }
    public void SaveGame(){
        if (isDataDisabled) return;
        // pass data to other scripts so they can update it
        foreach (IDataPersistence dPObj in dataPersistenceObjects){
            dPObj.SaveData(gameData);
        }
        gameData.lastUpdated = System.DataTime.Now.ToBinary();
        // save data using data handler
        dataHandler.Save(gameData, selectedProfile);
    }
    private List<IDataPersistence> FindAllDataPersistenceObjects(){
        //Find all the objects that implement IDataPersistence in the scene (Requires all the objects to also extend MonoBehaviour)
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
    public Dictionary<string, GameData> GetDataAllProfiles(){
        return dataHandler.LoadAllProfiles();
    }
}