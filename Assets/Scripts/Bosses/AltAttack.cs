using System.Collections;
using UnityEngine;

public class AltAttack : BossAction
{
    public override IEnumerator ActionCoroutine(BossController boss, float duration)
    {
        active = true;

        //if the boss's weapon is null,
        //then we need to throw an error
        //because it should be null.
        if (boss.weapon != null)
        {
            if (boss.animator != null)
            {
                //set the attack trigger animation
                //so we do the attack animation.
                boss.animator.SetTrigger("altAttack");
            }
            
            boss.weapon.AltAttack();

            //Do the bosses attack coroutine.
            //yield return boss.weapon.AltAttackCoroutine();

            //wait for the animation to finish before 
            //exiting this coroutine.
            yield return LDUtil.WaitForAnimationFinish(boss.animator);
        }
        else
        {
            Debug.LogError("The boss has no weapon equipped, please ensure there is one equipped.");
        }

        //Wait for the bosses weapon attack
        //to end so we wait here until the boss is allowed to attack
        //again, AKA when it has finished executing the attack.
        //This prevents us from exiting an attack early.
        while (boss.weapon.isAttacking)
        {
            yield return null;
        }


        //DashAwayMove dashAwayMove = new DashAwayMove();

        //boss.SwitchToIdle(0f);

        //yield return null;

        

        //return the dash away coroutine so we
        //just reuse the dash away move at the end of our attack.
        //yield return dashAwayMove.ActionCoroutine(boss, duration);

        active = false;

        //say this was executed.
        didExecute = true;
    }
}
