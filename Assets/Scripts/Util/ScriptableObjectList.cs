using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjectList", menuName = "Scriptable Object List")]
public class ScriptableObjectList : ScriptableObject
{
    public List<ScriptableObject> soList = new();
}
