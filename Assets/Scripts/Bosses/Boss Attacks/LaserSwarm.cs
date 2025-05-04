using System.Collections;
using UnityEngine;

public class LaserSwarm : BossAction{
    private BossActionMaterials materials;
    public LaserSwarm(BossActionMaterials mats){
        materials = mats;
    }
    public override IEnumerator ActionCoroutine(BossController boss, float duration){
        // Hand set y rotation for the projectile, use quaternity rotation
        Debug.Log("Reached the Action Call");
        Vector3 spawnPos = boss.GetFeetPosition() + new Vector3(0,2f,0);
        Quaternion spawnRotation = Quaternion.identity;
        spawnShockwaveSet(16, spawnPos, spawnRotation, boss);
        yield return new WaitForSeconds(1f);
        spawnRotation = spawnRotation * Quaternion.Euler(0, 45f, 0);
        spawnShockwaveSet(16, spawnPos, spawnRotation, boss);
        yield break;
    }
    
    private void spawnShockwaveSet(int numProjectiles, Vector3 spawnPos, Quaternion baseRotation, BossController boss){
        Debug.Log("Trying to spawn projectiles");
        for (int i = 0; i < numProjectiles; i++){   
            float angle = (360f / numProjectiles) * i;
            Quaternion rotation = baseRotation * Quaternion.Euler(0, angle, 0);
            GameObject.Instantiate(materials.projectiles[1], spawnPos, rotation);
        }
    }
}
