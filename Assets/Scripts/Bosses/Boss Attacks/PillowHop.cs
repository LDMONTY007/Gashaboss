using System.Collections;
using UnityEngine;

public class PillowHop : BossAction{
    private BossActionMaterials materials;
    public PillowHop(BossActionMaterials mats){
        materials = mats;
    }
    public override IEnumerator ActionCoroutine(BossController boss, float duration){
        // Hand set y rotation for the projectile, use quaternity rotation
        Vector3 spawnPos = boss.GetFeetPosition();
        Quaternion spawnRotation = Quaternion.identity;
        spawnShockwaveSet(4, spawnPos, spawnRotation);
        spawnRotation = spawnRotation * Quaternion.Euler(0, 45f, 0);
        spawnShockwaveSet(4, spawnPos, spawnRotation);
        yield break;
    }
    
private void spawnShockwaveSet(int numProjectiles, Vector3 spawnPos, Quaternion baseRotation){
    for (int i = 0; i < numProjectiles; i++){   
        float angle = (360f / numProjectiles) * i;
        Quaternion rotation = baseRotation * Quaternion.Euler(0, angle, 0);
        GameObject.Instantiate(materials.projectiles[0], spawnPos, rotation);
    }
}
}
