using System.Collections;
using UnityEngine;

public class LaserAttack : BossAction{
    private BossActionMaterials materials;
    private numProjectiles = 4;
    public LaserAttack(BossActionMaterials mats){
        materials = mats;
    }
    public override IEnumerator ActionCoroutine(BossController boss, float duration){
        // Hand set y rotation for the projectile, use quaternity rotation
        Vector3 spawnPos = boss.launchTransform.position;
        for (int i = 0; i < numProjectiles; i++){
            GameObject.Instantiate(materials.projectiles[1], spawnPos, Quaternion.identity);
            return yield new WaitForSeconds(1);
        }
        yield break;
    }
}
