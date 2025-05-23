using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This class holds the datastructure used for saves
//All things that need to persist between runs should go in here
//TODO: Add Data needed to persist across saves
//HowTo: Add the IDataPersitence Interface to any class that needs to save the data
//Add the data needed to save to this class and give a default value to the constructor below
//That's it, You're done!
//Dictionaries are a good type for collectabiles
public class GameData{
    public long lastUpdated;
    public bool didCompleteTutorial;
    public int coins;
    public int caps;
    public string playerWeapon;
    public List<StatModifier> modifiers;
    public List<string> inventory;
    public List<string> collectedCollectibles;


    //These are the default values the game starts with when no data to load
    public GameData(){
        coins = 5;
        caps = 0;
        playerWeapon = string.Empty;
        modifiers = new List<StatModifier>();
        inventory = new List<string>();
        collectedCollectibles = new List<string>();
    }
}