using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class FileDataHandler{
    private string dirPath = "";
    private string fileName = "";

    public FileDataHandler(string dirPath, string fileName){
        this.dirPath = dirPath;
        this.fileName = fileName;
    }
    public string GetMostRecentlyUpdatedProfileId(){
        string mostRecentProfileId = null;
        Dictionary<string, GameData> profiles = LoadAll();
        foreach(KeyValuePair<string, GameData> pair in profiles){
            string profileId = pair.Key;
            GameData gameData = pair.Value;

            if (gameData == null) continue;
            if (mostRecentProfileId == null) mostRecentProfileId = profileId;
            else{   
                DateTime mostRecentTime = DateTime.FromBinary(profiles[mostRecentProfileId].lastUpdated);
                DateTime newDateTime = DateTime.FromBinary(gameData.lastUpdated);
                if (newDateTime > mostRecentTime) mostRecentProfileId = profileId;
            }
        }
        return mostRecentProfileId;
    } 
    public GameData Load(string saveID){
        // Safety check: nothing to return from null profile
        if (saveID == null) return null;

        // Used to avoid os differences
        string fullPath = Path.Combine(dirPath, saveID, fileName);
        GameData loadedData = null;
        if (File.Exists(fullPath)){
            try{
                string dataToLoad = "";
                using(FileStream stream = new FileStream(fullPath, FileMode.Open)){
                    using (StreamReader reader = new StreamReader(stream)){
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                //deserialize the data
                //TODO: Replace with Json.NET Utility
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }catch(Exception e){
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }
    public Dictionary<string, GameData> LoadAll(){
        Dictionary<string, GameData> profileDictionary = new Dictionary<string, GameData>();

        //Loop over all directory names in save folder
        IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dirPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos){
            string profileId = dirInfo.Name;
            //check if datafile exists, if not it should be skipped as it's not a profile
            //in case there are other folders/info stored in dedicated directory
            string fullPath = Path.Combine(dirPath, profileId, fileName);
            if (!File.Exists(fullPath)){
                Debug.LogWarning("Skipping directory when loading profiles, does not contain data: " + profileId);
                continue;
            }
            // Load game data if valid, log error if not
            GameData profileData = Load(profileId);
            if(profileData == null){
                Debug.LogError("Tried to load profile, but flames instead... ProfileID: " + profileId);
            }else{
                profileDictionary.Add(profileId, profileData);
            }
        }
        
        return profileDictionary;
    }
    public void Save(GameData data, string saveID){
        //saftey check: can't save to a null file
        if (saveID == null) return;
        // Used to avoid os differences
        string fullPath = Path.Combine(dirPath, saveID, fileName);
        try{
            // make save file if not already made
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            // Serialize into Json file
            //TODO: Replace with Json.NET Utility
            string dataToStore = JsonUtility.ToJson(data, true);
            // using Create Mode to make or overwrite given file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create)){
                using (StreamWriter writer = new StreamWriter(stream)){
                    writer.Write(dataToStore);
                }
            }

        }catch(Exception e){
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
}