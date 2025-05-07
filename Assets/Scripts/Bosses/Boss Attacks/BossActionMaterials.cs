using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossActionMaterials", menuName = "Boss Action Materials")]
public class BossActionMaterials: ScriptableObject{
    public List<GameObject> projectiles = new();
}