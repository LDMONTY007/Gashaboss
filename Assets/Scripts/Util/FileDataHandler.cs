using System;
using System.IO;

public class FileDataHandler{
    private string dirPath = "";
    private string fileName = "";

    public FileDataHandler(string dirPath, string fileName){
        this.dirPath = dirPath;
        this.fileName = fileName;
    } 
    public GameData Load(){
        // Used to avoid os differences
        string fullPath = Path.Combine(dirPath, fileName);
        GameData loadedData;
        if (File.Exists(fullPath)){
            try{
                string dataToLoad;
                using(FileStream stream = new FileStream(fullPath, FileMode.Open)){
                    dataToLoad = reader.ReadToEnd();
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
    public void Save(GameData data){
        // Used to avoid os differences
        string fullPath = Path.Combine(dirPath, fileName);
        try{
            // make save file if not already made
            Directory.CreateDriectory(Path.GetDirectoryName(fullPath));
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