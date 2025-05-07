using System.Collections;
using UnityEngine;

public class MechaRexLaser : BossAction{
    private BossActionMaterials materials;
    public bool isRunning = false;

    public MechaRexLaser(BossActionMaterials mats){
        materials = mats;
    }
    public override IEnumerator ActionCoroutine(BossController boss, float duration){

        //turn on manual rotation
        //so it isn't overwritten by the velocity.
        boss.manualRotation = true;

        yield return new WaitForFixedUpdate();
        //Start looking at the player.
        boss.LookAtPlayer();
        yield return new WaitForFixedUpdate();

        Debug.Log(boss.rb.rotation.eulerAngles.ToString());

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

        //Return to velocity based rotation
        boss.manualRotation = false;

        Debug.LogWarning("END");

        yield break;
    }
}
