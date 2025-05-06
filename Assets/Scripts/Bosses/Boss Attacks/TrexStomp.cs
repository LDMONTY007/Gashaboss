using System.Collections;
using UnityEngine;

public class TrexStomp : BossAction{
    private BossActionMaterials materials;
    public TrexStomp(BossActionMaterials mats){
        materials = mats;
    }
    public override IEnumerator ActionCoroutine(BossController boss, float duration){
        // Hand set y rotation for the projectile, use quaternity rotation
        Debug.Log("Reached the Action Call");
        Vector3 spawnPos = boss.GetFeetPosition();
        Quaternion spawnRotation = Quaternion.identity;
        spawnShockwaveSet(4, spawnPos, spawnRotation, boss);
        yield break;
    }
    
    private void spawnShockwaveSet(int numProjectiles, Vector3 spawnPos, Quaternion baseRotation, BossController boss){
        Debug.Log("Trying to spawn projectiles");
        for (int i = 0; i < numProjectiles; i++){   
            float angle = (360f / numProjectiles) * i;
            Quaternion rotation = baseRotation * Quaternion.Euler(0, angle, 0);
            GameObject.Instantiate(materials.projectiles[2], spawnPos, rotation);
        }
    }
}
