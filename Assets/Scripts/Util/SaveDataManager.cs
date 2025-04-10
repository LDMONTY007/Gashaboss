using System;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEngine.SceneManagement;

// The SaveDataManager handles savedata for the game
// This is the class that will mostly be called from the gamemanager/UI manager
// All objects that need save data need to implemnet the IDataPersistence Interface
public class SaveDataManager: MonoBehaviour{
    [SerializeField] private string fileName;
    [SerializeField] private bool isDataDisabled = false;
    [SerializeField] private bool isProfileOverridden = false;
    [SerializeField] private string testSelectedProfile;
    public ScriptableObjectList dropDataList;
    public ScriptableObjectList itemDataList;
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    private string selectedProfile;
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
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.selectedProfile = dataHandler.GetMostRecentlyUpdatedProfileId();
        if (isProfileOverridden){
            this.selectedProfile = testSelectedProfile;
            Debug.LogWarning("Overriding selected profile with : " + testSelectedProfile);
        } 
    }
    private void OnEnable(){
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable(){
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnApplicationQuit(){
        SaveGame();
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
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
        gameData.lastUpdated = System.DateTime.Now.ToBinary();
        // save data using data handler
        dataHandler.Save(gameData, selectedProfile);
        Debug.Log("Saved Game");
    }



    //find the prefab stored in a dropData in our global dropData scriptable object list.
    public GameObject FindDropGameObj(string key)
    {
        DropData temp = (dropDataList.soList.Find(d => d.name == key) as DropData);
        if (temp == null)
        {
            Debug.LogError("DROP DATA WAS NULL FOR THIS SEARCH: " + key);
        }
        GameObject temp1 = temp.droppedObject;
        //return the dropped object specifically.
        //return (dropDataList.soList.Find(d => d.name == key) as DropData).droppedObject;
        return temp1;
    }

    //find the dropData in our global dropData scriptable object list.
    public DropData FindDropData(string key)
    {
        //return the dropped object specifically.
        return (dropDataList.soList.Find(d => d.name == key) as DropData);
    }

    //find the Item Data stored in the global ItemData scriptable object list.
    public ItemData FindItemData(string key)
    {
        //cast to ItemData and search the list for the key.
        return (itemDataList.soList.Find(d => d.name == key) as ItemData);
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects(){
        //Find all the objects that implement IDataPersistence in the scene (Requires all the objects to also extend MonoBehaviour)
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
    public Dictionary<string, GameData> GetDataAllProfiles(){
        return dataHandler.LoadAll();
    }
    
    public void ChangeLoadedProfile(string ProfileId){
        Debug.Log("Changing Loaded Profile" + ProfileId);
        selectedProfile = ProfileId;
        //go ahead and load gameData for the new profile
        LoadGame();
    }
    public bool HasData(){
        foreach (KeyValuePair<string, GameData> pair in GetDataAllProfiles()){
            if(pair.Key != null) return true;
        }
        return false;
    }
}