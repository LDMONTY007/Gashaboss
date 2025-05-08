using System.Collections;
using UnityEngine;

public class LaserAttack : BossAction{
    private BossActionMaterials materials;
    private int numProjectiles = 4;
    public LaserAttack(BossActionMaterials mats){
        materials = mats;
    }
    public override IEnumerator ActionCoroutine(BossController boss, float duration){


        
        // Hand set y rotation for the projectile, use quaternity rotation
        Vector3 spawnPos = boss.launchTransform.position;
        for (int i = 0; i < numProjectiles; i++){
            boss.manualRotation = true;

            //Look at the player before firing.
            boss.LookAtPlayer();

            GameObject.Instantiate(materials.projectiles[1], spawnPos, boss.launchTransform.rotation);

            boss.manualRotation = false;

            yield return new WaitForSeconds(1);
        }

        Debug.LogWarning("FINISHED LASER");
        yield break;
    }
}
