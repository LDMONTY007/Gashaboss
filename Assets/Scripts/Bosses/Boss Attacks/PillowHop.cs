using System.Collections;
using UnityEngine;

public class PillowHop : BossAction
{
    public override IEnumerator ActionCoroutine(BossController boss, float duration)
    {
        active = true;
        if (boss.animator != null){
            //set the trigger animation for juming
            boss.animator.SetTrigger("jump");
        }
        // Wait for animation to finish
        yield return LDUtil.WaitForAnimationFinish(boss.animator);
        PlayShockwave();
        active = false;
    }

    public void PlayShockwave(){
        instantiate()
    }


}
