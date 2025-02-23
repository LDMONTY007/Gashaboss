//This class holds the datastructure used for saves
//All things that need to persist between runs should go in here
//TODO: Add Data needed to persist across saves
//HowTo: Add the IDataPersitence Interface to any class that needs to save the data
//Add the data needed to save to this class and give a default value to the constructor below
//That's it, You're done!
//Dictionaries are a good type for collectabiles
public class GameData{
    public long lastUpdated;
    public int coins;
    
    //These are the default values the game starts with when no data to load
    public GameData(){
        this.coins = 5;

    }
}