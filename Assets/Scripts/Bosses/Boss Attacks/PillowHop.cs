using System.Collections;
using UnityEngine;

public class PillowHop : BossAction
{
    public override IEnumerator ActionCoroutine(BossController boss, float duration)
    {
        active = true;
        if (boss.animator != null){
            //set the trigger animation for jumping
            boss.animator.SetTrigger("GroundPound");
        }
        // Check for collisions during animation?
        // (Does this natively happen?, or do I need to build a weapon class for this as an attack?)
        // (Should I move the model? Or does the animation do this?)
        // Wait for animation to finish
        yield return LDUtil.WaitForAnimationFinish(boss.animator);
        PlayShockwave();
        active = false;
    }

    public void PlayShockwave(){
        // Thinking of making a projectile attack... 
        // Oh, maybe I can just add a projectile weapon, that launches multiple projectiles? 
        // Like, The first can just be an AOE projectile that and then the rest spreading "Shockwaves"

        // Undercurrent setup, I need to add another weapon script to the boss?
        // That means the boss needs a list of weapon scripts, for all possible attacks
        // I'll need to add the logic for multiple attacks/attack switching as well, 
        // this will be determined on a by-boss basis though right?
        // Will need a seperate component at min for each boss
        // To tell the boss under which conditions to use each attack, 
        // as not only will conditions for the attacs change
        // but the possible moves will change as well (Can't guarntee that every boss will have a melee attack?)
        
        // Alternatively, we could for the moment assume boss's have exactly two attacks
        // A special which, may or may not exist
        // And then a basic melee attack
    }
}
