using System.Collections;
using UnityEngine;

public class MechaRexLaser : BossAction{
    private BossActionMaterials materials;
    private int numProjectiles = 4;
    public bool isRunning = false;

    public MechaRexLaser(BossActionMaterials mats){
        materials = mats;
    }
    public override IEnumerator ActionCoroutine(BossController boss, float duration){

        
        Debug.LogWarning("START LASER ATTACK");
        
        //Tell the boss to start it's laser animation, 
        //it'll crouch down and open it's mouth to start playing the
        //charge up laser particles. 
        //Don't let the player get near it during this part.
        LaserTrexBossController Mechasaur = boss as LaserTrexBossController;

        //Fire the Mechasaur laser.
        Mechasaur.laserAnimator.SetBool("Fire", true);

        //Wait 2.2 seconds for the charge up animation.
        yield return new WaitForSeconds(2.20f);
        Debug.LogWarning("CHARGED");

        //The Actual laser firing starts now.
        //Wait .25 seconds for the laser to do it's fire animation
        yield return new WaitForSeconds(.25f);
        Debug.LogWarning("FIRED");

        //The laser is now firing continuously.
        //Wait however long you want here and then set "Fire" to false so the attack ends.
        yield return new WaitForSeconds(1f);
        Mechasaur.laserAnimator.SetBool("Fire", false);

        //Wait .45 seconds to let the StopFire animation complete
        //before ending this attack.
        yield return new WaitForSeconds(0.45f);

        //Wait an additional 1 second before exiting this attack.
        yield return new WaitForSeconds(1f);

        // Hand set y rotation for the projectile, use quaternity rotation
        /*        Vector3 spawnPos = boss.launchTransform.position;
                for (int i = 0; i < numProjectiles; i++){
                    GameObject.Instantiate(materials.projectiles[1], spawnPos, boss.launchTransform.rotation);
                    yield return new WaitForSeconds(1);
                }*/

        Debug.LogWarning("END");

        yield break;
    }
}
