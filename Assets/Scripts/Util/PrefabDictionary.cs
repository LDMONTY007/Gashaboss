using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabDictionary", menuName = "Prefab Dictionary")]
public class PrefabDictionary : ScriptableObject
{
    public List<DropData> dropDataDictionary = new();
}
