// Used to contain the drop stats for the drop tables for GachaMachine.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DropData", menuName = "Drop Data")]
public class DropData: ScriptableObject{
    public GameObject droppedObject;
    public int weight;
    public int cost;
    public bool isRemovedAfterDrop;
    //these are for things like a new gashapon machine.
    public bool isEnvironmentalUnlock;
    [HideInInspector]
    public bool removeFromDropList;
}