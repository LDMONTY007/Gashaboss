using System.Linq; 
using UnityEngine;

public class SaveDataManager: MonoBehaviour{
    [SerializeField] private string fileName;
    private GameData gameData;
    private List<IDataPersistance> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    public static SaveDataManager instance {get; private set;}
    
    private void Awake(){
        if (instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else{
            Destroy(gameObject);
        }
    }
    private void Start(){
        // Application.persistentDataPath gives off the file directory for the app
        this.dataHandler = newFileDataHandler(Application.persistentDataPath, fileName);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame(){
        this.gameData = new GameData();
    }

    public void LoadGame(){
        // Load save data from data handler/ file
        this.gameData = dataHandler.Load();
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
        // pass data to other scripts so they can update it
        foreach (IDataPersistence dPObj in dataPersistenceObjects){
            dPObj.SaveData(ref gameData);
        }
        // save data using data handler
        dataHandler.Save(gameData);
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects(){
        //Find all the objects that implement IDataPersistence in the scene (Requires all the objects to also extend MonoBehaviour)
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}