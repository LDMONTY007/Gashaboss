using System.Collections;
using UnityEngine;

public class MeleeAttack : BossAction
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
                boss.animator.SetTrigger("attack");
            }
            boss.weapon.Attack();

            //wait for the animation to finish before 
            //exiting this coroutine.
            yield return LDUtil.WaitForAnimationFinish(boss.animator);
        }
        else
        {
            Debug.LogError("The boss has no weapon equipped, please ensure there is one equipped.");
        }

       

        DashAwayMove dashAwayMove = new DashAwayMove();

        //boss.SwitchToIdle(0f);

        //yield return null;

        

        //return the dash away coroutine so we
        //just reuse the dash away move at the end of our attack.
        yield return dashAwayMove.ActionCoroutine(boss, duration);

        active = false;
    }


}
