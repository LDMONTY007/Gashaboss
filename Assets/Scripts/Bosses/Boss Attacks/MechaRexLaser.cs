using System.Collections;
using UnityEngine;

public class MechaRexLaser : BossAction{
    private BossActionMaterials materials;
    private int numProjectiles = 4;
    public MechaRexLaser(BossActionMaterials mats){
        materials = mats;
    }
    public override IEnumerator ActionCoroutine(BossController boss, float duration){
        //Tell the boss to start it's laser animation, 
        //it'll crouch down and open it's mouth to start playing the
        //charge up laser particles. 
        //Don't let the player get near it during this part.
        
        // Hand set y rotation for the projectile, use quaternity rotation
        Vector3 spawnPos = boss.launchTransform.position;
        for (int i = 0; i < numProjectiles; i++){
            GameObject.Instantiate(materials.projectiles[1], spawnPos, boss.launchTransform.rotation);
            yield return new WaitForSeconds(1);
        }
        yield break;
    }
}
