using System.Collections;
using UnityEngine;

public class DualCannonLaserAttack : BossAction{
    private BossActionMaterials materials;
    private int numProjectiles = 4;
    public DualCannonLaserAttack(BossActionMaterials mats){
        materials = mats;
    }
    public override IEnumerator ActionCoroutine(BossController boss, float duration){


        
        
        for (int i = 0; i < numProjectiles; i++){
            boss.manualRotation = true;

            //Look at the player before firing.
            boss.LookAtPlayer();

            //Shoot both cannons.
            GameObject.Instantiate(materials.projectiles[1], boss.launchTransform.position - (boss.launchTransform.right * 0.9f), boss.launchTransform.rotation);
            GameObject.Instantiate(materials.projectiles[1], boss.launchTransform.position + (boss.launchTransform.right * 0.9f), boss.launchTransform.rotation);

            Debug.Break();

            boss.manualRotation = false;

            yield return new WaitForSeconds(1);
        }

        Debug.LogWarning("FINISHED LASER");
        yield break;
    }
}
