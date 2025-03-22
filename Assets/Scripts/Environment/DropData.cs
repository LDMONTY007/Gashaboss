// Used to contain the drop stats for the drop tables for GachaMachine.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DropData", menuName = "Drop Data")]
public class DropData: ScriptableObject{
    public GameObject droppedObject;
    public int weight;
    public bool isRemovedAfterDrop;
}